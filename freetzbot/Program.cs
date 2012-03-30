using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace freetzbot
{
    class Program
    {
        static private System.ComponentModel.BackgroundWorker loggingthread;

        static private Boolean restart = false;
        static private String zeilen = Convert.ToString(74 + 170 + 313 + 2037);
        static private DateTime startzeit;
        static private List<string> logging_list = new List<string>();
        static private Mutex logging_safe = new Mutex();
        static private Mutex leave_safe = new Mutex();
        static private db boxdb = new db("box.db");
        static private db userdb = new db("user.db");
        static private db witzdb = new db("witze.db");
        static private db ignoredb = new db("ignore.db");
        static private db aliasdb = new db("alias.db");
        static private db fwdb = new db("fwdb.db");
        static private db servers = new db("servers.cfg");
        static private settings configuration = new settings("config.cfg");
        static private List<irc> irc_connections = new List<irc>();
        static private Thread laborthread;
        static private Thread antifloodingthread;
        static private int antifloodingcount;
        static private Boolean floodingnotificated;
        static private List<int> witz_randoms = new List<int>();

        static private void process_command(irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
            Boolean answered_user = true;
            Boolean answered_admin = true;

            #region Admin Befehle
            if (sender == "Suchiman" || sender == "hippie2000")
            {
                if (parameter.Length > 1) //Wenn ein zusätzlicher Parameter angegebenen wurde....
                {
                    switch (parameter[0].ToLower())
                    {
                        case "connect":
                            connect(connection, sender, receiver, parameter[1]);
                            break;
                        case "leave":
                            leave(parameter[1]);
                            break;
                        case "unignore":
                            unignore(parameter[1]);
                            connection.sendmsg("Alles klar", receiver);
                            break;
                        case "settings":
                            settings_command(connection, sender, receiver, parameter[1]);
                            break;
                        case "db":
                            db_command(connection, sender, receiver, parameter[1]);
                            break;
                        case "join":
                            connection.sendaction("rennt los zum channel " + parameter[1], receiver);
                            connection.join(parameter[1]);
                            break;
                        case "part":
                            connection.sendaction("verlässt den channel " + parameter[1], receiver);
                            connection.leave(parameter[1]);
                            break;
                        case "quit":
                            hilfe(connection, sender, receiver, "quit");
                            break;
                        case "restart":
                            hilfe(connection, sender, receiver, "restart");
                            break;
                        default:
                            answered_admin = false;
                            break;
                    }
                }
                else //Wenn kein zusätzlicher Parameter angegeben wurde....
                {
                    switch (parameter[0].ToLower())
                    {
                        case "connect":
                            hilfe(connection, sender, receiver, "connect");
                            break;
                        case "leave":
                            hilfe(connection, sender, receiver, "leave");
                            break;
                        case "unignore":
                            hilfe(connection, sender, receiver, "unignore");
                            break;
                        case "settings":
                            hilfe(connection, sender, receiver, "settings");
                            break;
                        case "db":
                            hilfe(connection, sender, receiver, "db");
                            break;
                        case "join":
                            hilfe(connection, sender, receiver, "join");
                            break;
                        case "part":
                            hilfe(connection, sender, receiver, "part");
                            break;
                        case "quit":
                            Trennen();
                            break;
                        case "restart":
                            restart = true;
                            Trennen();
                            break;
                        default:
                            answered_admin = false;
                            break;
                    }
                }
            }
            else
            {
                answered_admin = false;
            }
            #endregion

            #region Spezialfälle check
            if (ignore_check(sender)) return;
            if (parameter.Length > 1) if (ignore_check(parameter[1])) return;
            int floodingcount;
            if (!int.TryParse(configuration.get("floodingcount"), out floodingcount))
            {
                floodingcount = 5;//Default wert
            }
            if (antifloodingcount >= floodingcount)
            {
                if (floodingnotificated == false)
                {
                    floodingnotificated = true;
                    connection.sendmsg("Flooding Protection aktiviert", receiver);
                }
                return;
            }
            else
            {
                antifloodingcount++;
            }
            if (configuration.get("klappe") == "true") receiver = sender;
            #endregion

            #region Benutzerbefehle
            if (parameter.Length > 1)//Wenn ein zusätzlicher Parameter angegebenen wurde....
            {
                switch (parameter[0].ToLower())
                {
                    case "about":
                        connection.sendmsg("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Kleiner Scherz am Rande Ha-Ha. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit " + zeilen + " Zeilen. Entwickler: Suchiman", receiver);
                        break;
                    case "alias":
                    case "a":
                        alias(connection, sender, receiver, parameter[1]);
                        break;
                    case "box":
                        box(connection, sender, receiver, parameter[1]);
                        break;
                    case "boxfind":
                        boxfind(connection, sender, sender, parameter[1]);
                        break;
                    case "boxinfo":
                        boxinfo(connection, sender, receiver, parameter[1]);
                        break;
                    case "boxlist":
                        hilfe(connection, sender, receiver, "boxlist");
                        break;
                    case "boxremove":
                        boxremove(connection, sender, receiver, parameter[1]);
                        break;
                    case "frag":
                        frag(connection, sender, receiver, parameter[1]);
                        break;
                    case "freetz":
                    case "f":
                        freetz(connection, sender, receiver, parameter[1]);
                        break;
                    case "fw":
                        fw(connection, sender, receiver, parameter[1]);
                        break;
                    case "g":
                    case "google":
                        google(connection, sender, receiver, parameter[1]);
                        break;
                    case "help":
                    case "hilfe":
                    case "faq":
                    case "info":
                    case "man":
                        hilfe(connection, sender, receiver, parameter[1]);
                        break;
                    case "mem":
                        hilfe(connection, sender, receiver, "mem");
                        break;
                    case "ignore":
                        ignore(connection, sender, receiver, parameter[1]);
                        break;
                    case "labor":
                        labor(connection, sender, receiver, parameter[1]);
                        break;
                    case "lmgtfy":
                        lmgtfy(connection, sender, receiver, parameter[1]);
                        break;
                    case "ping":
                        hilfe(connection, sender, receiver, "ping");
                        break;
                    case "seen":
                        seen(connection, sender, receiver, parameter[1]);
                        break;
                    case "trunk":
                        hilfe(connection, sender, receiver, "trunk");
                        break;
                    case "uptime":
                    case "laufzeit":
                        hilfe(connection, sender, receiver, "uptime");
                        break;
                    case "userlist":
                        hilfe(connection, sender, receiver, "userlist");
                        break;
                    case "whmf":
                    case "w":
                        whmf(connection, sender, receiver, parameter[1]);
                        break;
                    case "witz":
                        witz(connection, sender, receiver, parameter[1]);
                        break;
                    case "zeit":
                        try
                        {
                            connection.sendmsg("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss") + ".", receiver);
                        }
                        catch
                        {
                            connection.sendmsg("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(", receiver);
                        }
                        break;
                    default:
                        answered_user = false;
                        break;
                }
            }
            else //Wenn kein zusätzlicher Parameter angegeben wurde....
            {
                switch (parameter[0].ToLower())
                {
                    case "about":
                        connection.sendmsg("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Kleiner Scherz am Rande Ha-Ha. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit " + zeilen + " Zeilen. Entwickler: Suchiman", receiver);
                        break;
                    case "alias":
                    case "a":
                        hilfe(connection, sender, receiver, "alias");
                        break;
                    case "box":
                        hilfe(connection, sender, receiver, "box");
                        break;
                    case "boxfind":
                        hilfe(connection, sender, receiver, "boxfind");
                        break;
                    case "boxinfo":
                        boxinfo(connection, sender, receiver, sender);
                        break;
                    case "boxlist":
                        boxlist(connection, sender, receiver, "");
                        break;
                    case "boxremove":
                        hilfe(connection, sender, receiver, "boxremove");
                        break;
                    case "frag":
                        hilfe(connection, sender, receiver, "frag");
                        break;
                    case "g":
                    case "google":
                        hilfe(connection, sender, receiver, "google");
                        break;
                    case "freetz":
                    case "f":
                        freetz(connection, sender, receiver, "");
                        break;
                    case "fw":
                        hilfe(connection, sender, receiver, "fw");
                        break;
                    case "help":
                    case "hilfe":
                    case "faq":
                    case "info":
                    case "man":
                        hilfe(connection, sender, receiver, "");
                        break;
                    case "mem":
                        connection.sendmsg("GC Totalmem: " + GC.GetTotalMemory(true).ToString() + "Byte, WorkingSet: " + (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024).ToString() + "kB", receiver);
                        break;
                    case "ignore":
                        hilfe(connection, sender, receiver, "ignore");
                        break;
                    case "labor":
                        labor(connection, sender, receiver, "");
                        break;
                    case "lmgtfy":
                        hilfe(connection, sender, receiver, "lmgtfy");
                        break;
                    case "ping":
                        ping(connection, sender, receiver, "");
                        break;
                    case "seen":
                        hilfe(connection, sender, receiver, "seen");
                        break;
                    case "trunk":
                        trunk(connection, sender, receiver, "");
                        break;
                    case "uptime":
                    case "laufzeit":
                        uptime(connection, sender, receiver, "");
                        break;
                    case "userlist":
                        userlist(connection, sender, sender, "");
                        break;
                    case "whmf":
                    case "w":
                        whmf(connection, sender, receiver, "");
                        break;
                    case "witz":
                        witz(connection, sender, receiver, "");
                        break;
                    case "zeit":
                        try
                        {
                            connection.sendmsg("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss") + ".", receiver);
                        }
                        catch
                        {
                            connection.sendmsg("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(", receiver);
                        }
                        break;
                    default:
                        answered_user = false;
                        break;
                }
            }
            #endregion

            if (!answered_user && !answered_admin)
            {
                if (!alias(connection, sender, receiver, message, true) && !receiver.Contains("#") && receiver != connection.nickname)
                {
                    connection.sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", receiver);
                }
            }
        }

        private static String[] news_parse(String url)
        {
            String news = get_web(url);
            List<String> newstopic = new List<String>();
            List<String> subnews = new List<String>();
            List<String> uberschriftblau = new List<String>(news.Split(new String[] { "<span class=\"uberschriftblau\">" }, 21, StringSplitOptions.None));
            uberschriftblau.RemoveAt(0);
            foreach (String uberschrift in uberschriftblau)
            {
                String text = uberschrift;
                int nbsp = uberschrift.IndexOf("&nbsp;");
                if (nbsp != -1)
                {
                    text = text.Remove(nbsp);
                }
                newstopic.Add(text.Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0]);
            }
            List<String> fliesstextblau = new List<String>(news.Split(new String[] { "<span class=\"fliesstextblau\">" }, 21, StringSplitOptions.None));
            fliesstextblau.RemoveAt(0);
            foreach (String fliesstext in fliesstextblau)
            {
                String text = fliesstext.Replace("&nbsp;", " ");
                subnews.Add(text.Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0]);
            }
            List<String> news_new = new List<String>();
            for (int i = 0; i < uberschriftblau.Count; i++)
            {
                news_new.Add(newstopic[i] + ":" + subnews[i]);
            }
            return news_new.ToArray();
        }

        private static void news()
        {
            String baseurl = "http://webgw.avm.de/download/UpdateNews.jsp";
            String[] news_de_old = news_parse(baseurl + "?lang=de");
            String[] news_en_old = news_parse(baseurl + "?lang=en");
            while (true)
            {
                String[] news_de = news_parse(baseurl + "?lang=de");
                String[] news_en = news_parse(baseurl + "?lang=en");
                if (news_de_old[0] != news_de[0])
                {
                    List<String> differs = new List<String>();
                    for (int i = 0; i < news_de.Length; i++)
                    {
                        if (news_de_old[0] != news_de[i])
                        {
                            differs.Add(news_de[i]);
                        }
                    }
                    String output = "Neue Deutsche News gesichtet! ";
                    foreach (String thenews in differs)
                    {
                        output += ", " + thenews;
                    }
                    announce(output);
                    news_de_old = news_de;
                }
                if (news_en_old[0] != news_en[0])
                {
                    List<String> differs = new List<String>();
                    for (int i = 0; i < news_de.Length; i++)
                    {
                        if (news_de_old[0] != news_de[i])
                        {
                            differs.Add(news_de[i]);
                        }
                    }
                    String output = "Neue englische News gesichtet! ";
                    foreach (String thenews in differs)
                    {
                        output += ", " + thenews;
                    }
                    announce(output);
                    news_en_old = news_en;
                }
                int sleep;
                if (!int.TryParse(configuration.get("news_check_intervall"), out sleep))
                {
                    sleep = 300000;
                }
                Thread.Sleep(sleep);
            }
        }

        private static void announce(String message)
        {
            foreach(irc connection in irc_connections)
            {
                foreach(String room in connection.rooms)
                {
                    connection.sendmsg(message, room);
                }
            }
        }

        private static void google(irc connection, String sender, String receiver, String message)
        {
            String output = "https://www.google.de/search?q=";
            if (message == "")
            {
                output = "http://www.google.de/";
            }
            else
            {
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes("\"" + message + "\""));
            }
            connection.sendmsg(output, receiver);
        }

        private static Boolean alias(irc connection, String sender, String receiver, String message, Boolean not_answered = false)
        {
            String[] parameter = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
            String alias = "";
            Boolean[] cases = new Boolean[3];
            switch (parameter[0].ToLower())
            {
                case "add":
                    if (parameter[1].Contains("="))
                    {
                        cases[2] = true;
                    }
                    else
                    {
                        connection.sendmsg("Das habe ich jetzt nicht als gültigen Alias erkannt, es muss einmal \"=\" enthalten sein, damit ich weiß was der Alias ist", receiver);
                    }
                    break;
                case "edit":
                    if (parameter[1].Contains("="))
                    {
                        cases[1] = true;
                        cases[2] = true;
                    }
                    else
                    {
                        connection.sendmsg("Das habe ich jetzt nicht als gültigen Alias erkannt, es muss einmal \"=\" enthalten sein, damit ich weiß was der Alias ist", receiver);
                    }
                    break;
                case "remove":
                    cases[1] = true;
                    break;
                default:
                    String[] splitted = message.Split(' ');
                    if (splitted.Length > 0)
                    {
                        String[] aliase = aliasdb.GetContaining(splitted[0] + "=");
                        if (aliase.Length > 0)
                        {
                            String[] thealias = aliase[0].Split(new String[] { "=" }, 2, StringSplitOptions.None);
                            String output = "";
                            int forindex = 0;
                            if (thealias[1].Split('$').Length - 1 < splitted.Length)
                            {
                                forindex = thealias[1].Split('$').Length - 1;
                            }
                            else
                            {
                                forindex = splitted.Length - 1;
                            }
                            output = thealias[1];
                            for (int i = 0; i < forindex; i++)
                            {
                                while (true)
                                {
                                    int index = output.IndexOf("$" + (i + 1));
                                    if (index == -1) break;
                                    output = output.Remove(index, 2);
                                    output = output.Insert(index, splitted[i + 1]);
                                }
                            }
                            connection.sendmsg(output, receiver);
                            return true;
                        }
                        else if(!not_answered)
                        {
                            connection.sendmsg("Diesen Alias gibt es nicht.", receiver);
                            return false;
                        }
                    }
                    return false;
            }
            if (message.Contains("="))
            {
                alias = parameter[1].Split(new String[] { "=" }, 2, StringSplitOptions.None)[0];
            }
            else
            {
                alias = parameter[1];
            }
            if (aliasdb.GetContaining(alias + "=").Length > 0)
            {
                cases[0] = true;
            }
            if (!cases[0] && cases[1] && cases[2])
            {
                connection.sendmsg("Diesen Alias gibt es noch nicht, verwende \"add\" um ihn hinzuzufügen.", receiver);
                return false;
            }
            if (cases[1])
            {
                if (!cases[0])
                {
                    connection.sendmsg("Diesen Alias gibt es nicht.", receiver);
                    return false;
                }
                aliasdb.Remove(aliasdb.GetContaining(alias + "=")[0]);
            }
            if (cases[2])
            {
                if ((cases[1] && cases[2]) ^ cases[0])
                {
                    connection.sendmsg("Es gibt diesen Alias schon, wenn du ihn verändern möchtest verwende statt \"add\", \"edit\".", receiver);
                    return false;
                }
                aliasdb.Add(parameter[1]);
            }
            if (!cases[1] && cases[2])
            {
                connection.sendmsg("Alias wurde erfolgreich hinzugefügt.", receiver);
            }
            if (cases[1] && cases[2])
            {
                connection.sendmsg("Alias wurde erfolgreich editiert!", receiver);
            }
            if (cases[1] && !cases[2])
            {
                connection.sendmsg("Alias wurde erfolgreich gelöscht!", receiver);
            }
            return false;
        }

        private static void fw(irc connection, String sender, String receiver, String message)
        {
            Boolean recovery = false;
            Boolean source = false;
            Boolean firmware = false;
            if (message.Contains(" "))
            {
                String[] splitted = message.Split(' ');
                switch (splitted[1].ToLower())
                {
                    case "add":
                        fwdb.Add(splitted[1]);
                        connection.sendmsg("Der FW Alias wurde hinzugefügt", receiver);
                        return;
                    case "all":
                        firmware = true;
                        recovery = true;
                        source = true;
                        message = splitted[0];
                        break;
                    case "source":
                        source = true;
                        message = splitted[0];
                        break;
                    case "recovery":
                        recovery = true;
                        message = splitted[0];
                        break;
                    default:
                        firmware = true;
                        message = splitted[0];
                        break;
                }
            }
            else
            {
                firmware = true;
            }

            String ftp = "ftp://ftp.avm.de/fritz.box/";
            String output = "";

            //Ordner der Box bestimmen, wenn möglich Alias verwenden, ansonsten versuchen zu finden
            String[] fws = fwdb.GetContaining(message + "=");
            if (fws.Length > 0)
            {
                ftp += fws[0].Split(new String[] { "=" }, 2, StringSplitOptions.None)[1] + "/";
            }
            else
            {
                String directory = ftp_directory(ftp);
                String[] directories = directory.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String data in directories)
                {
                    String pfad = data.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries)[8];
                    if (pfad.ToLower().Contains(message.ToLower()))
                    {
                        ftp += pfad + "/";
                        break;
                    }
                }
            }
            if (ftp == "ftp://ftp.avm.de/fritz.box/")
            {
                connection.sendmsg("Ich habe zu deiner Angabe leider nichts gefunden", receiver);
                return;
            }
            output = ftp;
            //Box Ordner ist nun gefunden, Firmware Image muss gefunden werden, vorsicht könnte bereits hier sein oder erst in einem weiteren Unterordner
            String[] ftp_recur = ftp_recursiv(ftp).Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            String recoveries = "";
            String sources = "";
            String firmwares = "";
            foreach (String datei in ftp_recur)
            {
                String[] slashsplit = datei.Split(new String[] { "/" }, 2, StringSplitOptions.None);
                String[] file_splitted = slashsplit[1].Split('.'); //fritz.box_fon_5010.23.04.27.image
                String final = slashsplit[0] + "/";
                if (slashsplit[1].Contains(".recover-image.exe"))
                {
                    int result;
                    if (file_splitted[file_splitted.Length - 5].Contains("_"))
                    {
                        String[] recoversplit = file_splitted[file_splitted.Length - 5].Split('_');
                        final += recoversplit[recoversplit.Length - 1] + "." + file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3];
                    }
                    else if (!int.TryParse(file_splitted[file_splitted.Length - 5], out result))
                    {
                        final += file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3];
                    }
                    else
                    {
                        final += file_splitted[file_splitted.Length - 5] + "." + file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3];
                    }
                    if (recoveries != "")
                    {
                        recoveries += ", " + final;
                    }
                    else
                    {
                        recoveries += final;
                    }
                }
                if (slashsplit[1].Contains(".image"))
                {
                    final += file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3] + "." + file_splitted[file_splitted.Length - 2];
                    if (firmwares != "")
                    {
                        firmwares += ", " + final;
                    }
                    else
                    {
                        firmwares += final;
                    }
                }
                if (slashsplit[1].Contains(".tar.gz"))//fritzbox7170-source-files-04.87.tar.gz
                {
                    String[] dotsplit = slashsplit[1].Split('.');
                    final = dotsplit[dotsplit.Length - 4] + "." + dotsplit[dotsplit.Length - 3];
                    if (sources != "")
                    {
                        sources += ", " + final;
                    }
                    else
                    {
                        sources += final;
                    }
                }
            }
            if (firmwares == "")
            {
                connection.sendmsg("Ich habe zu deiner Angabe leider nichts gefunden", receiver);
            }
            else
            {
                if (firmware && firmwares != "")
                {
                    output += " - Firmwares: " + firmwares;
                }
                if (recovery && recoveries != "")
                {
                    output += " - Recoveries: " + recoveries;
                }
                if (source && sources != "")
                {
                    output += " - Sources: " + sources;
                }
                connection.sendmsg(output, receiver);
            }
        }

        private static String ftp_recursiv(String ftp)
        {
            String boxdirectory_content = ftp_directory(ftp);
            String[] boxes = boxdirectory_content.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String found = "";
            foreach (String daten in boxes)
            {
                if (daten.ToCharArray()[0] == 'd')
                {
                    String pfad = daten.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    found += ftp_recursiv(ftp + pfad + "/") + ";";
                }
                if (daten.ToCharArray()[0] == '-' && (daten.Contains(".image") || daten.Contains(".recover-image.exe") || daten.Contains(".tar.gz")))
                {
                    String file = daten.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries)[8];
                    String[] ftp_splitted = ftp.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    found += ";" + ftp_splitted[ftp_splitted.Length - 1] + "/" + file;
                }
            }
            return found;
        }

        private static String ftp_directory(String ftp)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse directory = (FtpWebResponse)request.GetResponse();
            StreamReader directory_list = new StreamReader(directory.GetResponseStream());
            return directory_list.ReadToEnd();
        }

        private static void seen(irc connection, String sender, String receiver, String message)
        {
            if (userdb.GetContaining(message).Length > 0)
            {
                if (userdb.GetContaining(message)[0].Contains(","))
                {
                    String datum = userdb.GetContaining(message)[0].Split(',')[1];
                    DateTime seen;
                    DateTime.TryParse(datum, out seen);
                    connection.sendmsg("Den habe ich hier zuletzt am " + seen.ToString("dd.MM.yyyy ") + "um" + seen.ToString(" HH:mm:ss ") + "Uhr gesehen", receiver);
                }
                else
                {
                    connection.sendmsg("Der ist doch gerade hier ;)", receiver);
                }
            }
            else
            {
                connection.sendmsg("Diesen Benutzer habe ich noch nie gesehen", receiver);
            }
        }

        private static void db_command(irc connection, String sender, String receiver, String message)
        {
            try
            {
                String[] split = message.Split(' ');
                if (split[1] == "reload")
                {
                    switch (split[0])
                    {
                        case "box":
                            boxdb.Reload();
                            break;
                        case "user":
                            userdb.Reload();
                            break;
                        case "witz":
                            witzdb.Reload();
                            break;
                        case "ignore":
                            ignoredb.Reload();
                            break;
                        case "server":
                            servers.Reload();
                            break;
                        case "fw":
                            fwdb.Reload();
                            break;
                        case "alias":
                            aliasdb.Reload();
                            break;
                        case "all":
                            boxdb.Reload();
                            userdb.Reload();
                            witzdb.Reload();
                            ignoredb.Reload();
                            servers.Reload();
                            fwdb.Reload();
                            aliasdb.Reload();
                            break;
                        default:
                            connection.sendmsg("Das sieht nach einem Syntax fehler aus", receiver);
                            return;
                    }
                }
                if (split[1] == "flush")
                {
                    switch (split[0])
                    {
                        case "box":
                            boxdb.Write();
                            break;
                        case "user":
                            userdb.Write();
                            break;
                        case "witz":
                            witzdb.Write();
                            break;
                        case "ignore":
                            ignoredb.Write();
                            break;
                        case "server":
                            servers.Write();
                            break;
                        case "all":
                            boxdb.Write();
                            userdb.Write();
                            witzdb.Write();
                            ignoredb.Write();
                            servers.Write();
                            break;
                        default:
                            connection.sendmsg("Das sieht nach einem Syntax fehler aus", receiver);
                            return;
                    }
                }
                connection.sendmsg("Okay", receiver);
            }
            catch (Exception ex)
            {
                logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                connection.sendmsg("Wups, das hat eine Exception verursacht", receiver);
            }
        }

        private static void settings_command(irc connection, String sender, String receiver, String message)
        {
            String[] split = message.Split(' ');
            switch (split[0])
            {
                case "set":
                    try
                    {
                        configuration.set(split[1], split[2]);
                    }
                    catch
                    {
                        connection.sendmsg("Wups, das hat eine Exception ausgelöst", receiver);
                        return;
                    }
                    connection.sendmsg("Okay", receiver);
                    break;
                case "get":
                    try
                    {
                        connection.sendmsg(configuration.get(split[1]), receiver);
                    }
                    catch
                    {
                        connection.sendmsg("Wups, das hat eine Exception ausgelöst", receiver);
                        return;
                    }
                    break;
                default:
                    connection.sendmsg("Wups, da stimmt wohl etwas mit der Syntax nicht", receiver);
                    break;
            }
        }

        static private void leave(String message)
        {
            leave_safe.WaitOne();
            String[] config_servers_array = servers.GetAll();
            for (int i = 0; i < config_servers_array.Length; i++)
            {
                if (config_servers_array[i].Split(',')[0] == message)
                {
                    servers.Remove(servers.GetAt(i));
                    break;
                }
            }
            for (int i = 0; i < irc_connections.Count; i++)
            {
                if (irc_connections[i].hostname == message)
                {
                    irc_connections[i].disconnect();
                    irc_connections[i] = null;
                    irc_connections.RemoveAt(i);
                    break;
                }
            }
            leave_safe.ReleaseMutex();
        }

        static private void connect(irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { "," }, 5, StringSplitOptions.None);
            if (parameter.Length < 5)
            {
                hilfe(connection, sender, receiver, "connect");
            }
            if (parameter[2].Length > 9)
            {
                connection.sendmsg("Hörmal, das RFC erlaubt nur Nicknames mit 9 Zeichen", receiver);
                return;
            }
            try
            {
                Convert.ToInt32(parameter[1]);
            }
            catch
            {
                connection.sendmsg("Der PORT sollte eine gültige Ganzahl sein, Prüfe das", receiver);
                return;
            }
            try
            {
                try
                {
                    IPHostEntry hostInfo = Dns.GetHostEntry(parameter[0]);
                }
                catch
                {
                    connection.sendmsg("Ich konnte die Adresse nicht auflösen, Prüfe nochmal ob deine Eingabe korrekt ist", receiver);
                    return;
                }
                instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                servers.Add(message);
            }
            catch
            {
                connection.sendmsg("Das hat nicht funktioniert, sorry", receiver);
            }
        }

        static private void box(irc connection, String sender, String receiver, String message)
        {
            if (boxdb.Find(sender + ":" + message) == -1)
            {
                boxdb.Add(sender + ":" + message);
                connection.sendmsg("Okay danke, ich werde mir deine \"" + message + "\" notieren.", receiver);
            }
            else
            {
                connection.sendmsg("Wups, danke aber du hast mir deine \"" + message + "\" bereits mitgeteilt ;-).", receiver);
            }
        }

        static private void boxfind(irc connection, String sender, String receiver, String message)
        {
            String[] Daten = boxdb.GetContaining(message);
            if (Daten.Length > 0)
            {
                String besitzer = "";
                String[] temp;
                for (int i = 0; i < Daten.Length; i++)
                {
                    temp = Daten[i].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                    if (besitzer == "")
                    {
                        besitzer = temp[0];
                    }
                    else
                    {
                        besitzer += ", " + temp[0];
                    }
                }
                connection.sendmsg("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer, receiver);
            }
            else
            {
                connection.sendmsg("Diese Box scheint niemand zu besitzen!", receiver);
            }
        }

        static private void boxinfo(irc connection, String sender, String receiver, String message)
        {
            String[] Daten = boxdb.GetContaining(message);
            if (Daten.Length > 0)
            {
                String boxen = "";
                for (int i = 0; i < Daten.Length; i++)
                {
                    String[] user = Daten[i].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                    if (boxen != "")
                    {
                        boxen += ", " + user[1];
                    }
                    else
                    {
                        boxen = user[1];
                    }
                }
                if (message == sender)
                {
                    connection.sendmsg("Du hast bei mir die Box/en " + boxen + " registriert.", receiver);
                }
                else
                {
                    connection.sendmsg(message + " sagte mir er hätte die Box/en " + boxen, receiver);
                }
            }
            else
            {
                if (message == sender)
                {
                    connection.sendmsg("Du hast bei mir noch keine Box registriert.", receiver);
                }
                else
                {
                    connection.sendmsg("Über den habe ich keine Informationen.", receiver);
                }
            }
        }

        static private void boxlist(irc connection, String sender, String receiver, String message)
        {
            Boolean gefunden = false;
            String[] Daten = boxdb.GetAll();
            String boxen = "";
            foreach (String data in Daten)
            {
                String[] temp = data.Split(new String[] { ":" }, 2, StringSplitOptions.None);
                if (!boxen.ToLower().Contains(temp[1].ToLower()))
                {
                    if (boxen == "")
                    {
                        boxen = temp[1];
                        gefunden = true;
                    }
                    else
                    {
                        boxen += ", " + temp[1];
                        gefunden = true;
                    }
                }
            }
            if (gefunden == true)
            {
                connection.sendmsg("Folgende Boxen wurden bei mir registriert: " + boxen, receiver);
            }
            else
            {
                connection.sendmsg("Da stimmt etwas nicht, es wurde bei mir keine Box registriert", receiver);
            }
        }

        static private void boxremove(irc connection, String sender, String receiver, String message)
        {
            if (boxdb.Remove(sender + ":" + message))
            {
                connection.sendmsg("Erledigt!", receiver);
            }
            else
            {
                connection.sendmsg("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht", receiver);
            }
        }

        static private void frag(irc connection, String sender, String receiver, String message)
        {
            connection.sendmsg("Hallo " + message + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :).", message);
            connection.sendmsg("Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !box 7270) und achte darauf, dass du den Nicknamen trägst dem die Box zugeordnet werden soll", message);
        }

        static private void hilfe(irc connection, String sender, String receiver, String message)
        {
            if (message != "")
            {
                switch (message)
                {
                    case "about":
                        connection.sendmsg("Ich würde dir dann kurz etwas über mich erzählen.", receiver);
                        break;
                    case "alias":
                        connection.sendmsg("Legt einen Alias für einen Begriff fest, z.b. !alias oder !a, \"!a add freetz=Eine Modifikation für...\", \"!a edit freetz=DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen wie z.b. $1 sind möglich.", receiver);
                        break;
                    case "box":
                        connection.sendmsg("Dies trägt deine Boxdaten ein, Beispiel: \"!box 7270\", bitte jede Box einzeln angeben.", receiver);
                        break;
                    case "boxfind":
                        connection.sendmsg("Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".", receiver);
                        break;
                    case "boxinfo":
                        connection.sendmsg("Zeigt die Box/en des angegebenen Benutzers an.", receiver);
                        break;
                    case "boxlist":
                        connection.sendmsg("Dies listet alle registrierten Boxtypen auf.", receiver);
                        break;
                    case "boxremove":
                        connection.sendmsg("Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".", receiver);
                        break;
                    case "connect":
                        connection.sendmsg("Baut eine Verbindung zu einem anderen IRC Server auf, Syntax: !connect server,port,nick,quit_message,initial_channel", receiver);
                        break;
                    case "db":
                        connection.sendmsg("Führt Operationen an meiner Datenbank aus, Operator Befehl: !db dbname reload / flush", receiver);
                        break;
                    case "frag":
                        connection.sendmsg("Dann werde ich den genannten Benutzer nach seiner Box fragen, z.b. !frag Anonymous", receiver);
                        break;
                    case "freetz":
                        connection.sendmsg("Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz \"Build System\", !freetz FAQ Benutzer", receiver);
                        break;
                    case "fw":
                        connection.sendmsg("Sucht auf dem AVM FTP nach der Version des angegbenen Modells, z.b. \"!fw 7390\", \"!fw 7270_v1\", \"!fw 7390 source\", \"!fw 7390 recovery\" \"!fw 7390 all\"", receiver);
                        break;
                    case "google":
                        connection.sendmsg("Syntax: (!g) !google etwas das du suchen möchtest", receiver);
                        break;
                    case "hilfe":
                        connection.sendmsg("Du scherzbold, hehe.", receiver);
                        break;
                    case "ignore":
                        connection.sendmsg("Schließt die angegebene Person von mir aus", receiver);
                        break;
                    case "join":
                        connection.sendmsg("Daraufhin werde ich den angegebenen Channel betreten, Operator Befehl: z.b. !join #testchannel", receiver);
                        break;
                    case "labor":
                        connection.sendmsg("Ich schaue mal auf das aktuelle Datum der Labor Firmwares, Parameter: '7270', '7390', 'fhem', '7390at', 'android', 'ios'.", receiver);
                        break;
                    case "leave":
                        connection.sendmsg("Zum angegebenen Server werde ich die Verbindung trennen, Operator Befehl: !leave test.de", receiver);
                        break;
                    case "lmgtfy":
                        connection.sendmsg("Dazu sage ich jetzt mal nichts, finde es raus!", receiver);
                        break;
                    case "mem":
                        connection.sendmsg("Meine aktuelle Speicherlast berechnet vom GC (Gargabe Collector) und die insgesamt Last", receiver);
                        break;
                    case "part":
                        connection.sendmsg("Den angegebenen Channel werde ich verlassen, Operator Befehl: z.b. !part #testchannel", receiver);
                        break;
                    case "ping":
                        connection.sendmsg("Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin", receiver);
                        break;
                    case "quit":
                        connection.sendmsg("Das wird mich beenden :(, Operator Befehl: kein parameter", receiver);
                        break;
                    case "restart":
                        connection.sendmsg("Ich werde versuchen mich selbst neuzustarten, Operator Befehl: kein parameter", receiver);
                        break;
                    case "seen":
                        connection.sendmsg("Gibt aus wann der Nutzer zuletzt gesehen wurde.", receiver);
                        break;
                    case "settings":
                        connection.sendmsg("Ändert meine Einstellungen, Operator Befehl: !settings get name, !settings set name wert", receiver);
                        break;
                    case "trunk":
                        connection.sendmsg("Dies zeigt den aktuellsten Changeset an.", receiver);
                        break;
                    case "unignore":
                        connection.sendmsg("Die betroffene Person wird von der ignore Liste gestrichen, Operator Befehl: z.b. !unignore Testnick", receiver);
                        break;
                    case "uptime":
                        connection.sendmsg("Das zeigt meine aktuelle Laufzeit an.", receiver);
                        break;
                    case "userlist":
                        connection.sendmsg("Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben.", receiver);
                        break;
                    case "whmf":
                        connection.sendmsg("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf \"CAPI Treiber\", !whmf 7270 Benutzer", receiver);
                        break;
                    case "witz":
                        connection.sendmsg("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen", receiver);
                        break;
                    case "zeit":
                        connection.sendmsg("Das gibt die aktuelle Uhrzeit aus.", receiver);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                connection.sendmsg("Befehle: about alias box boxfind boxinfo boxlist boxremove frag freetz fw google hilfe ignore labor lmgtfy mem ping seen trunk uptime userlist whmf witz zeit.", receiver);
                connection.sendmsg("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen", receiver);
            }
        }

        static private Boolean ignore_check(String parameter = "")
        {
            if (ignoredb.Find(parameter) != -1)
            {
                return true;
            }
            return false;
        }

        static private void ignore(irc connection, String sender, String receiver, String message)
        {
            if (sender == message || sender == "Suchiman" || sender == "hippie2000")
            {
                ignoredb.Add(message);
                connection.sendmsg("Ich werde " + message + " ab sofort keine beachtung mehr schenken", receiver);
            }
        }

        static private void labor_check()
        {
            List<String> LaborDates = new List<String>();
            Boolean[] released = new Boolean[7];
            String output = "";
            String webseite = get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
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
                if (configuration.get("labor_check") == "false")
                {
                    Thread.Sleep(30000);
                }
                while (configuration.get("labor_check") == "true")
                {
                    do
                    {
                        Thread.Sleep(1000);
                        webseite = get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
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
                        announce(output);
                    }
                    output = "";
                    int labor_check_intervall;
                    if (!int.TryParse(configuration.get("labor_check_intervall"), out labor_check_intervall))
                    {
                        labor_check_intervall = 300000;
                    }
                    Thread.Sleep(labor_check_intervall);
                }
            }
        }

        static private void labor(irc connection, String sender, String receiver, String message)
        {
            String webseite = get_web("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
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
                    String feedback = get_web(url);
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

        static private void lmgtfy(irc connection, String sender, String receiver, String message)
        {
            if (message.Contains("\""))
            {
                String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                split[1] = split[1].Replace(' ', '+');
                String[] nick = split[2].Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (nick.Length > 1)
                {
                    connection.sendmsg("@" + split[2] + ": Siehe: http://lmgtfy.com/?q=" + split[1], receiver);
                }
                else
                {
                    connection.sendmsg("http://lmgtfy.com/?q=" + split[1], receiver);
                }
            }
            else
            {
                String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (split.Length > 1)
                {
                    connection.sendmsg("@" + split[1] + ": Siehe: http://lmgtfy.com/?q=" + split[0], receiver);
                }
                else
                {
                    connection.sendmsg("http://lmgtfy.com/?q=" + split[0], receiver);
                }
            }
        }

        static private void ping(irc connection, String sender, String receiver, String message)
        {
            connection.sendmsg("Pong " + sender, receiver);
        }

        static private void trunk(irc connection, String sender, String receiver, String message)
        {
            String webseite = get_web("http://freetz.org/changeset");
            if (webseite != "")
            {
                String changeset = "Der aktuellste Changeset ist " + webseite.Split(new String[] { "<h1>" }, 2, StringSplitOptions.None)[1].Split(new String[] { "</h1>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Changeset " }, 2, StringSplitOptions.None)[1];
                changeset += " und wurde am" + webseite.Split(new String[] { "<dd class=\"time\">" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "   " }, 5, StringSplitOptions.None)[4] + " in den Trunk eingecheckt. Siehe: http://freetz.org/changeset";
                connection.sendmsg(changeset, receiver);
            }
            else
            {
                connection.sendmsg("Leider war es mir nicht möglich auf die Freetz Webseite zuzugreifen", receiver);
            }
        }

        static private void unignore(String parameter)
        {
            ignoredb.Remove(parameter);
        }

        static private void uptime(irc connection, String sender, String receiver, String message)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            TimeSpan connecttime = connection.uptime();
            connection.sendmsg("Meine Laufzeit beträgt " + laufzeit.Days + " Tage, " + laufzeit.Hours + " Stunden, " + laufzeit.Minutes + " Minuten und " + laufzeit.Seconds + " Sekunden und bin mit diesem Server seit " + connecttime.Days + " Tage, " + connecttime.Hours + " Stunden, " + connecttime.Minutes + " Minuten und " + connecttime.Seconds + " Sekunden verbunden", receiver);
        }

        static private void userlist(irc connection, String sender, String receiver, String message)
        {
            Boolean gefunden = false;
            String[] Daten = boxdb.GetAll();
            String besitzer = "";
            String[] temp;
            foreach (String data in Daten)
            {
                temp = data.Split(new String[] { ":" }, 2, StringSplitOptions.None);
                if (!besitzer.Contains(temp[0]))
                {
                    if (besitzer == "")
                    {
                        besitzer = temp[0];
                        gefunden = true;
                    }
                    else
                    {
                        besitzer += ", " + temp[0];
                        gefunden = true;
                    }
                }
            }
            if (gefunden == true)
            {
                connection.sendmsg("Diese Benutzer haben bei mir mindestens eine Box registriert: " + besitzer, receiver);
            }
            else
            {
                connection.sendmsg("Ich fürchte, mir ist ein Fehler unterlaufen. Ich kann keine registrierten Benutzer feststellen.", receiver);
            }
        }

        static private void whmf(irc connection, String sender, String receiver, String message)
        {
            String output = "http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=";
            String nick = "";
            String uri = "";
            if (message == "")
            {
                output = "http://www.wehavemorefun.de/fritzbox/index.php";
            }
            else
            {
                if (message.Contains("\""))
                {
                    String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    uri = split[1];
                    if (split[2] != "")
                    {
                        nick = split[2].Remove(0, 1);
                    }
                }
                else
                {
                    String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    uri = split[0];
                    if (split.Length > 1)
                    {
                        nick = split[1];
                    }
                }
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(uri));
                if (configuration.get("whmf_url_resolve") == "true")
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(output);
                    request.Timeout = 10000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    output = response.ResponseUri.ToString();
                }
                if (nick != "")
                {
                    output = nick + ": Siehe: " + output;
                }
            }
            output = output.Replace("%23", "#");
            connection.sendmsg(output, receiver);
        }

        static private void freetz(irc connection, String sender, String receiver, String message)
        {
            String output = "http://freetz.org/search?q=";
            String nick = "";
            String uri = "";
            if (message == "")
            {
                output = "http://freetz.org/wiki";
            }
            else
            {
                if (message.Contains("\""))
                {
                    String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    uri = split[1];
                    if (split[2] != "")
                    {
                        nick = split[2].Remove(0, 1);
                    }
                }
                else
                {
                    String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    uri = split[0];
                    if (split.Length > 1)
                    {
                        nick = split[1];
                    }
                }
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(uri)) + "&wiki=on";
                if (nick != "")
                {
                    output = nick + ": Siehe: " + output;
                }
            }
            output = output.Replace("%23", "#");
            connection.sendmsg(output, receiver);
        }

        static private void witz(irc connection, String sender, String receiver, String message)
        {
            if (message != "")
            {
                String[] witz = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (witz[0] == "add")
                {
                    witzdb.Add(witz[1]);
                    connection.sendmsg("Ist notiert " + sender, receiver);
                }
                else
                {
                    String[] splitted = message.Split(' ');
                    List<String> alle_witze = new List<String>(witzdb.GetAll());
                    List<String> such_witze = new List<String>(alle_witze);
                    for (int i = 0; i < alle_witze.Count; i++)
                    {
                        foreach (String data in splitted)
                        {
                            if (!alle_witze[i].ToLower().Contains(data.ToLower()))
                            {
                                such_witze.Remove(alle_witze[i]);
                            }
                        }
                    }
                    if (such_witze.Count > 0)
                    {
                        Random rand = new Random();
                        connection.sendmsg(such_witze[rand.Next(such_witze.Count)], receiver);
                    }
                    else
                    {
                        connection.sendmsg("Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet", receiver);
                    }
                }
            }
            else
            {
                Random rand = new Random();
                if (witz_randoms.Count >= 10)
                {
                    witz_randoms.RemoveAt(0);
                }
                int random = rand.Next(witzdb.Size());
                for (int i = 0; !(!witz_randoms.Contains(random) && i < 10); i++)
                {
                    random = rand.Next(witzdb.Size());
                }
                witz_randoms.Add(random);
                if (witzdb.Size() > 0)
                {
                    connection.sendmsg(witzdb.GetAt(random), receiver);
                }
                else
                {
                    connection.sendmsg("Mir fällt gerade kein Fritz!Witz ein", receiver);
                }
            }
        }

        static private void boxfrage(irc connection, String sender, String receiver, String message)
        {
            if (ignore_check(sender) || configuration.get("boxfrage") == "false") return;
            try
            {
                String[] users = userdb.GetAll();
                foreach (String user in users)
                {
                    if (user.Contains(sender) || sender.Contains(user))
                    {
                        return;
                    }
                }
                Thread.Sleep(10000);
                connection.sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :).", receiver);
                connection.sendmsg("Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !box 7270) und achte darauf, dass du den Nicknamen trägst dem die Box zugeordnet werden soll", receiver);
                userdb.Add(sender);
            }
            catch (Exception ex)
            {
                logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }

        static private void process_incomming(irc connection, String source, String nick, String message)
        {
            switch (source)
            {
                case "LOG":
                    logging(message);
                    return;
                case "JOIN":
                    logging(nick + " hat den Raum " + message + " betreten");
                    boxfrage(connection, nick, nick, nick);
                    if (userdb.GetContaining(nick).Length > 0)
                    {
                        if (userdb.GetContaining(nick)[0].Contains(","))
                        {
                            userdb.Remove(userdb.GetContaining(nick)[0]);
                            userdb.Add(nick);
                        }
                    }
                    return;
                case "QUIT":
                    logging(nick + " hat den Server verlassen");
                    if (userdb.GetContaining(nick).Length > 0)
                    {
                        if (!userdb.GetContaining(nick)[0].Contains(","))
                        {
                            userdb.Remove(userdb.GetContaining(nick)[0]);
                            userdb.Add(nick + "," + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        }
                    }
                    return;
                case "PART":
                    logging(nick + " hat den Raum " + message + " verlassen");
                    if (userdb.GetContaining(nick).Length > 0)
                    {
                        if (!userdb.GetContaining(nick)[0].Contains(","))
                        {
                            userdb.Remove(userdb.GetContaining(nick)[0]);
                            userdb.Add(nick + "," + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        }
                    }
                    return;
                case "NICK":
                    logging(nick + " heißt jetzt " + message);
                    return;
                case "KICK":
                    logging(nick + " hat mich aus dem Raum " + message + " geworfen");
                    connection.leave(message);
                    return;
                default:
                    break;
            }
            if (message.Contains("#96*6*"))
            {
                if (DateTime.Now.Hour > 5 && DateTime.Now.Hour < 16)
                {
                    connection.sendmsg("Kein Bier vor 4", source);
                }
                else
                {
                    connection.sendmsg("Bier holen", source);
                }
            }
            if (source.ToCharArray()[0] == '#')
            {
                logging(source + " " + nick + ": " + message);
            }
            else
            {
                logging("Von " + nick + ": " + message);
                if (message.ToCharArray()[0] != '!' && !nick.Contains(".") && nick != connection.nickname)
                {
                    connection.sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", nick);
                }
                source = nick;
            }
            if (message.ToCharArray()[0] == '!')
            {
                process_command(connection, nick, source, message.Remove(0, 1));
            }
        }

        static private String get_web(String url)
        {
            StringBuilder sb = new StringBuilder();
            Stream resStream = null;
            String tempString = null;
            Byte[] buf = new Byte[8192];
            int count = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                resStream = response.GetResponseStream();
            }
            catch (Exception ex)
            {
                logging("Exception beim Webseiten Aufruf aufgetreten: " + ex.Message);
                return "";
            }
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    tempString = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(tempString);
                }
            }
            while (count > 0);
            return sb.ToString();
        }

        static private void Trennen()
        {
            laborthread.Abort();
            foreach(irc connections in irc_connections)
            {
                String raumliste = null;
                foreach (String raum in connections.rooms)
                {
                    if (raumliste == null)
                    {
                        raumliste = raum;
                    }
                    else
                    {
                        raumliste += ":" + raum;
                    }
                }
                int position = servers.Find(servers.GetContaining(connections.hostname)[0]);
                String[] substr = servers.GetAt(position).Split(new String[] { "," }, 5, StringSplitOptions.None);
                substr[4] = raumliste;
                servers.Remove(servers.GetAt(position));
                servers.Add(substr[0] + "," + substr[1] + "," + substr[2] + "," + substr[3] + "," + substr[4]);
                connections.disconnect();
            }
        }

        static private void init()
        {
            loggingthread = new System.ComponentModel.BackgroundWorker();
            loggingthread.WorkerSupportsCancellation = true;
            loggingthread.DoWork += new System.ComponentModel.DoWorkEventHandler(log_thread);
            loggingthread.RunWorkerAsync();
            startzeit = DateTime.Now;
            String[] config = servers.GetAll();
            try
            {
                foreach (String connection_server in config)
                {
                    if (connection_server.Length > 0)
                    {
                        String[] parameter = connection_server.Split(new String[] { "," }, 5, StringSplitOptions.None);
                        instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                    }
                }
            }
            catch (Exception ex)
            {
                logging("Exception in der Initialesierung der Server: " + ex.Message);
            }
            laborthread = new Thread(delegate() { labor_check(); });
            laborthread.Start();
            Thread consolenthread = new Thread(new ThreadStart(consoleread));
            consolenthread.IsBackground = true;
            consolenthread.Start();
            antifloodingcount = 0;
            antifloodingthread = new Thread(new ThreadStart(antiflooding));
            antifloodingthread.IsBackground = true;
            antifloodingthread.Start();
            Thread newsthread = new Thread(new ThreadStart(news));
            newsthread.IsBackground = true;
            newsthread.Start();
        }

        static private void antiflooding()
        {
            while (true)
            {
                int time;
                if (!int.TryParse(configuration.get("floodingcount_reduction"), out time))
                {
                    time = 5000;//Standard Wert wenn die Konvertierung fehlschlägt
                }
                Thread.Sleep(time);
                if (antifloodingcount > 0)
                {
                    antifloodingcount--;
                }
                if (antifloodingcount == 0)
                {
                    floodingnotificated = false;
                }
            }
        }

        static private void instantiate_connection(String server, int port, String nick, String quit_message, String initial_channel)
        {
            irc connection = new irc(server, port, nick);
            connection.quit_message = quit_message;
            connection.Received += new irc.ReceivedEventHandler(process_incomming);
            connection.AutoReconnect = true;
            connection.connect();
            Thread.Sleep(1000);
            if (initial_channel.Contains(":"))
            {
                String[] channels = initial_channel.Split(':');
                foreach (String channel in channels)
                {
                    connection.join(channel);
                }
            }
            else
            {
                connection.join(initial_channel);
            }
            irc_connections.Add(connection);
        }

        static private Boolean running_check()
        {
            for (int i = 0; i < irc_connections.Count; i++)
            {
                if (irc_connections[i].running())
                {
                    return true;
                }
            }
            return false;
        }

        static private void logging(String to_log)
        {
            logging_safe.WaitOne();
            try
            {
                logging_list.Add(DateTime.Now.ToString("dd.MM HH:mm:ss ") + to_log);
            }
            catch (Exception ex)
            {
                logging("Exception beim logging aufgetreten: " + ex.Message);
            }
            logging_safe.ReleaseMutex();
        }

        static private void log_thread(Object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    while (!(logging_list.Count > 0))
                    {
                        Thread.Sleep(500);
                    }
                    FileInfo loginfo = new FileInfo("log.txt");
                    if (loginfo.Exists)
                    {
                        if (loginfo.Length >= 1048576)
                        {
                            if (!Directory.Exists("oldlogs"))
                            {
                                Directory.CreateDirectory("oldlogs");
                            }
                            if (!File.Exists("oldlogs/log" + DateTime.Now.Day + "." + DateTime.Now.Month + ".txt"))
                            {
                                loginfo.MoveTo("oldlogs/log" + DateTime.Now.Day + "." + DateTime.Now.Month + ".txt");
                            }
                        }
                    }
                    File.AppendAllText("log.txt", logging_list[0] + "\r\n", Encoding.GetEncoding("iso-8859-1"));
                    Console.WriteLine(logging_list[0]);
                    logging_list.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Zugriff auf den Serverlog: " + ex.Message);
                    return;
                }
            }
        }

        static private void consoleread()
        {
            while (true)
            {
                String console_input = Console.ReadLine();
                String[] console_splitted = console_input.Split(new String[] { " " }, 2, StringSplitOptions.None);
                switch (console_splitted[0])
                {
                    case "exit":
                        Trennen();
                        break;
                    case "connect":
                        String[] parameter = console_splitted[1].Split(new String[] { "," }, 5, StringSplitOptions.None);
                        instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                        servers.Add(console_splitted[1]);
                        break;
                    case "leave":
                        leave(console_splitted[1]);
                        break;
                }
            }
        }

        static private void Main(String[] args)
        {
            init();
            while (running_check())
            {
                Thread.Sleep(2000);
            }
            if (restart == true)
            {
                try
                {
                    System.Diagnostics.Process.Start("/bin/sh", "/home/suchi/ircbot/start");
                }
                catch (Exception ex)
                {
                    logging("Exception beim restart aufgetreten: " + ex.Message);
                }
            }
        }
    }
}