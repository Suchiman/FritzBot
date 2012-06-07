using System;
using System.Collections.Generic;
using System.Threading;

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
            LaborDaten = new Labordaten[boxdata.Count];
            laborthread = new Thread(new ThreadStart(this.labor_check));
            laborthread.Name = "LaborThread";
            laborthread.IsBackground = true;
            laborthread.Start();
            LaborDatenUpdate = DateTime.MinValue;
        }

        public void Destruct()
        {
            laborthread.Abort();
        }

        private Labordaten[] LaborDaten;
        private DateTime LaborDatenUpdate;
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
                LaborDaten[i - 1].daten = datumsection[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                String changelog_url_element = datumsection[i].Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];
                String titel = datumsection[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[1].Split(new String[] { "<img style=" }, 2, StringSplitOptions.None)[0].Replace("\r\n\t \t\t", "");
                titel = titel.Remove(titel.IndexOf("\r\n"));
                if (titel.Contains("Offiziell") || titel.Contains("Regulär") || titel.Contains("Update") || changelog_url_element.Contains("Die neuen Leistungsmerkmale"))
                {
                    LaborDaten[i - 1].daten = "Released";
                }
                else
                {
                    changelog_url_element = changelog_url_element.Split(new String[] { "<a href=" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\"" }, 3, StringSplitOptions.None)[1];
                    LaborDaten[i - 1].url = "http://www.avm.de/de/Service/Service-Portale/Labor/" + changelog_url_element;
                    String url = LaborDaten[i - 1].url.Remove(LaborDaten[i - 1].url.LastIndexOf('/')) + "/labor_feedback_versionen.php";
                    String feedback = toolbox.GetWeb(url);
                    if (String.IsNullOrEmpty(feedback))
                    {
                        throw new InvalidOperationException("Verbindungsfehler");
                    }
                    LaborDaten[i - 1].url = toolbox.ShortUrl(LaborDaten[i - 1].url);
                    LaborDaten[i - 1].version = feedback.Split(new String[] { "</strong>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Version " }, 2, StringSplitOptions.None)[1];
                }
            }
            LaborDatenUpdate = DateTime.Now;
        }

        public void Run(ircMessage theMessage)
        {
            String changeset = "";
            int modell = 0;
            try
            {
                update_labor_cache();
            }
            catch (Exception ex)
            {
                if (LaborDatenUpdate == DateTime.MinValue)
                {
                    theMessage.Answer("Ich konnte leider keine Daten von der Laborwebseite abrufen und mein Cache ist leer");
                    return;
                }
                theMessage.Answer("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + ex.Message + ". Verwende Cache vom " + LaborDatenUpdate.ToString());
            }
            if (boxdata.ContainsKey(theMessage.CommandLine.ToLower()))
            {
                modell = boxdata[theMessage.CommandLine.ToLower()];
                if (LaborDaten[modell - 1].daten == "Released")
                {
                    theMessage.Answer("Aktuell ist keine Laborversion verfügbar da die Features in eine neue Release Firmware eingeflossen sind");
                }
                else
                {
                    changeset += "Die neueste " + theMessage.CommandLine + " labor Version ist am " + LaborDaten[modell - 1].daten + " erschienen mit der Versionsnummer: " + LaborDaten[modell - 1].version + ". Laborseite: " + LaborDaten[modell - 1].url;
                }
            }
            else if (String.IsNullOrEmpty(theMessage.CommandLine.ToLower()))
            {
                for (int i = 0; i < boxdatareverse.Count; i++)
                {
                    changeset += ", " + boxdatareverse[i + 1] + ": " + LaborDaten[i].daten;
                }
                changeset = "Aktuelle Labor Daten: " + changeset.Remove(0, 2) + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            }
            else
            {
                changeset += "Für die " + theMessage.CommandLine + " steht derzeit keine Labor Version zur Verfügung. ";
            }
            theMessage.Answer(changeset);
        }

        private void labor_check()
        {
            Labordaten[] labor_old = null;
            String output = "";
            while (true)
            {
                if (Properties.Settings.Default.LaborCheck)
                {
                    do
                    {
                        try
                        {
                            update_labor_cache();
                            if (labor_old == null)
                            {
                                labor_old = new Labordaten[boxdata.Count];
                                LaborDaten.CopyTo(labor_old, 0);
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                            if (ex is System.IndexOutOfRangeException)
                            {
                                Program.TheServers.AnnounceGlobal("Mir ist bei der Verarbeitung der AVM Labor Webseite ein Fehler unterlaufen der typisch dafür ist, dass AVM etwas hinzugefügt oder entfernt hat. Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
                                return;
                            }
                        }
                    } while (true);
                    String released = "";
                    String labors = "";
                    for (int i = 0; i < boxdata.Count; i++)
                    {
                        if (LaborDaten[i] != labor_old[i])
                        {
                            if (LaborDaten[i].daten == "Released")
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
                        LaborDaten.CopyTo(labor_old, 0);
                        Program.TheServers.AnnounceGlobal(output);
                    }
                    output = "";
                    Thread.Sleep(Properties.Settings.Default.LaborCheckIntervall);
                }
                else
                {
                    Thread.Sleep(30000);
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