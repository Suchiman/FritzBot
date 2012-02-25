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
        static private String zeilen = Convert.ToString(64 + 172 + 335 + 995);
        static private DateTime startzeit;
        static private List<string> logging_list = new List<string>();
        static private db boxdb = new db("box.db");
        static private db userdb = new db("user.db");
        static private db witzdb = new db("witze.db");
        static private db ignoredb = new db("ignore.db");
        static private db servers = new db("servers.cfg");
        static private settings configuration = new settings("config.cfg");
        static private List<irc> irc_connections = new List<irc>();

        static private void process_command(irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
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
                            leave(connection, sender, receiver, parameter[1]);
                            break;
                        case "unignore":
                            unignore(parameter[1]);
                            connection.sendmsg("Alles klar", receiver);
                            break;
                        case "klappe":
                            hilfe(connection, sender, receiver, "klappe");
                            break;
                        case "okay":
                            hilfe(connection, sender, receiver, "okay");
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
                        case "klappe":
                            configuration.set("klappe", "true");
                            connection.sendmsg("Tschuldigung, bin ruhig", receiver);
                            break;
                        case "okay":
                            configuration.set("klappe", "false");
                            connection.sendmsg("Okay bin zurück ;-)", receiver);
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
                    }
                }
            }
            if (ignore_check(sender)) return;
            if (parameter.Length > 1) if (ignore_check(parameter[1])) return;
            if (configuration.get("klappe") == "true") receiver = sender;

            if (parameter.Length > 1)//Wenn ein zusätzlicher Parameter angegebenen wurde....
            {
                switch (parameter[0].ToLower())
                {
                    case "about":
                        connection.sendmsg("Programmiert hat mich Suchiman, und ich bin dazu da, um Daten über Fritzboxen zu sammeln und andere kleine Dinge zu machen. Ich bestehe derzeit aus " + zeilen + " Zeilen C# Code.", receiver);
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
                    case "help":
                    case "hilfe":
                    case "faq":
                    case "info":
                    case "man":
                        hilfe(connection, sender, receiver, parameter[1]);
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
                        break;
                }
            }
            else //Wenn kein zusätzlicher Parameter angegeben wurde....
            {
                switch (parameter[0].ToLower())
                {
                    case "about":
                        connection.sendmsg("Programmiert hat mich Suchiman, und ich bin dazu da, um Daten über Fritzboxen zu sammeln und andere kleine Dinge zu tuen. Ich bestehe derzeit aus " + zeilen + " Zeilen C# Code.", receiver);
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
                    case "freetz":
                    case "f":
                        freetz(connection, sender, receiver, "");
                        break;
                    case "help":
                    case "hilfe":
                    case "faq":
                    case "info":
                    case "man":
                        hilfe(connection, sender, receiver, "");
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
                        break;
                }
            }
        }

        static private void leave(irc connection, String sender, String receiver, String message)
        {
            String[] config_servers_array = servers.GetAll();
            for (int i = 0; i < config_servers_array.Length; i++)
            {
                if (config_servers_array[i].Split(',')[0] == message)
                {
                    servers.Remove(servers.GetAt(i));
                }
            }
            for (int i = 0; i < irc_connections.Count; i++)
            {
                if (irc_connections[i].hostname == message)
                {
                    irc_connections[i].disconnect();
                    irc_connections[i] = null;
                    irc_connections.RemoveAt(i);
                }
            }
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
            connection.sendmsg("Hallo " + message + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast. :)", receiver);
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
                    case "frag":
                        connection.sendmsg("Dann werde ich den genannten Benutzer nach seiner Box fragen, z.b. !frag Anonymous", receiver);
                        break;
                    case "freetz":
                        connection.sendmsg("Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz \"Build System\", !freetz FAQ Benutzer", receiver);
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
                    case "klappe":
                        connection.sendmsg("Zwingt mich nur noch Privat zu antworten, Operator Befehl: kein parameter", receiver);
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
                    case "okay":
                        connection.sendmsg("Erlaubt es mir wieder im Channel zu sprechen, Operator Befehl: kein parameter", receiver);
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
                        connection.sendmsg("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen.", receiver);
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
                connection.sendmsg("Aktuelle Befehle: about box boxfind boxinfo boxlist boxremove frag freetz hilfe ignore labor lmgtfy ping trunk uptime userlist whmf witz zeit.", receiver);
                connection.sendmsg("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen", receiver);
            }
        }

        static private Boolean ignore_check(String parameter = "")
        {
            if(ignoredb.Find(parameter) != -1)
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
                        daten[i-1] = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[i].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                    }
                    changeset = "Aktuelle Labor Daten: iOS: " + daten[0] + ", Android: " + daten[1] + ", 7390: " + daten[2] + ", FHEM: " + daten[3] + ", 7390at: " + daten[4] + ", 7320: " + daten[5] + ", 7270: " + daten[6] + ".";
                    break;
                default:
                    changeset += "Für die " + message + " steht derzeit keine Labor Version zur Verfügung. ";
                    break;
            }

            if (modell != 0)
            {
                String datum = webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[modell].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0];
                String url = "http://www.avm.de/de/Service/Service-Portale/Labor/" + webseite.Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 8, StringSplitOptions.None)[modell].Split(new String[] { "<a href=" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\"" }, 3, StringSplitOptions.None)[1].Split(new String[] { "/" }, 2, StringSplitOptions.None)[0] + "/labor_feedback_versionen.php";
                String feedback = get_web(url);
                if (feedback == "")
                {
                    connection.sendmsg("Leider war es mir nicht möglich alle Daten von der AVM Webseite abzurufen", receiver);
                    return;
                }
                String version = feedback.Split(new String[] { "</strong>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Version " }, 2, StringSplitOptions.None)[1];
                changeset += "Die neueste " + message + " labor Version ist am " + datum + " erschienen mit der Versionsnummer: " + version + ". Changelog: " + url;
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
            if (message == "")
            {
                connection.sendmsg("http://www.wehavemorefun.de/fritzbox/index.php", receiver);
            }
            else
            {
                //Parameter: "CAPI Treiber" peter
                if (message.Contains("\""))
                {
                    String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    split[1] = split[1].Replace(' ', '_');
                    String[] nick = split[2].Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (nick.Length > 1)
                    {
                        connection.sendmsg("@" + split[2] + ": Siehe: http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[1], receiver);
                    }
                    else
                    {
                        connection.sendmsg("http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[1], receiver);
                    }
                }
                else
                {
                    String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (split.Length > 1)
                    {
                        connection.sendmsg("@" + split[1] + ": Siehe: http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[0], receiver);
                    }
                    else
                    {
                        connection.sendmsg("http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[0], receiver);
                    }
                }
            }
        }

        static private void freetz(irc connection, String sender, String receiver, String message)
        {
            if (message == "")
            {
                connection.sendmsg("http://freetz.org/wiki", receiver);
            }
            else
            {
                if (message.Contains("\""))
                {
                    String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    split[1] = split[1].Replace(' ', '_');
                    String[] nick = split[2].Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (nick.Length > 1)
                    {
                        connection.sendmsg("@" + split[2] + ": Siehe: http://freetz.org/search?q=" + split[1] + "&wiki=on", receiver);
                    }
                    else
                    {
                        connection.sendmsg("http://freetz.org/search?q=" + split[1] + "&wiki=on", receiver);
                    }
                }
                else
                {
                    String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (split.Length > 1)
                    {
                        connection.sendmsg("@" + split[1] + ": Siehe: http://freetz.org/search?q=" + split[0] + "&wiki=on", receiver);
                    }
                    else
                    {
                        connection.sendmsg("http://freetz.org/search?q=" + split[0] + "&wiki=on", receiver);
                    }
                }
            }
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
            }
            else
            {
                Random rand = new Random();
                if (witzdb.GetAt(rand.Next(witzdb.Size())) != "")
                {
                    connection.sendmsg(witzdb.GetAt(rand.Next(witzdb.Size())), receiver);
                }
                else
                {
                    connection.sendmsg("Mir fällt gerade kein Fritz!Witz ein", receiver);
                }
            }
        }

        static private void boxfrage(irc connection, String sender, String receiver, String message)
        {
            if (ignore_check(sender)) return;
            try
            {
                if (!(userdb.GetContaining(sender).Length > 0))
                {
                    Thread.Sleep(10000);
                    connection.sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :). Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /PRIVMSG FritzBot !box 7270)", receiver);
                    userdb.Add(sender);
                }
            }
            catch { }
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
                    return;
                case "QUIT":
                    logging(nick + " hat den Server verlassen");
                    return;
                case "PART":
                    logging(nick + " hat den Raum " + message + " verlassen");
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
            if (source.ToCharArray()[0] == '#')
            {
                logging(source + " " + nick + ": " + message);
            }
            else
            {
                logging("Von " + nick + ": " + message);
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
            for (int i = 0; i < irc_connections.ToArray().Length; i++)
            {
                irc_connections[i].disconnect();
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
            if (config.Length > 0)
            {
                foreach (String connection_server in config)
                {
                    String[] parameter = connection_server.Split(new String[] { "," }, 5, StringSplitOptions.None);
                    instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                }
            }
        }

        static private void instantiate_connection(String server, int port, String nick, String quit_message, String initial_channel)
        {
            irc connection = new irc(server, port, nick);
            connection.quit_message = quit_message;
            connection.Received += new irc.ReceivedEventHandler(process_incomming);
            connection.connect();
            connection.AutoReconnect = true;
            connection.join(initial_channel);
            irc_connections.Add(connection);
        }

        static private Boolean running_check()
        {
            for (int i = 0; i < irc_connections.ToArray().Length; i++)
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
            try
            {
                logging_list.Add(DateTime.Now.ToString("dd.MM HH:mm:ss ") + to_log);
            }
            catch (Exception ex)
            {
                logging("Exception beim logging aufgetreten: " + ex.Message);
            }
        }

        static private void log_thread(Object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                while (!(logging_list.Count > 0))
                {
                    Thread.Sleep(500);
                }
                StreamWriter log;
                try
                {
                    log = new StreamWriter("log.txt", true, Encoding.GetEncoding("iso-8859-1"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Zugriff auf den Serverlog: " + ex.Message);
                    return;
                }
                log.WriteLine(logging_list[0]);
                Console.WriteLine(logging_list[0]);
                logging_list.RemoveAt(0);
                log.Close();
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