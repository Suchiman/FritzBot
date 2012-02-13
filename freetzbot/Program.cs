using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace freetzbot
{
    class Program
    {
        static private System.ComponentModel.BackgroundWorker empfangsthread;
        static private System.ComponentModel.BackgroundWorker loggingthread;

        static public TcpClient c;
        static public String nickname = "FritzBot";

        #if DEBUG
        static public String server = "suchiman.selfip.org";
        static public String raum = "#eingang";
        #else
        static public String server = "irc.atw-inter.net";
        static public String raum = "#fritzbox";
        #endif

        static public Boolean klappe = false;
        static public Boolean crashed = true;
        static public Boolean restart = false;
        static public String zeilen = "1029";
        static public DateTime startzeit;
        static public List<string> logging_list = new List<string>();

        static private void bot_antwort(String sender, Boolean privat, String nachricht)
        {
            String[] parameter = nachricht.Split(new String[] { " " }, 2, StringSplitOptions.None);
            if (sender == "Suchiman" || sender == "hippie2000")
            {
                switch (parameter[0].ToLower())
                {
                    case "klappe":
                        klappe = true;
                        Senden("Tschuldigung, bin ruhig", privat, sender);
                        break;
                    case "okay":
                        klappe = false;
                        Senden("Okay bin zurück ;-)", privat, sender);
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
            if (klappe)
            {
                privat = true;
            }
            switch (parameter[0].ToLower())
            {
                case "about":
                    Senden("Programmiert hat mich Suchiman, und ich bin dazu da, um Daten über Fritzboxen zu sammeln und andere kleine Dinge zu tuen. Ich bestehe derzeit aus " + zeilen + " Zeilen C# Code.", privat, sender);
                    break;
                case "box":
                    if (parameter.Length > 1)
                    {
                        box(sender, privat, parameter[1]);
                    }
                    else
                    {
                        box(sender, privat);
                    }
                    break;
                case "boxfind":
                    if (parameter.Length > 1)
                    {
                        boxfind(sender, true, parameter[1]);
                    }
                    else
                    {
                        boxfind(sender, true);
                    }
                    break;
                case "boxinfo":
                    if (parameter.Length > 1)
                    {
                        boxinfo(sender, privat, parameter[1]);
                    }
                    else
                    {
                        boxinfo(sender, privat);
                    }
                    break;
                case "boxlist":
                    boxlist(sender, true);
                    break;
                case "boxremove":
                    if (parameter.Length > 1)
                    {
                        boxremove(sender, privat, parameter[1]);
                    }
                    else
                    {
                        boxremove(sender, privat);
                    }
                    break;
                case "frag":
                    if (parameter.Length > 1)
                    {
                        frag(sender, privat, parameter[1]);
                    }
                    else
                    {
                        frag(sender, privat);
                    }
                    break;
                case "freetz":
                case "f":
                    if (parameter.Length > 1)
                    {
                        freetz(sender, privat, parameter[1]);
                    }
                    else
                    {
                        freetz(sender, privat);
                    }
                    break;
                case "help":
                case "hilfe":
                case "faq":
                case "info":
                    if (parameter.Length > 1)
                    {
                        hilfe(sender, privat, parameter[1]);
                    }
                    else
                    {
                        hilfe(sender, privat);
                    }
                    break;
                case "labor":
                    if (parameter.Length > 1)
                    {
                        labor(sender, privat, parameter[1]);
                    }
                    else
                    {
                        labor(sender, privat);
                    }
                    break;
                case "ping":
                    ping(sender, privat);
                    break;
                case "trunk":
                    trunk(sender, privat);
                    break;
                case "uptime":
                    uptime(sender, privat);
                    break;
                case "userlist":
                    userlist(sender, true);
                    break;
                case "whmf":
                case "w":
                    if (parameter.Length > 1)
                    {
                        whmf(sender, privat, parameter[1]);
                    }
                    else
                    {
                        whmf(sender, privat);
                    }
                    break;
                case "witz":
                    if (parameter.Length > 1)
                    {
                        witz(sender, privat, parameter[1]);
                    }
                    else
                    {
                        witz(sender, privat);
                    }
                    break;
                case "zeit":
                    try
                    {
                        Senden("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss") + ".", privat, sender);
                    }
                    catch
                    {
                        Senden("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(", privat, sender);
                    }
                    break;
                default:
                    break;
            }
        }

        static private void box(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                StreamWriter db = new StreamWriter("box.db", true, Encoding.GetEncoding("iso-8859-1"));
                db.WriteLine(sender + ":" + parameter);
                db.Close();
                Senden("Okay danke, ich werde es mir notieren.", privat, sender);
            }
            else
            {
                hilfe(sender, privat, parameter = "box");
            }
        }

        static private void boxfind(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                Boolean gefunden = false;
                String[] Daten = db_lesen("box.db");
                String besitzer = "";
                String[] temp;
                foreach (String data in Daten)
                {
                    if (data.ToLower().Contains(parameter.ToLower()))
                    {
                        temp = data.Split(new String[] { ":" }, 2, StringSplitOptions.None);
                        if (!besitzer.ToLower().Contains(temp[0].ToLower()))
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
                }
                if (gefunden == true)
                {
                    Senden("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer, privat, sender);
                }
                else
                {
                    Senden("Diese Box scheint niemand zu besitzen!", privat, sender);
                }
            }
            else
            {
                hilfe(sender, privat, "boxfind");
            }
        }

        static private void boxinfo(String sender, Boolean privat, String parameter = "")
        {
            if (parameter == "")
            {
                parameter = sender;
            }
            Boolean gefunden = false;
            String[] user = new String[2];
            String[] Daten = db_lesen("box.db");
            String boxen = "";
            for (int i = 0; i < Daten.Length; i++)
            {
                user = Daten[i].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                if (parameter.ToLower() == user[0].ToLower())
                {
                    gefunden = true;
                    if (boxen != "")
                    {
                        boxen += ", " + user[1];
                    }
                    else
                    {
                        boxen = user[1];
                    }
                }
            }
            if (gefunden == true)
            {
                if (parameter == sender)
                {
                    Senden("Du hast bei mir die Box/en " + boxen + " registriert.", privat, sender);
                }
                else
                {
                    Senden(parameter + " sagte mir er hätte die Box/en " + boxen, privat, sender);
                }
            }
            else
            {
                if (parameter == sender)
                {
                    Senden("Du hast bei mir noch keine Box registriert.", privat, sender);
                }
                else
                {
                    Senden("Über den habe ich keine Informationen.", privat, sender);
                }
            }
        }

        static private void boxlist(String sender, Boolean privat)
        {
            Boolean gefunden = false;
            String[] Daten = db_lesen("box.db");
            String boxen = "";
            String[] temp;
            foreach (String data in Daten)
            {
                temp = data.Split(new String[] { ":" }, 2, StringSplitOptions.None);
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
                Senden("Folgende Boxen wurden bei mir registriert: " + boxen, privat, sender);
            }
            else
            {
                Senden("Diese Box scheint niemand zu besitzen.", privat, sender);
            }
        }

        static private void boxremove(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                String[] Daten = db_lesen("box.db");
                int i = 0;
                String suche = sender + ":" + parameter;
                for (i = 0; i < Daten.Length; i++)
                {
                    if (Daten[i] == suche)
                    {
                        List<String> tmp = new List<String>(Daten);
                        tmp.RemoveAt(i);
                        Daten = tmp.ToArray();
                        Senden("Erledigt!", privat, sender);
                    }
                }
                StreamWriter db = new StreamWriter("box.db", false, Encoding.GetEncoding("iso-8859-1"));
                for (i = 0; i < Daten.Length; i++)
                {
                    db.WriteLine(Daten[i]);
                }
                db.Close();
            }
            else
            {
                hilfe(sender, privat, "boxremove");
            }
        }

        static private void frag(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                Senden("Hallo " + parameter + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast. :)", privat, sender);
            }
            else
            {
                hilfe(sender, privat, "frag");
            }
        }

        static private void hilfe(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                switch (parameter)
                {
                    case "about":
                        Senden("Ich würde dir dann kurz etwas über mich erzählen.", privat, sender);
                        break;
                    case "box":
                        Senden("Dies trägt deine Boxdaten ein, Beispiel: \"!box 7270\", bitte jede Box einzeln angeben.", privat, sender);
                        break;
                    case "boxfind":
                        Senden("Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".", privat, sender);
                        break;
                    case "boxinfo":
                        Senden("Zeigt die Box/en des angegebenen Benutzers an.", privat, sender);
                        break;
                    case "boxlist":
                        Senden("Dies listet alle registrierten Boxtypen auf.", privat, sender);
                        break;
                    case "boxremove":
                        Senden("Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".", privat, sender);
                        break;
                    case "frag":
                        Senden("Dann werde ich den genannten Benutzer nach seiner Box fragen, z.b. !frag Anonymous", privat, sender);
                        break;
                    case "freetz":
                        Senden("Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz \"Build System\", !freetz FAQ Benutzer", privat, sender);
                        break;
                    case "hilfe":
                        Senden("Du scherzbold, hehe.", privat, sender);
                        break;
                    case "labor":
                        Senden("Ich schaue mal auf das aktuelle Datum der Labor Firmwares, Parameter: '7270', '7390', 'fhem', '7390at', 'android', 'ios'.", privat, sender);
                        break;
                    case "ping":
                        Senden("Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin", privat, sender);
                        break;
                    case "trunk":
                        Senden("Dies zeigt den aktuellsten Changeset an.", privat, sender);
                        break;
                    case "uptime":
                        Senden("Das zeigt meine aktuelle Laufzeit an.", privat, sender);
                        break;
                    case "userlist":
                        Senden("Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben.", privat, sender);
                        break;
                    case "whmf":
                        Senden("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf \"CAPI Treiber\", !whmf 7270 Benutzer", privat, sender);
                        break;
                    case "witz":
                        Senden("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen.", privat, sender);
                        break;
                    case "zeit":
                        Senden("Das gibt die aktuelle Uhrzeit aus.", privat, sender);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Senden("Aktuelle Befehle: about box boxfind boxinfo boxlist boxremove frag freetz hilfe labor ping trunk uptime userlist whmf witz zeit.", privat, sender);
                Senden("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen", privat, sender);
            }
        }

        static private void labor(String sender, Boolean privat, String parameter = "")
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();
            Stream resStream = response.GetResponseStream();
            String tempString = null;
            int count = 0;

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
            int modell = 0;
            String changeset = "";
            if (parameter == "")
            {
                parameter = "7270";
            }
            switch (parameter.ToLower())
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
                case "7270":
                    modell = 6;
                    break;
                default:
                    changeset += "Für die " + parameter + " steht derzeit keine Labor Version zur Verfügung. ";
                    break;
            }
            if (modell != 0)
            {
                changeset += "Die neueste " + parameter + " labor Version ist am " + sb.ToString().Split(new String[] { "<span style=\"font-size:10px;float:right; margin-right:20px;\">" }, 7, StringSplitOptions.None)[modell].Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "\t \t\t\t " }, 3, StringSplitOptions.None)[1].Split(new String[] { "\r" }, 3, StringSplitOptions.None)[0] + " erschienen.";
            }
            Senden(changeset, privat, sender);
        }

        static private void ping(String sender, Boolean privat)
        {
            Senden("Pong " + sender, privat, sender);
        }

        static private void trunk(String sender, Boolean privat)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create("http://freetz.org/changeset");
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();
            Stream resStream = response.GetResponseStream();
            String tempString = null;
            int count = 0;

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
            String changeset = "Der aktuellste Changeset ist " + sb.ToString().Split(new String[] { "<h1>" }, 2, StringSplitOptions.None)[1].Split(new String[] { "</h1>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Changeset " }, 2, StringSplitOptions.None)[1];
            changeset += " und wurde am" + sb.ToString().Split(new String[] { "<dd class=\"time\">" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "   " }, 5, StringSplitOptions.None)[4] + " in den Trunk eingecheckt. Siehe: http://freetz.org/changeset";
            Senden(changeset, privat, sender);
        }

        static private void uptime(String sender, Boolean privat)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            Senden("Meine Laufzeit beträgt " + laufzeit.Days + " Tage, " + laufzeit.Hours + " Stunden, " + laufzeit.Minutes + " Minuten und " + laufzeit.Seconds + " Sekunden.", privat, sender);
        }

        static private void userlist(String sender, Boolean privat)
        {
            Boolean gefunden = false;
            String[] Daten = db_lesen("box.db");
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
                Senden("Diese Benutzer haben bei mir mindestens eine Box registriert: " + besitzer, privat, sender);
            }
            else
            {
                Senden("Ich fürchte, mir ist ein Fehler unterlaufen. Ich kann keine registrierten Benutzer feststellen.", privat, sender);
            }
        }

        static private void whmf(String sender, Boolean privat, String parameter = "")
        {
            if (parameter == "")
            {
                Senden("http://www.wehavemorefun.de/fritzbox/index.php", privat, sender);
            }
            else
            {
                //Parameter: "CAPI Treiber" peter
                if (parameter.Contains("\""))
                {
                    String[] split = parameter.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    split[1] = split[1].Replace(' ', '_');
                    String[] nick = split[2].Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (nick.Length > 1)
                    {
                        Senden("@" + split[2] + ": Siehe: http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[1], privat, sender);
                    }
                    else
                    {
                        Senden("http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[1], privat, sender);
                    }
                }
                else
                {
                    String[] split = parameter.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (split.Length > 1)
                    {
                        Senden("@" + split[1] + ": Siehe: http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[0], privat, sender);
                    }
                    else
                    {
                        Senden("http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=" + split[0], privat, sender);
                    }
                }
            }
        }

        static private void freetz(String sender, Boolean privat, String parameter = "")
        {
            if (parameter == "")
            {
                Senden("http://freetz.org/wiki", privat, sender);
            }
            else
            {
                if (parameter.Contains("\""))
                {
                    String[] split = parameter.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    split[1] = split[1].Replace(' ', '_');
                    String[] nick = split[2].Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (nick.Length > 1)
                    {
                        Senden("@" + split[2] + ": Siehe: http://freetz.org/search?q=" + split[1] + "&wiki=on", privat, sender);
                    }
                    else
                    {
                        Senden("http://freetz.org/search?q=" + split[1] + "&wiki=on", privat, sender);
                    }
                }
                else
                {
                    String[] split = parameter.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    if (split.Length > 1)
                    {
                        Senden("@" + split[1] + ": Siehe: http://freetz.org/search?q=" + split[0] + "&wiki=on", privat, sender);
                    }
                    else
                    {
                        Senden("http://freetz.org/search?q=" + split[0] + "&wiki=on", privat, sender);
                    }
                }
            }
        }

        static private void witz(String sender, Boolean privat, String parameter = "")
        {
            String[] Daten = db_lesen("witze.db");
            if (parameter != "")
            {
                String[] witz = parameter.Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (witz[0] == "add")
                {
                    StreamWriter db = new StreamWriter("witze.db", true, Encoding.GetEncoding("iso-8859-1"));
                    db.WriteLine(witz[1]);
                    db.Close();
                    Senden("Ist notiert", privat, sender);
                }
            }
            else
            {
                Random rand = new Random();
                if (Daten[rand.Next(Daten.Length)] != "")
                {
                    Senden(Daten[rand.Next(Daten.Length)], privat, sender);
                }
                else
                {
                    Senden("Mir fällt gerade kein Fritz!Witz ein", privat, sender);
                }
            }
        }

        static private void boxfrage(String sender)
        {
            try
            {
                Boolean gefunden = false;
                String[] Daten = db_lesen("user.db");
                for (int i = 0; i < Daten.Length; i++)
                {
                    if (sender == Daten[i] || sender.Contains(Daten[i]))
                    {
                        gefunden = true;
                    }
                }
                if (gefunden == false)
                {
                    Thread.Sleep(10000);
                    Senden("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :). Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170.", true, sender, "NOTICE");
                    StreamWriter db = new StreamWriter("user.db", true, Encoding.GetEncoding("iso-8859-1"));
                    db.WriteLine(sender);
                    db.Close();
                }
            }
            catch { }
        }

        static private void verarbeitung(String eingehend)
        {
            //Beispiel einer v6 Nachricht: ":User!~info@2001:67c:1401:2100:5ab0:35fa:fe76:feb0 PRIVMSG #eingang :hehe"
            //Beispiel einer Nachricht: ":Suchiman!~Suchiman@Robin-PC PRIVMSG #eingang :hi"
            //Beispiel eines Joins: ":Suchiman!~robinsue@91-67-134-206-dynip.superkabel.de JOIN :#eingang"
            Boolean privat = true;

            //Auf Ping Prüfen, Wichtig: wird Pong ausgelassen kickt dich der Server
            try
            {
                String[] ping = eingehend.Split(new String[] { ":" }, 3, StringSplitOptions.None);
                if (ping[0] == "PING ")
                {
                    Senden("PONG " + ping[1], false, "", "RAW");
                }
            }
            catch (Exception ex)
            {
                logging("Exception bei der Ping verarbeitung: " + ex.Message);
            }
            try
            {
                String[] privat_check = eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None);
                if (privat_check.Length > 2)
                {
                    if (privat_check[2] == raum)
                    {
                        privat = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logging("Exception bei der Verarbeitung ob es eine Private nachricht ist: " + ex.Message);
            }

            try
            {
                if (eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None).LongLength > 2)
                {
                    //Join checken
                    if (eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None)[1] == "JOIN")
                    {
                        String nick = eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None)[0].Split(new String[] { "!" }, 2, StringSplitOptions.None)[0].Split(new String[] { ":" }, 2, StringSplitOptions.None)[1];
                        if (klappe == false)
                        {
                            boxfrage(nick);
                        }
                        logging(nick + " hat den Raum betreten");
                        return;
                    }
                    //Prüfen ob der Raum verlassen wird
                    if (eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None)[1] == "PART")
                    {
                        String nick = eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None)[0].Split(new String[] { "!" }, 2, StringSplitOptions.None)[0].Split(new String[] { ":" }, 2, StringSplitOptions.None)[1];
                        logging(nick + " hat den Raum verlassen");
                        return;
                    }
                    //Prüfen ob der Server verlassen wird
                    if (eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None)[1] == "QUIT")
                    {
                        String nick = eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None)[0].Split(new String[] { "!" }, 2, StringSplitOptions.None)[0].Split(new String[] { ":" }, 2, StringSplitOptions.None)[1];
                        logging(nick + " hat den Server verlassen");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logging("Exception bei der boxfrage: " + ex.Message);
            }
            //Verarbeitung einer Nachricht, eine Nachricht sollte 3 gesplittete Elemente im Array haben
            try
            {
                String[] verarbeitung = eingehend.Split(new String[] { " " }, 4, StringSplitOptions.None);
                if (verarbeitung.Length > 3)
                {
                    if (verarbeitung[3].Contains(":"))
                    {
                        String nachricht = verarbeitung[3].Split(new String[] { ":" }, 2, StringSplitOptions.None)[1];
                        String nick = verarbeitung[0].Split(new String[] { "!" }, 2, StringSplitOptions.None)[0].Split(new String[] { ":" }, 2, StringSplitOptions.None)[1];
                        logging(nick + ": " + nachricht);
                        try
                        {
                        if (nachricht.ToCharArray()[0] == '!')
                        {
                            String befehl = nachricht.Split(new String[] { "!" }, 2, StringSplitOptions.None)[1];
                            bot_antwort(nick, privat, befehl);
                        }
                        }
                        catch (Exception ex)
                        {
                            logging("Exception im Bot Antwort thread aufgetreten: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logging("Exception bei der Verarbeitung: " + ex.Message);
            }
        }

        static private void empfangsthread_DoWork(Object sender, DoWorkEventArgs e)
        {
            try
            {
                StreamReader inStream = new StreamReader(c.GetStream(), Encoding.GetEncoding("iso-8859-1"));
                while (true)
                {
                    if (empfangsthread.CancellationPending)
                    {
                        logging("Beendung wurde angefordert");
                        return;
                    }
                    String Daten = "";
                    try
                    {
                        Daten = inStream.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        logging("Exception beim TCP lesen aufgefangen: " + ex.Message);
                        return;//Exception an kritischer Stelle, Thread beenden, wird von Main thread neugestartet
                    }
                    Thread thread = new Thread(delegate() { verarbeitung(Daten); });
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                logging("Exception in unbekannten Code bereich des empfangsthread aufgefangen: " + ex.Message);
                return;
            }
        }

        static private String[] db_lesen(String db)
        {
            if (!File.Exists(db))
            {
                StreamWriter create_db = new StreamWriter(db, true, Encoding.GetEncoding("iso-8859-1"));
                create_db.WriteLine("");
                create_db.Close();
            }
            StreamReader sr = new StreamReader(db, Encoding.GetEncoding("iso-8859-1"));
            String[] Daten = new String[0];
            int i = 0;
            while (sr.Peek() >= 0)
            {
                Array.Resize(ref Daten, Daten.Length + 1);
                Daten[i] = sr.ReadLine();
                i++;
            }
            sr.Close();
            return Daten;
        }

        static private String[] splitat(String text, int length)
        {
            String[] gesplittet = new String[0];
            if (text.Length >= length)
            {
                Decimal loops = Math.Ceiling((decimal)text.Length / (decimal)length);
                int splitlength = length;
                for (int i = 0; i < loops; i++)
                {
                    if (!(i < loops - 1))
                    {
                        splitlength = text.Length % length;
                    }
                    Array.Resize(ref gesplittet, gesplittet.Length + 1);
                    gesplittet[i] = text.Substring(length * i, splitlength);
                }
            }
            else
            {
                Array.Resize(ref gesplittet, gesplittet.Length + 1);
                gesplittet[0] = text;
            }
            return gesplittet;
        }

        static private void Senden(String text, Boolean privat, String adressant = "", String methode = "PRIVMSG")
        {
            if (!privat || adressant == "")
            {
                adressant = raum;
            }
            try
            {
                Byte[] sendBytes;
                NetworkStream inOut = c.GetStream();
                if (methode != "RAW")
                {
                    String[] tosend = splitat(text,507-(methode.Length+adressant.Length));
                    for (int i = 0; i < tosend.Length; i++)
                    {
                        sendBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(methode + " " + adressant + " :" + tosend[i] + "\r\n");
                        logging(nickname + ": " + text);
                        inOut.Write(sendBytes, 0, sendBytes.Length);
                    }
                }
                else
                {
                    sendBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(text + "\r\n");
                    inOut.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch { }
        }

        static private Boolean Verbinden()
        {
            try
            {
                c = new TcpClient(server, 6667);
                empfangsthread.RunWorkerAsync();
                Thread.Sleep(10);
                Senden("USER " + nickname + " " + nickname + " " + nickname + " :" + nickname + "\r\n", false, "", "RAW");
                Senden("NICK " + nickname + "\r\n", false, "", "RAW");
                Senden("JOIN " + raum + "\r\n", false, "", "RAW");
                return true;
            }
            catch
            {
                return false;
            }
        }

        static private void Trennen()
        {
            crashed = false;
            empfangsthread.CancelAsync();
            Senden("QUIT :I'll be back", false, "", "RAW");
        }

        static public void init()
        {
            empfangsthread = new System.ComponentModel.BackgroundWorker();
            empfangsthread.WorkerSupportsCancellation = true;
            empfangsthread.DoWork += new System.ComponentModel.DoWorkEventHandler(empfangsthread_DoWork);
            loggingthread = new System.ComponentModel.BackgroundWorker();
            loggingthread.WorkerSupportsCancellation = true;
            loggingthread.DoWork += new System.ComponentModel.DoWorkEventHandler(log_thread);
            loggingthread.RunWorkerAsync();
            startzeit = DateTime.Now;
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
            Verbinden();
            while (crashed)
            {
                while (empfangsthread.IsBusy)
                {
                    Thread.Sleep(500);
                }
                if (crashed)
                {
                    logging("Verbindung verloren :( versuche Verbindung wiederherzustellen");
                    int count = 0;
                    while (!empfangsthread.IsBusy)
                    {
                        count++;
                        logging("Versuch " + count);
                        if (!Verbinden())
                        {
                            Thread.Sleep(5000);
                        }
                    }
                    logging("Verbindung nach dem " + count + " versuch erfolgreich wiederhergestellt");
                }
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