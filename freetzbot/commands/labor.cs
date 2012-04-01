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
            laborthread = new Thread(new ThreadStart(labor_check));
            laborthread.IsBackground = true;
            laborthread.Start();
        }

        Thread laborthread = new Thread(new ThreadStart(labor_check));

        public void run(irc connection, String sender, String receiver, String message)
        {
            String webseite = toolbox.get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            if (webseite == "")
            {
                connection.sendmsg("Leider war es mir nicht möglich auf die Labor Webseite von AVM zuzugreifen", receiver);
                return;
            }

            String changeset = "";
            int modell = 0;
            switch (message.ToLower())
            {
                case "ios":
                    modell = 1;
                    break;
                case "android":
                    modell = 2;
                    break;
                case "7390":
                    modell = 3;
                    break;
                case "fhem":
                    modell = 4;
                    break;
                case "7390at":
                    modell = 5;
                    break;
                case "7320":
                    modell = 6;
                    break;
                case "7270":
                    modell = 7;
                    break;
                case "":
                    String[] daten = new String[7];
                    for (int i = 1; i < 8; i++)
                    {
                        daten[i - 1] = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];

                        String changelog_url_element = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i];
                        changelog_url_element = changelog_url_element.Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];
                        if (changelog_url_element.Contains("<p>Die neuen Leistungsmerkmale aus diesem Labor"))
                        {
                            daten[i - 1] = "Released";
                        }
                    }
                    changeset = "Aktuelle Labor Daten: iOS: " + daten[0] + ", Android: " + daten[1] + ", 7390: " + daten[2] + ", FHEM: " + daten[3] + ", 7390at: " + daten[4] + ", 7320: " + daten[5] + ", 7270: " + daten[6] + " - http://www.avm.de/de/Service/Service-Portale/Labor/index.php";
                    break;
                default:
                    changeset += "Für die " + message + " steht derzeit keine Labor Version zur Verfügung. ";
                    break;
            }

            if (modell != 0)
            {
                String datum = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[modell].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                String changelog_url_element = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[modell];
                changelog_url_element = changelog_url_element.Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];
                if (changelog_url_element.Contains("<p>Die neuen Leistungsmerkmale aus diesem Labor"))
                {
                    connection.sendmsg("Aktuell ist keine Laborversion verfügbar da die Features in eine neue Release Firmware eingeflossen sind", receiver);
                }
                else
                {
                    changelog_url_element = changelog_url_element.Split(new String[] { "<a href=" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\"" }, 3, StringSplitOptions.None)[1].Split(new String[] { "/" }, 2, StringSplitOptions.None)[0];
                    String url = "http://www.avm.de/de/Service/Service-Portale/Labor/" + changelog_url_element + "/labor_feedback_versionen.php";
                    String feedback = toolbox.get_web(url);
                    if (feedback == "")
                    {
                        connection.sendmsg("Leider war es mir nicht möglich alle Daten von der AVM Webseite abzurufen", receiver);
                        return;
                    }
                    String version = feedback.Split(new String[] { "</strong>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Version " }, 2, StringSplitOptions.None)[1];
                    changeset += "Die neueste " + message + " labor Version ist am " + datum + " erschienen mit der Versionsnummer: " + version + ". Changelog: " + url;
                }
            }
            connection.sendmsg(changeset, receiver);
        }

        private static void labor_check()
        {
            List<String> LaborDates = new List<String>();
            Boolean[] released = new Boolean[7];
            String output = "";
            String webseite = toolbox.get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            for (int i = 1; i < 8; i++)
            {
                String Date = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                LaborDates.Add(Date);

                String changelog_url_element = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i];
                changelog_url_element = changelog_url_element.Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];

                if (changelog_url_element.Contains("<p>Die neuen Leistungsmerkmale aus diesem Labor"))
                {
                    released[i - 1] = true;
                }
            }
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
                        Thread.Sleep(1000);
                        webseite = toolbox.get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
                    } while (webseite == "" || webseite == null);
                    for (int i = 1; i < 8; i++)
                    {
                        String Date = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];

                        String changelog_url_element = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i];
                        changelog_url_element = changelog_url_element.Split(new String[] { "<div class=\"boxBottom\">" }, 2, StringSplitOptions.None)[0];

                        if (changelog_url_element.Contains("<p>Die neuen Leistungsmerkmale aus diesem Labor"))
                        {
                            if (released[i - 1] != true)
                            {
                                released[i - 1] = true;
                                output = "Labor Version wurde als Stabil released!";
                                switch (i)
                                {
                                    case 1:
                                        output += ", iOS App";
                                        break;
                                    case 2:
                                        output += ", Android App";
                                        break;
                                    case 3:
                                        output += ", 7390";
                                        break;
                                    case 4:
                                        output += ", 7390 FHEM";
                                        break;
                                    case 5:
                                        output += ", 7390 AT-CH";
                                        break;
                                    case 6:
                                        output += ", 7320";
                                        break;
                                    case 7:
                                        output += ", 7270";
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            released[i - 1] = false;
                        }

                        if (Date != LaborDates[i - 1])
                        {
                            LaborDates[i - 1] = Date;
                            if (output == "")
                            {
                                output += "Neue Labor Versionen gesichtet!";
                            }
                            switch (i)
                            {
                                case 1:
                                    output += ", iOS App";
                                    break;
                                case 2:
                                    output += ", Android App";
                                    break;
                                case 3:
                                    output += ", 7390";
                                    break;
                                case 4:
                                    output += ", 7390 FHEM";
                                    break;
                                case 5:
                                    output += ", 7390 AT-CH";
                                    break;
                                case 6:
                                    output += ", 7320";
                                    break;
                                case 7:
                                    output += ", 7270";
                                    break;
                                default:
                                    break;
                            }
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