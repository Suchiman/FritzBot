using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FritzBot
{
    public class Irc
    {
        public delegate void ReceivedEventHandler(Irc connection, String source, String nick, String message);
        public event ReceivedEventHandler Received;
        private String _quitmessage;

        private Thread _empfangsthread;
        private Thread _watchthread;
        private String _server;
        private int _port;
        private String _nick;
        private TcpClient _connection;
        private DateTime _connecttime;
        public Boolean AutoReconnect { get; set; }
        public Boolean Ready { get; private set; }
        public int AutoReconnectIntervall;

        public Irc(String server, int port, String nick)
        {
            _server = server;
            _port = port;
            _nick = nick;
            _quitmessage = "";
            _empfangsthread = null;
            _watchthread = null;
            _connection = null;
            _connecttime = DateTime.MinValue;
            AutoReconnect = false;
            Ready = false;
            AutoReconnectIntervall = 5000;
        }

        public void Connect()
        {
            DeinitReceiver();
            _connection = null;
            InitConnection();
            do
            {
                Thread.Sleep(100);
            } while (!Ready);
            Log("Verbindung mit Server " + _server + " hergestellt");
            _connecttime = DateTime.Now;
            InitAutoReconnect();
        }

        private void InitConnection()
        {
            _empfangsthread = new Thread(new ThreadStart(ConnectionHandler));
            _empfangsthread.Name = "EmpfangsThread " + _server;
            _empfangsthread.Start();
        }

        private void DeinitReceiver()
        {
            if (_empfangsthread != null)
            {
                if (_empfangsthread.IsAlive)
                {
                    _empfangsthread.Abort();
                }
                _empfangsthread = null;
            }
        }

        private void InitAutoReconnect()
        {
            if (_watchthread == null)
            {
                _watchthread = new Thread(new ThreadStart(WatchThread));
                _watchthread.Name = "WatchThread " + _server;
                _watchthread.Start();
            }
        }

        private void DeinitAutoReconnect()
        {
            if (_watchthread != null)
            {
                if (_watchthread.IsAlive)
                {
                    _watchthread.Abort();
                }
                _watchthread = null;
            }
        }

        private Boolean Authenticate()
        {
            SetNick(_nick);
            return Sendraw("USER " + _nick + " 8 * :" + _nick);
        }

        private void WatchThread()
        {
            while (true)
            {
                if (_empfangsthread != null)
                {
                    if (!_empfangsthread.IsAlive)
                    {
                        Log("Connection lost, trying reconnect");
                        Connect();
                    }
                }
                Thread.Sleep(AutoReconnectIntervall);
            }
        }

        public void Disconnect()
        {
            DeinitAutoReconnect();
            if (_connection != null)
            {
                if (_connection.Connected)
                {
                    Sendraw("QUIT" + _quitmessage);
                }
            }
            DeinitReceiver();
            _connection = null;
            Log("Server " + _server + " verlassen");
        }

        public void JoinChannel(String channel)
        {
            Sendraw("JOIN " + channel);
            Log("Betrete Raum " + channel);
        }

        public String Nickname
        {
            get
            {
                return _nick;
            }
            set
            {
                SetNick(value);
                _nick = value;
            }
        }

        public String QuitMessage
        {
            get
            {
                if (_quitmessage != "")
                {
                    return _quitmessage.Substring(2);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (value != "")
                {
                    _quitmessage = " :" + value;
                }
                else
                {
                    _quitmessage = "";
                }
            }
        }

        private void SetNick(String Nick)
        {
            Sendraw("NICK " + Nick);
        }

        public void Leave(String channel)
        {
            if (Sendraw("PART " + channel))
            {
                Log("Verlasse Raum " + channel);
            }
            else
            {
                Log("Fehler, konnte den Raum nicht verlassen");
            }
        }

        private String[] SplitLength(String text, int length)
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

        private void Log(String to_log)
        {
            Received(this, "LOG", "", to_log);
        }

        public Boolean Running
        {
            get
            {
                if (_watchthread != null)
                {
                    return _watchthread.IsAlive;
                }
                return false;
            }
        }

        public TimeSpan Uptime
        {
            get
            {
                return DateTime.Now.Subtract(_connecttime);
            }
        }

        public Boolean Sendaction(String message, String receiver)
        {
            return Sendmsg("\u0001ACTION " + message + "\u0001", receiver);
        }

        public Boolean Sendmsg(String message, String receiver)
        {
            while (!Ready)
            {
                Thread.Sleep(100);
            }
            String output = "PRIVMSG " + receiver + " :";
            String[] tosend = SplitLength(message, 500 - (output.Length));
            foreach (String send in tosend)
            {
                if (!Sendraw(output + send))
                {
                    return false;
                }
                Log("An " + receiver + ": " + send);
            }
            return true;
        }

        public Boolean Sendraw(String message)
        {
            if (_connection != null)
            {
                if (_connection.Connected)
                {
                    try
                    {
                        StreamWriter stream = new StreamWriter(_connection.GetStream(), Encoding.GetEncoding("iso-8859-1"));
                        stream.AutoFlush = true;
                        stream.Write(message + "\r\n");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log("Sendraw failed: " + ex.Message);
                    }
                }
            }
            return false;
        }

        private void ConnectionHandler()
        {
            do
            {
                _connection = new TcpClient(_server, _port);
            } while (!Authenticate());
            StreamReader stream = new StreamReader(_connection.GetStream(), Encoding.GetEncoding("iso-8859-1"));
            while (true)
            {
                String Daten = "";
                try
                {
                    Daten = stream.ReadLine();
                }
                catch (Exception ex)
                {
                    Log("Receiving Data failed: " + ex.Message);
                    return;
                }
                if (String.IsNullOrEmpty(Daten))
                {
                    Log("connection lost");
                    return;
                }
                Thread thread = new Thread(delegate() { ProcessRespond(Daten); });
                thread.Name = "Process " + _server;
                thread.Start();
            }
        }

        private void ProcessRespond(String message)
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
                    if (splitmessage[1] == "376")
                    {
                        Ready = true;
                    }
                    String[] nachricht = splitmessage[3].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                    if (nachricht.Length > 1)
                    {
                        if (nachricht[1].Contains("\u0001ACTION"))
                        {
                            nachricht[1] = nachricht[1].Replace("\u0001ACTION", "***" + nick).Replace("\u0001", "***");
                        }
                        if (nachricht[1].Contains("&#x2;"))
                        {
                            nachricht[1] = nachricht[1].Replace("&#x2;", "*");
                        }
                        Received(this, splitmessage[2], nick, nachricht[1]);
                    }
                    else
                    {
                        if (nachricht[0].Contains("\u0001ACTION"))
                        {
                            nachricht[0] = nachricht[0].Replace("\u0001ACTION", "***" + nick).Replace("\u0001", "***");
                        }
                        if (nachricht[0].Contains("&#x2;"))
                        {
                            nachricht[0] = nachricht[0].Replace("&#x2;", "*");
                        }
                        Received(this, splitmessage[2], nick, nachricht[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception bei der Verarbeitung aufgefangen: " + ex.Message);
            }
        }
    }
}