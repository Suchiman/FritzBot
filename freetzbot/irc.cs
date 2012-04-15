using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FritzBot
{
    class Irc
    {
        public delegate void ReceivedEventHandler(Irc connection, String source, String nick, String message);
        public event ReceivedEventHandler Received;
        public String QuitMessage;

        private Thread empfangs_thread;
        private Thread Watchthread;
        private Boolean CancelThread;
        public string HostName;
        private int Port;
        public String Nickname;
        private TcpClient Connection;
        private DateTime ConnectTime;
        public Boolean AutoReconnect;
        public int AutoReconnectIntervall;
        public List<string> rooms = new List<string>();

        public Irc(String server, int serverport, String nick)
        {
            HostName = server;
            Port = serverport;
            Nickname = nick;
            CancelThread = false;
            QuitMessage = "";
            empfangs_thread = new Thread(delegate() { empfangsthread(); });
            Watchthread = new Thread(delegate() { reconnect(); });
            AutoReconnect = false;
            AutoReconnectIntervall = 5000;
        }

        private void reconnect()
        {
            int count = 1;
            while (true)
            {
                try
                {
                    if (!empfangs_thread.IsAlive || !Connection.Client.Connected)
                    {
                        log("Verbindung abgerissen, versuche Verbindung wiederherzustellen");
                        log("Versuch " + count);
                        while (!Connect())
                        {
                            Thread.Sleep(AutoReconnectIntervall);
                            count++;
                            log("Versuch " + count);
                        }
                        log("Verbindung nach dem " + count + " versuch erfolgreich wiederhergestellt");
                        count = 1;
                    }
                }
                catch (Exception ex)
                {
                    log("Exception beim AutoReconnect aufgetreten: " + ex.Message);
                }
                Thread.Sleep(AutoReconnectIntervall);
            }
        }

        public Boolean Connect()
        {
            try
            {
                if (empfangs_thread.IsAlive)
                {
                    empfangs_thread.Abort();
                }
                Connection = new TcpClient(HostName, Port);
                empfangs_thread = new Thread(delegate() { empfangsthread(); });
                empfangs_thread.Name = "EmpfangsThread" + HostName;
                empfangs_thread.Start();
                Sendraw("NICK " + Nickname);
                Sendraw("USER " + Nickname + " 8 * :" + Nickname);
                log("Verbindung mit Server " + HostName + " hergestellt");
                ConnectTime = DateTime.Now;
                rejoin();
                if (AutoReconnect && !Watchthread.IsAlive)
                {
                    Watchthread = new Thread(delegate() { reconnect(); });
                    Watchthread.Name = "WatchThread " + HostName;
                    Watchthread.Start();
                }
                return true;
            }
            catch (Exception ex)
            {
                log("Exception beim Herstellen der Verbindung: " + ex);
                return false;
            }
        }

        public void Disconnect()
        {
            Watchthread.Abort();
            String output = "QUIT";
            if (!String.IsNullOrEmpty(QuitMessage))
            {
                output += " :" + QuitMessage;
            }
            Sendraw(output);
            CancelThread = true;
            log("Server " + HostName + " verlassen");
        }

        public void JoinChannel(String channel)
        {
            if (channel.ToCharArray()[0] != '#')
            {
                log("Diesem channel kann ich nicht joinen");
                return;
            }
            Sendraw("JOIN " + channel);
            if (!rooms.Contains(channel)) rooms.Add(channel);
            log("Betrete Raum " + channel);
        }

        private void rejoin()
        {
            foreach (String room in rooms)
            {
                JoinChannel(room);
            }
        }

        public void Leave(String channel)
        {
            if (rooms.Contains(channel))
            {
                Sendraw("PART " + channel);
                rooms.Remove(channel);
                log("Verlasse Raum " + channel);
            }
        }

        private static String[] splitlength(String text, int length)
        {
            List<String> output = new List<String>();
            List<String> splitted = new List<String>(text.Split(' '));
            for (int i = 0; splitted.Count > 0; i++)
            {
                output.Add("");
                while (output[i].Length < length - 10 && splitted.Count > 0)
                {
                    output[i] += " " + splitted[0];
                    splitted.RemoveAt(0);
                }
                output[i] = output[i].Remove(0, 1);
            }
            return output.ToArray();
        }

        private void log(String to_log)
        {
            Received(this, "LOG", "", to_log);
        }

        public Boolean Running()
        {
            return Watchthread.IsAlive;
        }

        public TimeSpan Uptime()
        {
            return DateTime.Now.Subtract(ConnectTime);
        }

        public void Sendaction(String message, String receiver)
        {
            Sendmsg("\u0001ACTION " + message + "\u0001", receiver);
        }

        public void Sendmsg(String message, String receiver)
        {
            String output = "PRIVMSG " + receiver + " :";
            String[] tosend = splitlength(message, 500 - (output.Length));
            foreach (String send in tosend)
            {
                Sendraw(output + send);
                log("An " + receiver + ": " + send);
            }
        }

        public void Sendraw(String message)
        {
            try
            {
                StreamWriter stream = new StreamWriter(Connection.GetStream(), Encoding.GetEncoding("iso-8859-1"));
                stream.AutoFlush = true;
                stream.Write(message + "\r\n");
            }
            catch (Exception ex)
            {
                log("Exception beim Senden: " + ex);
            }
        }

        private void empfangsthread()
        {
            int ErrorCount = 0;
            while (true)
            {
                try
                {
                    StreamReader stream = new StreamReader(Connection.GetStream(), Encoding.GetEncoding("iso-8859-1"));
                    while (true)
                    {
                        if (CancelThread)
                        {
                            return;
                        }
                        String Daten = stream.ReadLine();
                        if (Daten == null)
                        {
                            throw new InvalidOperationException("connection lost");
                        }
                        Thread thread = new Thread(delegate() { process_respond(Daten); });
                        thread.Name = "Process " + HostName;
                        thread.Start();
                    }
                }
                catch (Exception ex)
                {
                    log("Exception im empfangsthread aufgefangen: " + ex.Message);
                    ErrorCount++;
                    if (ErrorCount > 3)
                    {
                        return;
                    }
                }
            }
        }

        private void process_respond(String message)
        {
            //Beispiel einer v6 Nachricht: ":User!~info@2001:67c:1401:2100:5ab0:35fa:fe76:feb0 PRIVMSG #eingang :hehe"
            //Beispiel einer Nachricht: ":Suchiman!~Suchiman@Robin-PC PRIVMSG #eingang :hi"
            //Beispiel einer PRIVMSG: ":Suchi!~email@91-67-134-206-dynip.superkabel.de PRIVMSG Suchiman :hi"
            //Beispiel eines Joins: ":Suchiman!~robinsue@91-67-134-206-dynip.superkabel.de JOIN :#eingang"
            //Action: ":FritzBot!~FritzBot@91-67-134-206-dynip.superkabel.de PRIVMSG #fritzbox :\001ACTION rennt los zum channel #eingang\001"
            //Rename: :Suchi!~email@91-67-134-206-dynip.superkabel.de NICK :testi
            //KICK: :Suchiman!~email@91-67-134-206-dynip.superkabel.de KICK #fritzbox FritzBot :Suchiman
            //Ping anforderung des Servers: "PING :fritz.box"
            try
            {
                String[] splitmessage = message.Split(new String[] { " " }, 4, StringSplitOptions.None);
                String nick = null;
                if (splitmessage.Length > 1)
                {
                    if (splitmessage[0] == "PING")
                    {
                        Sendraw("PONG " + splitmessage[1]);
                        return; //Es ist ja sonst nichts weiter zu tuen
                    }
                    if (splitmessage[0] == "ERROR")
                    {
                        return; //Mhhh... was machen wenn error gesendet wird?
                    }
                }
                if (splitmessage.Length > 2)
                {
                    nick = splitmessage[0].Split(new String[] { "!" }, 2, StringSplitOptions.None)[0].Split(new String[] { ":" }, 2, StringSplitOptions.None)[1];
                    String what = null;
                    if (splitmessage[2].ToCharArray()[0] == ':') what = splitmessage[2].Remove(0, 1);
                    else what = splitmessage[2];
                    //Join checken
                    if (splitmessage[1] == "JOIN")
                    {
                        Received(this, "JOIN", nick, what);
                        return;
                    }
                    //Prüfen ob der Raum verlassen wird
                    if (splitmessage[1] == "PART")
                    {
                        Received(this, "PART", nick, what);
                        return;
                    }
                    //Prüfen ob der Server verlassen wird
                    if (splitmessage[1] == "QUIT")
                    {
                        Received(this, "QUIT", nick, what);
                        return;
                    }
                    //Umbenennung Prüfen
                    if (splitmessage[1] == "NICK")
                    {
                        Received(this, "NICK", nick, what);
                        return;
                    }
                    //Kick Prüfen
                    if (splitmessage[1] == "KICK")
                    {
                        Received(this, "KICK", nick, what);
                        return;
                    }
                }
                //Verarbeitung einer Nachricht, eine Nachricht sollte 3 gesplittete Elemente im Array haben
                if (splitmessage.Length > 3)
                {
                    String[] nachricht = splitmessage[3].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                    if (nachricht.Length > 1)
                    {
                        if (nachricht[1].Contains("\u0001ACTION"))
                        {
                            nachricht[1] = nachricht[1].Replace("\u0001ACTION", "***" + nick).Replace("\u0001", "***");
                        }
                        Received(this, splitmessage[2], nick, nachricht[1]);
                    }
                    else
                    {
                        if (nachricht[0].Contains("\u0001ACTION"))
                        {
                            nachricht[0] = nachricht[0].Replace("\u0001ACTION", "***" + nick).Replace("\u0001", "***");
                        }
                        Received(this, splitmessage[2], nick, nachricht[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                log("Exception bei der Verarbeitung aufgefangen: " + ex.Message);
            }
        }
    }
}