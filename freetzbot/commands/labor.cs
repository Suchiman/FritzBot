using System;
using System.Collections.Generic;
using System.Threading;
using FritzBot;

namespace FritzBot.commands
{
    class labor : ICommand
    {
        public String[] Name { get { return new String[] { "labor" }; } }
        public String HelpText { get { return "Ich schaue mal auf das aktuelle Datum der Labor Firmwares, Parameter: '7270', '7390', 'fhem', '7390at', 'android', 'ios'."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public labor()
        {
            boxdata = new Dictionary<String, int>()
            {
                {"ios", 1},
                {"android", 2},
                {"7390", 3},
                {"fhem", 4},
                //{"7390at", 5},
                //{"7320", 6},
                {"7270", 5}
            };
            boxdatareverse = new Dictionary<int, String>()
            {
                {1, "iOS App"},
                {2, "Android App"},
                {3, "7390"},
                {4, "7390 FHEM"},
                //{5, "7390 AT-CH"},
                //{6, "7320"},
                {5, "7270"}
            };
            labor_daten = new Labordaten[boxdata.Count];
            laborthread = new Thread(new ThreadStart(this.labor_check));
            laborthread.Name = "LaborThread";
            laborthread.IsBackground = true;
            laborthread.Start();
        }

        public void Destruct()
        {
            laborthread.Abort();
        }

        private Labordaten[] labor_daten;
        private Dictionary<String, int> boxdata;
        private Dictionary<int, String> boxdatareverse;
        Thread laborthread;

        private void update_labor_cache()
        {
            String webseite = toolbox.GetWeb("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            if (String.IsNullOrEmpty(webseite))
            {
                throw new InvalidOperationException("Verbindungsfehler");
            }
            String[] datumsection = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None);
            for (int i = 1; i <= boxdata.Count; i++)
            {
                labor_daten[i - 1].daten = datumsection[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                String changelog_url_element = datumsection[i].Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];//.Replace("\r\n\t\t\t\r\n\t\t\t\t", "");
                String titel = datumsection[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[1].Split(new String[] { "<img style=" }, 2, StringSplitOptions.None)[0].Replace("\r\n\t \t\t", "");
                titel = titel.Remove(titel.IndexOf("\r\n"));
                if (changelog_url_element.Contains("<p>Die neuen Leistungsmerkmale aus diesem Labor") || titel == "Reguläres Update verfügbar")
                {
                    labor_daten[i - 1].daten = "Released";
                }
                else
                {
                    changelog_url_element = changelog_url_element.Split(new String[] { "<a href=" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\"" }, 3, StringSplitOptions.None)[1];
                    labor_daten[i - 1].url = "http://www.avm.de/de/Service/Service-Portale/Labor/" + changelog_url_element;
                    String url = labor_daten[i - 1].url.Remove(labor_daten[i - 1].url.LastIndexOf('/')) + "/labor_feedback_versionen.php";
                    String feedback = toolbox.GetWeb(url);
                    if (String.IsNullOrEmpty(feedback))
                    {
                        throw new InvalidOperationException("Verbindungsfehler");
                    }
                    labor_daten[i - 1].url = toolbox.ShortUrl(labor_daten[i - 1].url);
                    labor_daten[i - 1].version = feedback.Split(new String[] { "</strong>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Version " }, 2, StringSplitOptions.None)[1];
                }
            }
        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String changeset = "";
            int modell = 0;
            try
            {
                update_labor_cache();
            }
            catch (Exception ex)
            {
                connection.Sendmsg("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + ex.Message + ". Verwende Cache", receiver);
            }
            if (boxdata.ContainsKey(message.ToLower()))
            {
                modell = boxdata[message.ToLower()];
                if (labor_daten[modell - 1].daten == "Released")
                {
                    connection.Sendmsg("Aktuell ist keine Laborversion verfügbar da die Features in eine neue Release Firmware eingeflossen sind", receiver);
                }
                else
                {
                    changeset += "Die neueste " + message + " labor Version ist am " + labor_daten[modell - 1].daten + " erschienen mit der Versionsnummer: " + labor_daten[modell - 1].version + ". Laborseite: " + labor_daten[modell - 1].url;
                }
            }
            else if (String.IsNullOrEmpty(message.ToLower()))
            {
                for (int i = 0; i < boxdatareverse.Count; i++)
                {
                    changeset += ", " + boxdatareverse[i + 1] + ": " + labor_daten[i].daten;
                }
                changeset = "Aktuelle Labor Daten: " + changeset.Remove(0, 2) + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            }
            else
            {
                changeset += "Für die " + message + " steht derzeit keine Labor Version zur Verfügung. ";
            }
            connection.Sendmsg(changeset, receiver);
        }

        private void labor_check()
        {
            Labordaten[] labor_old = null;
            String output = "";
            while (true)
            {
                if (Program.configuration["labor_check"] == "false")
                {
                    Thread.Sleep(30000);
                }
                while (Program.configuration["labor_check"] == "true")
                {
                    do
                    {
                        try
                        {
                            update_labor_cache();
                            if (labor_old == null)
                            {
                                labor_old = new Labordaten[boxdata.Count];
                                labor_daten.CopyTo(labor_old, 0);
                            }
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                    } while (true);
                    String released = "";
                    String labors = "";
                    for (int i = 0; i < boxdata.Count; i++)
                    {
                        if (labor_daten[i] != labor_old[i])
                        {
                            if (labor_daten[i].daten == "Released")
                            {
                                released += ", " + boxdatareverse[i + 1];
                            }
                            else
                            {
                                labors += ", " + boxdatareverse[i + 1];
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(released))
                    {
                        output = "Labor Version wurde als neue Firmware released! -" + released.Remove(0, 1);
                    }
                    if (!String.IsNullOrEmpty(labors))
                    {
                        if (!String.IsNullOrEmpty(output))
                        {
                            output += ", Neue Labor Versionen gesichtet! -" + labors.Remove(0, 1);
                        }
                        else
                        {
                            output += "Neue Labor Versionen gesichtet! -" + labors.Remove(0, 1);
                        }
                    }
                    if (!String.IsNullOrEmpty(output))
                    {
                        output += " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
                        labor_daten.CopyTo(labor_old, 0);
                        toolbox.Announce(output);
                    }
                    output = "";
                    int labor_check_intervall;
                    if (!int.TryParse(Program.configuration["labor_check_intervall"], out labor_check_intervall))
                    {
                        labor_check_intervall = 300000;
                    }
                    Thread.Sleep(labor_check_intervall);
                }
            }
        }
    }
}
namespace FritzBot
{
    struct Labordaten : IEquatable<Labordaten>
    {
        public String daten;
        public String version;
        public String url;
        
        public override int GetHashCode()
        {
            return daten.GetHashCode() ^ version.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Labordaten))
                return false;

            return Equals((Labordaten)obj);
        }  

        public bool Equals(Labordaten other)
        {
            if (daten != other.daten)
                return false;

            return version == other.version;    
        }

        public static bool operator ==(Labordaten labordaten1, Labordaten labordaten2)
        {
            return labordaten1.Equals(labordaten2);
        }

        public static bool operator !=(Labordaten labordaten1, Labordaten labordaten2)
        {
            return !labordaten1.Equals(labordaten2);
        }    
    }
}