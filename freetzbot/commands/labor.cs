using System;
using System.Collections.Generic;
using System.Threading;

namespace freetzbot.commands
{
    class labor : command
    {
        private String[] name = { "labor" };
        private String helptext = "Ich schaue mal auf das aktuelle Datum der Labor Firmwares, Parameter: '7270', '7390', 'fhem', '7390at', 'android', 'ios'.";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
        private Boolean accept_every_param = true;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public labor()
        {
            labor_daten = new labordaten[7];
            boxdata = new Dictionary<String, int>();
            boxdatareverse = new Dictionary<int, String>();
            boxdata.Add("ios", 1);
            boxdata.Add("android", 2);
            boxdata.Add("7390", 3);
            boxdata.Add("fhem", 4);
            boxdata.Add("7390at", 5);
            boxdata.Add("7320", 6);
            boxdata.Add("7270", 7);
            boxdatareverse.Add(1, "iOS App");
            boxdatareverse.Add(2, "Android App");
            boxdatareverse.Add(3, "7390");
            boxdatareverse.Add(4, "7390 FHEM");
            boxdatareverse.Add(5, "7390 AT-CH");
            boxdatareverse.Add(6, "7320");
            boxdatareverse.Add(7, "7270");
            Thread laborthread = new Thread(() => { this.labor_check(); });
            laborthread.IsBackground = true;
            laborthread.Start();
        }

        private labordaten[] labor_daten;
        private Dictionary<String, int> boxdata;
        private Dictionary<int, String> boxdatareverse;
        
        private void update_labor_cache()
        {
            String webseite = toolbox.get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            if (webseite == "")
            {
                throw new Exception("Verbindungsfehler");
            }
            for (int i = 1; i < 8; i++)
            {
                labor_daten[i - 1].daten = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                String changelog_url_element = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i];
                changelog_url_element = changelog_url_element.Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];
                if (changelog_url_element.Contains("<p>Die neuen Leistungsmerkmale aus diesem Labor"))
                {
                    labor_daten[i - 1].daten = "Released";
                }
                else
                {
                    changelog_url_element = changelog_url_element.Split(new String[] { "<a href=" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\"" }, 3, StringSplitOptions.None)[1].Split(new String[] { "/" }, 2, StringSplitOptions.None)[0];
                    labor_daten[i - 1].url = "http://www.avm.de/de/Service/Service-Portale/Labor/" + changelog_url_element + "/labor_feedback_versionen.php";
                    String feedback = toolbox.get_web(labor_daten[i - 1].url);
                    if (feedback == "")
                    {
                        throw new Exception("Verbindungsfehler");
                    }
                    labor_daten[i - 1].version = feedback.Split(new String[] { "</strong>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Version " }, 2, StringSplitOptions.None)[1];
                }
            }
        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            String changeset = "";
            int modell = 0;
            try
            {
                update_labor_cache();
            }
            catch (Exception ex)
            {
                connection.sendmsg("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + ex.Message + ". Verwende Cache", receiver);
            }
            if (boxdata.ContainsKey(message.ToLower()))
            {
                modell = boxdata[message.ToLower()];
                if (labor_daten[modell].daten == "Released")
                {
                    connection.sendmsg("Aktuell ist keine Laborversion verfügbar da die Features in eine neue Release Firmware eingeflossen sind", receiver);
                }
                else
                {
                    changeset += "Die neueste " + message + " labor Version ist am " + labor_daten[modell].daten + " erschienen mit der Versionsnummer: " + labor_daten[modell].version + ". Changelog: " + labor_daten[modell].url;
                }
            }
            else if (message.ToLower() == "")
            {
                changeset = "Aktuelle Labor Daten: iOS: " + labor_daten[0].daten + ", Android: " + labor_daten[1].daten + ", 7390: " + labor_daten[2].daten + ", FHEM: " + labor_daten[3].daten + ", 7390at: " + labor_daten[4].daten + ", 7320: " + labor_daten[5].daten + ", 7270: " + labor_daten[6].daten + " - http://www.avm.de/de/Service/Service-Portale/Labor/index.php";
            }
            else
            {
                changeset += "Für die " + message + " steht derzeit keine Labor Version zur Verfügung. ";
            }
            connection.sendmsg(changeset, receiver);
        }

        private void labor_check()
        {
            labordaten[] labor_old = labor_daten;
            String output = "";
            while (true)
            {
                if (freetzbot.Program.configuration.get("labor_check") == "false")
                {
                    Thread.Sleep(30000);
                }
                while (freetzbot.Program.configuration.get("labor_check") == "true")
                {
                    do
                    {
                        try
                        {
                            update_labor_cache();
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                    } while (true);
                    String released = "";
                    String labors = "";
                    for (int i = 1; i < 8; i++)
                    {
                        if (labor_daten[i - 1].daten != labor_old[i - 1].daten)
                        {
                            if (labor_daten[i - 1].daten == "Released")
                            {
                                released += ", " + boxdatareverse[i];
                            }
                            else
                            {
                                labors += ", " + boxdatareverse[i];
                            }
                        }
                    }
                    if (released != "")
                    {
                        output = "Labor Version wurde als neue Firmware released!" + released.Remove(0, 2);
                    }
                    if (labors != "")
                    {
                        if (output != "")
                        {
                            output += ", Neue Labor Versionen gesichtet!" + labors.Remove(0, 2);
                        }
                        else
                        {
                            output += "Neue Labor Versionen gesichtet!" + labors.Remove(0, 2);
                        }
                    }
                    if (output != "")
                    {
                        output += " - http://www.avm.de/de/Service/Service-Portale/Labor/index.php";
                        toolbox.announce(output);
                    }
                    output = "";
                    int labor_check_intervall;
                    if (!int.TryParse(freetzbot.Program.configuration.get("labor_check_intervall"), out labor_check_intervall))
                    {
                        labor_check_intervall = 300000;
                    }
                    Thread.Sleep(labor_check_intervall);
                }
            }
        }
    }
}
namespace freetzbot
{
    public struct labordaten
    {
        public String daten;
        public String version;
        public String url;
    }
}