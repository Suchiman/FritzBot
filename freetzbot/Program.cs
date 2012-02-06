using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;

namespace freetzbot
{
    class Program
    {
        static private System.ComponentModel.BackgroundWorker empfangsthread;
        static private System.ComponentModel.BackgroundWorker boxfrage;

        static public TcpClient c;
        static public String nickname = "FritzBot";
        static public String server = "irc.atw-inter.net";
        static public String raum = "#fritzbox";
        static public Boolean klappe = false;
        static public Boolean crashed = true;

        static private void bot_antwort(String sender, Boolean privat, String nachricht)
        {
            String[] parameter = nachricht.Split(new string[] { " " }, 2, StringSplitOptions.None);
            if (sender == "Suchiman" || sender == "hippie2000")
            {
                switch (parameter[0])
                {
                    case "quit":
                        Trennen();
                        break;
                    case "klappe":
                        klappe = true;
                        Senden("Tschuldigung, bin ruhig", privat);
                        break;
                    case "okay":
                        klappe = false;
                        Senden("Okay bin zurück ;-)", privat);
                        break;
                }
            }
            if (!klappe)
            {
                switch (parameter[0])
                {
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
                            Senden("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss"), privat, sender);
                        }
                        catch { }
                        break;
                    case "frag":
                        try
                        {
                            Senden("Hallo " + parameter[1] + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :)", privat, sender);
                        }
                        catch { }
                        break;
                    case "about":
                        Senden("Programmiert hat mich Suchiman, und ich bin dazu da, um Daten über Fritzboxen zu sammeln. Ich bestehe derzeit aus 594 Zeilen C# code", privat, sender);
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
                    case "trunk":
                        trunk(sender, privat);
                        break;
                    case "box":
                        box(sender, privat, parameter[1]);
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
                    case "userlist":
                        userlist(sender, privat);
                        break;
                    case "boxfind":
                        if (parameter.Length > 1)
                        {
                            boxfind(sender, privat, parameter[1]);
                        }
                        else
                        {
                            boxfind(sender, privat);
                        }
                        break;
                    case "boxlist":
                        boxlist(sender, privat);
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
                    default:
                        break;
                }
            }
        }

        static private void trunk(String sender, Boolean privat)
        {
            Senden("Einen moment bitte ich stelle sogleich die Nachforschungen an...", privat, sender);
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
            String changeset = "Der aktuellste Changeset ist " + sb.ToString().Split(new string[] { "<h1>" }, 2, StringSplitOptions.None)[1].Split(new string[] { "</h1>" }, 2, StringSplitOptions.None)[0].Split(new string[] { "Changeset " }, 2, StringSplitOptions.None)[1];
            changeset += " und wurde am " + sb.ToString().Split(new string[] { "<dd class=\"time\">" }, 2, StringSplitOptions.None)[1].Split(new string[] { "\n" }, 3, StringSplitOptions.None)[1] + " in den Trunk eingecheckt. Siehe: http://freetz.org/changeset";
            Senden(changeset, privat, sender);
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
                        temp = data.Split(new string[] { ":" }, 2, StringSplitOptions.None);
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
                    Senden("Folgende User scheinen diese Box zu haben: " + besitzer, privat, sender);
                }
                else
                {
                    Senden("Diese Box scheint niemand zu haben", privat, sender);
                }
            }
            else
            {
                hilfe(sender, privat, "boxfind");
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
                temp = data.Split(new string[] { ":" }, 2, StringSplitOptions.None);
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
                Senden("Diese Box scheint niemand zu haben", privat, sender);
            }
        }

        static private void boxremove(String sender, Boolean privat, String parameter = "")
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
                }
            }
            StreamWriter db = new StreamWriter("box.db", false);
            for (i = 0; i < Daten.Length; i++)
            {
                db.WriteLine(Daten[i]);
            }
            db.Close();
        }

        static private void userlist(String sender, Boolean privat)
        {
            Boolean gefunden = false;
            String[] Daten = db_lesen("box.db");
            String besitzer = "";
            String[] temp;
            foreach (String data in Daten)
            {
                temp = data.Split(new string[] { ":" }, 2, StringSplitOptions.None);
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
                Senden("Folgende User haben bei mir Boxen registriert: " + besitzer, privat, sender);
            }
            else
            {
                Senden("Ich glaube etwas stimmt mit meiner Datenbank nicht denn niemand hat sich bei mir registriert", privat, sender);
            }
        }

        static private void witz(String sender, Boolean privat, String parameter = "")
        {
            String[] Daten = db_lesen("witze.db");
            if (parameter != "")
            {
                String[] witz = parameter.Split(new string[] { " " }, 2, StringSplitOptions.None);
                if (witz[0] == "add")
                {
                    StreamWriter db = new StreamWriter("witze.db", true);
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

        static private void hilfe(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                switch (parameter)
                {
                    case "about":
                        Senden("Sollte klar sein", privat, sender);
                        break;
                    case "frag":
                        Senden("Ich nerve einen User ;)", privat, sender);
                        break;
                    case "zeit":
                        Senden("Ich schaue mal kurz auf meine Armbanduhr", privat, sender);
                        break;
                    case "witz":
                        Senden("Erzählt dir einen Witz, mit \"!witz add witztext\" kannst du einen neuen hinzufügen", privat, sender);
                        break;
                    case "trunk":
                        Senden("Zeigt den aktuellsten Changeset an", privat, sender);
                        break;
                    case "box":
                        Senden("Trägt deine boxdaten ein, example: \"!box 7270\" bitte jede Box einzeln angeben", privat, sender);
                        break;
                    case "boxinfo":
                        Senden("Ruft die gespeicherten Boxinfos eines nutzers ab", privat, sender);
                        break;
                    case "boxfind":
                        Senden("Findet die nutzer der angegebenen Box: Bsp. \"!boxfind 7270\"", privat, sender);
                        break;
                    case "boxlist":
                        Senden("Listet alle Registrierten Boxtypen auf", privat, sender);
                        break;
                    case "boxremove":
                        Senden("Entfernt die exakt von dir genannte Box aus deiner boxinfo, als Beispiel: \"!boxremove 7270v1\"", privat, sender);
                        break;
                    case "userlist":
                        Senden("Listet die Nutzernamen all jener auf die eine Box bei mir registriert haben", privat, sender);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Senden("Aktuelle Befehle: about frag zeit witz trunk box boxinfo boxfind boxlist boxremove userlist", privat, sender);
                Senden("Hilfe zu jedem Befehl mit \"!help befehl\"", privat, sender);
            }
        }

        static private void box(String sender, Boolean privat, String parameter = "")
        {
            if (parameter != "")
            {
                StreamWriter db = new StreamWriter("box.db", true);
                db.WriteLine(sender + ":" + parameter);
                db.Close();
                Senden("Okay danke, ich werde es mir notieren", privat, sender);
            }
            else
            {
                hilfe(sender, privat, parameter = "box");
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
                user = Daten[i].Split(new string[] { ":" }, 2, StringSplitOptions.None);
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
                    Senden("Du hast bei mir die Box/en " + boxen + " registriert", privat, sender);
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
                    Senden("Du hast bei mir noch keine Box registriert", privat, sender);
                }
                else
                {
                    Senden("Von dem weiss ich nix", privat, sender);
                }
            }
        }

        static private void boxfrage_DoWork(Object sender, DoWorkEventArgs e)
        {
            try
            {
                Boolean gefunden = false;
                String[] Daten = db_lesen("user.db");
                for (int i = 0; i < Daten.Length; i++)
                {
                    if (e.Argument.ToString() == Daten[i])
                    {
                        gefunden = true;
                    }
                }
                if (gefunden == false)
                {
                    Thread.Sleep(10000);
                    Senden("Hallo " + e.Argument.ToString() + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :). Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170", true, e.Argument.ToString(), "NOTICE");
                    StreamWriter db = new StreamWriter("user.db", true);
                    db.WriteLine(e.Argument.ToString());
                    db.Close();
                }
            }
            catch { }
        }

        static private void empfangsthread_DoWork(Object sender, DoWorkEventArgs e)
        {
            NetworkStream inOut = c.GetStream();
            StreamReader inStream = new StreamReader(c.GetStream());
            while (true)
            {
                if (empfangsthread.CancellationPending)
                {
                    break;
                }
                String Daten = inStream.ReadLine();
                String[] pieces = Daten.Split(new string[] { ":" }, 3, StringSplitOptions.None);
                Boolean privat = true;
                if (pieces.Length > 2)
                {
                    String[] methode = pieces[1].Split(new string[] { " " }, 5, StringSplitOptions.None);
                    if (methode[2] == raum)
                    {
                        privat = false;
                    }
                }
                if (pieces[0] == "PING ")       //Wichtig: Auf Ping anforderungen mit Pong antworten oder Kick ;- )
                {
                    Byte[] sendBytes = Encoding.GetEncoding("iso-8859-1").GetBytes("PONG " + pieces[1] + "\r\n");
                    inOut.Write(sendBytes, 0, sendBytes.Length);
                }
                else if (pieces.Length >= 3)    //Verarbeitung einer Nachricht, eine Nachricht sollte 3 gesplittete Elemente im Array haben
                {                               //Beispiel einer Nachricht: ":Suchiman!~Suchiman@Robin-PC PRIVMSG #eingang :hi"
                    string[] nickname = pieces[1].Split(new string[] { "!" }, 2, StringSplitOptions.None);
                    logging(nickname[0] + ": " + pieces[2]);
                    if (pieces[2].ToCharArray()[0] == '!')
                    {
                        string[] befehl = pieces[2].Split(new string[] { "!" }, 2, StringSplitOptions.None);
                        Thread thread = new Thread(delegate() { bot_antwort(nickname[0], privat, befehl[1]); });
                        thread.Start();
                    }
                    if (pieces[2] == raum && klappe == false)
                    {
                        boxfrage.RunWorkerAsync(nickname[0]);
                    }
                }
            }
        }

        static private String[] db_lesen(String db)
        {
            if (!File.Exists(db))
            {
                StreamWriter create_db = new StreamWriter(db, true);
                create_db.WriteLine("");
                create_db.Close();
            }
            StreamReader sr = new StreamReader(db);
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
                sendBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(methode + " " + adressant + " :" + text + "\r\n");
                inOut.Write(sendBytes, 0, sendBytes.Length);
                logging(nickname + ": " + text);
            }
            catch { }
        }

        static private Boolean Verbinden()
        {
            #if DEBUG
            server = "fritz.box";
            raum = "#eingang";
            #endif
            try
            {
                c = new TcpClient(server, 6667);
                NetworkStream inOut = c.GetStream();
                empfangsthread.RunWorkerAsync();
                Byte[] sendBytes = Encoding.GetEncoding("iso-8859-1").GetBytes("NICK " + nickname + "\r\nUSER " + nickname + " " + nickname + " " + nickname + " :" + nickname + "\r\nJOIN " + raum + "\r\n");
                inOut.Write(sendBytes, 0, sendBytes.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static private void Trennen()
        {
            NetworkStream inOut = c.GetStream();
            Byte[] sendBytes = Encoding.GetEncoding("iso-8859-1").GetBytes("QUIT\r\n");
            try
            {
                inOut.Write(sendBytes, 0, sendBytes.Length);
                c.Close();
            }
            catch { }
            crashed = false;
            empfangsthread.CancelAsync();
        }

        static public void init()
        {
            empfangsthread = new System.ComponentModel.BackgroundWorker();
            empfangsthread.WorkerSupportsCancellation = true;
            empfangsthread.DoWork += new System.ComponentModel.DoWorkEventHandler(empfangsthread_DoWork);
            boxfrage = new System.ComponentModel.BackgroundWorker();
            boxfrage.WorkerSupportsCancellation = true;
            boxfrage.DoWork += new System.ComponentModel.DoWorkEventHandler(boxfrage_DoWork);
        }

        static private void logging(String to_log)
        {
            StreamWriter log = new StreamWriter("log.txt", true);
            log.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss ") + to_log);
            Console.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss ") + to_log);
            log.Close();
        }

        static private void Main(string[] args)
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
                        logging("Versuch " + count);
                        if (!Verbinden())
                        {
                            Thread.Sleep(5000);
                        }
                        count++;
                    }
                    logging("Verbindung nach dem " + count + " versuch erfolgreich wiederhergestellt");
                }
            }
        }
    }
}