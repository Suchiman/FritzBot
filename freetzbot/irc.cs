using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace freetzbot
{
    class irc
    {
        public delegate void ReceivedEventHandler(irc connection, String source, String nick, String message);
        public event ReceivedEventHandler Received;
        public String quit_message;

        private Thread empfangs_thread;
        private Thread watchthread;
        private Boolean cancelthread;
        public string hostname;
        private int port;
        private String nickname;
        private TcpClient connection;
        private DateTime connecttime;
        public Boolean AutoReconnect;
        public int AutoReconnectIntervall;
        public List<string> rooms = new List<string>();

        public irc(String server, int server_port, String nick)
        {
            hostname = server;
            port = server_port;
            nickname = nick;
            cancelthread = false;
            quit_message = "";
            empfangs_thread = new Thread(delegate() { empfangsthread(); });
            watchthread = new Thread(delegate() { reconnect(); });
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
                    if (!empfangs_thread.IsAlive || !connection.Client.Connected)
                    {
                        log("Verbindung abgerissen, versuche Verbindung wiederherzustellen");
                        log("Versuch " + count);
                        while (!connect())
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

        public Boolean connect()
        {
            try
            {
                if (empfangs_thread.IsAlive)
                {
                    empfangs_thread.Abort();
                }
                connection = new TcpClient(hostname, port);
                empfangs_thread = new Thread(delegate() { empfangsthread(); });
                empfangs_thread.Start();
                sendraw("NICK " + nickname);
                sendraw("USER " + nickname + " 8 * :" + nickname);
                log("Verbindung mit Server " + hostname + " hergestellt");
                connecttime = DateTime.Now;
                rejoin();
                if (AutoReconnect && !watchthread.IsAlive)
                {
                    watchthread = new Thread(delegate() { reconnect(); });
                    watchthread.Start();
                }
                return true;
            }
            catch (Exception ex)
            {
                log("Exception beim Herstellen der Verbindung: " + ex);
                return false;
            }
        }

        public void disconnect()
        {
            watchthread.Abort();
            String output = "QUIT";
            if (quit_message != "")
            {
                output += " :" + quit_message;
            }
            sendraw(output);
            cancelthread = true;
            log("Server " + hostname + " verlassen");
        }

        public void join(String channel)
        {
            if (channel.ToCharArray()[0] != '#')
            {
                log("Diesem channel kann ich nicht joinen");
                return;
            }
            sendraw("JOIN " + channel);
            if (!rooms.Contains(channel)) rooms.Add(channel);
            log("Betrete Raum " + channel);
        }

        private void rejoin()
        {
            foreach (String room in rooms)
            {
                join(room);
            }
        }

        public void leave(String channel)
        {
            if (rooms.Contains(channel))
            {
                sendraw("PART " + channel);
                rooms.Remove(channel);
                log("Verlasse Raum " + channel);
            }
        }

        private String[] splitlength(String text, int length)
        {
            List<String> splitted = new List<String>();
            while (true)
            {
                if (length > text.Length)
                {
                    length = text.Length;
                }
                if (length >= text.Length)
                {
                    splitted.Add(text);
                    return splitted.ToArray();
                }
                splitted.Add(text.Remove(length));
                text = text.Remove(0, length);
            }
        }

        private void log(String to_log)
        {
            Received(this, "LOG", "", to_log);
        }

        public Boolean running()
        {
            return watchthread.IsAlive;
        }

        public TimeSpan uptime()
        {
            return DateTime.Now.Subtract(connecttime);
        }

        public void sendaction(String message, String receiver)
        {
            sendmsg("\u0001ACTION " + message + "\u0001", receiver);
        }

        public void sendmsg(String message, String receiver)
        {
            String output = "PRIVMSG " + receiver + " :";
            String[] tosend = splitlength(message, 500 - (output.Length));
            foreach (String send in tosend)
            {
                sendraw(output + send);
                log("An " + receiver + ": " + send);
            }
        }

        public void sendraw(String message)
        {
            try
            {
                StreamWriter stream = new StreamWriter(connection.GetStream(), Encoding.GetEncoding("iso-8859-1"));
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
            try
            {
                StreamReader stream = new StreamReader(connection.GetStream(), Encoding.GetEncoding("iso-8859-1")); 
                while (true)
                {
                    if (cancelthread)
                    {
                        return;
                    }
                    String Daten = stream.ReadLine();
                    if (Daten == null)
                    {
                        throw new Exception("connection lost");
                    }
                    Thread thread = new Thread(delegate() { process_respond(Daten); });
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                log("Exception im empfangsthread aufgefangen: " + ex.Message);
                return;
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
                        sendraw("PONG " + splitmessage[1]);
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
                        Received(this, splitmessage[2], nick, nachricht[1]);
                    }
                    else
                    {
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