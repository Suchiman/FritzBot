using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace freetzbot
{
    class irc
    {
        public delegate void ReceivedEventHandler(irc connection, String source, String nick, String message);
        public event ReceivedEventHandler Received;
        public String quit_message;

        private Thread empfangs_thread;
        private Boolean cancelthread;
        private string hostname;
        private int port;
        private String nickname;
        private TcpClient connection;
        private DateTime connecttime;
        private List<string> rooms = new List<string>();

        public irc(String server, int server_port, String nick)
        {
            hostname = server;
            port = server_port;
            nickname = nick;
            cancelthread = false;
            quit_message = "";
            empfangs_thread = new Thread(delegate() { empfangsthread(); });
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
            if (quit_message == "")
            {
                sendraw("QUIT");
            }
            else
            {
                sendraw("QUIT :" + quit_message);
            }
            cancelthread = true;
            log("Server " + hostname + " verlassen");
        }

        public void join(String channel)
        {
            if (channel.ToCharArray()[0] != '#')
            {
                channel = "#" + channel;
            }
            sendraw("JOIN " + channel);
            if (!rooms.Contains(channel)) rooms.Add(channel);
            log("Betrete Raum " + channel);
        }

        private void rejoin()
        {
            for (int i = 0; i < rooms.ToArray().Length; i++)
            {
                join(rooms[i]);
            }
        }

        public void leave(String channel)
        {
            if (channel.ToCharArray()[0] != '#')
            {
                channel = "#" + channel;
            }
            sendraw("PART " + channel);
            if (rooms.Contains(channel)) rooms.Remove(channel);
            log("Verlasse Raum " + channel);
        }

        private String[] splitat(String text, int length)
        {
            String[] gesplittet = new String[0];
            if (text.Length >= length)
            {
                Decimal loops = Math.Ceiling((Decimal)text.Length / (Decimal)length);
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

        private void log(String to_log)
        {
            Received(this, "LOG", "", to_log);
        }

        public Boolean running()
        {
            return empfangs_thread.IsAlive;
        }

        public TimeSpan uptime()
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(connecttime);
            return laufzeit;
        }

        public void sendmsg(String message, String receiver)
        {
            try
            {
                String methode = "PRIVMSG";
                StreamWriter stream = new StreamWriter(connection.GetStream(), Encoding.GetEncoding("iso-8859-1"));
                stream.AutoFlush = true;
                String[] tosend = splitat(message, 507 - (methode.Length + receiver.Length));
                for (int i = 0; i < tosend.Length; i++)
                {
                    stream.Write(methode + " " + receiver + " :" + tosend[i] + "\r\n");
                    log("An " + receiver + ": " + tosend[i]);
                }
            }
            catch (Exception ex)
            {
                log("Exception beim Senden einer Nachricht: " + ex);
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
                log("Exception beim Senden eines Kommandos: " + ex);
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
                        return;//Wenn ReadLine null ergibt ist die Verbindung abgerissen -> empfangsthread beenden
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
            //Rename: :Suchi!~email@91-67-134-206-dynip.superkabel.de NICK :testi
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