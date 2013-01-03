using FritzBot.DataModel.IRC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using FritzBot.Core;
using System.Globalization;

namespace FritzBot
{
    public class Irc : IDisposable
    {
        public event Action<IRCEvent> ReceivedEvent;
        public event EventHandler ConnectionLost;

        private string _quitmessage;
        private Thread _connectionHandlerThread;
        private string _server;
        private int _port;
        private string _nick;
        private TcpClient _connection;
        private DateTime _connecttime;
        private bool _disconnecting;
        public Encoding CharEncoding { get; set; }
        public bool Ready { get; private set; }
        public List<Channel> Channels { get; private set; }
        public string MOTD { get; private set; }

        public Irc(string server, int port, string nick)
        {
            _server = server;
            _port = port;
            _nick = nick;
            _quitmessage = "";
            _connectionHandlerThread = null;
            _connection = null;
            _connecttime = DateTime.MinValue;
            _disconnecting = false;
            CharEncoding = Encoding.GetEncoding("iso-8859-1");
            Ready = false;
            Channels = new List<Channel>();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Connect()
        {
            InitConnection();
            WaitForReady();
            toolbox.Logging("Verbindung mit Server " + _server + " hergestellt");
            _connecttime = DateTime.Now;
        }

        private void InitConnection()
        {
            _connectionHandlerThread = new Thread(new ThreadStart(ConnectionHandler));
            _connectionHandlerThread.Name = "EmpfangsThread " + _server;
            _connectionHandlerThread.Start();
        }

        private void DeinitReceiver()
        {
            if (_connectionHandlerThread != null)
            {
                if (_connectionHandlerThread.IsAlive)
                {
                    _connectionHandlerThread.Abort();
                }
                _connectionHandlerThread = null;
            }
        }

        private bool Authenticate()
        {
            SetNick(_nick);
            return Sendraw("USER " + _nick + " 8 * :" + _nick);
        }

        /// <summary>
        /// Unterbricht den Thread bis die Verbindung bereit
        /// </summary>
        public void WaitForReady()
        {
            WaitForReady(Int32.MaxValue);
        }

        /// <summary>
        /// Unterbricht den Thread bis die Verbindung bereit ist oder das Timeout erreicht wurde
        /// </summary>
        /// <param name="timeout">Zeit in Sekunden bis zum Timeout</param>
        public void WaitForReady(int timeout)
        {
            int counter = 0;
            while (!Ready)
            {
                Thread.Sleep(500);
                counter++;
                if ((counter / 2) > timeout)
                {
                    throw new TimeoutException("Die Verbindung wurde nicht innerhalb des gegebenen Timeouts bereit");
                }
            }
        }

        public void Disconnect()
        {
            _disconnecting = true;
            if (_connection != null)
            {
                if (_connection.Connected)
                {
                    Sendraw("QUIT" + _quitmessage);
                }
            }
            DeinitReceiver();
            _connection.Close();
            _connection = null;
        }

        public Channel JoinChannel(string name)
        {
            Sendraw("JOIN " + name);
            toolbox.Logging("Betrete Raum " + name);
            Channel chan = new Channel(this, name);
            Channels.Add(chan);
            return chan;
        }

        public Channel GetChannel(string name)
        {
            return Channels.Single(x => x.ChannelName == name);
        }

        public string Nickname
        {
            get
            {
                return _nick;
            }
            set
            {
                SetNick(value);
            }
        }

        public string QuitMessage
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

        private void SetNick(string Nick)
        {
            Sendraw("NICK " + Nick);
            _nick = Nick;
        }

        public void Leave(string channel)
        {
            if (Sendraw("PART " + channel))
            {
                toolbox.Logging("Verlasse Raum " + channel);
            }
            else
            {
                toolbox.Logging("Fehler, konnte den Raum nicht verlassen");
            }
        }

        private string[] SplitLength(string text, int length)
        {
            List<string> splitted = new List<string>();
            while (true)
            {
                if (text.Length < length)
                {
                    splitted.Add(text);
                    return splitted.ToArray();
                }
                int temp = text.Substring(0, length).LastIndexOf(' ');
                if (temp == -1)
                {
                    temp = length - 2;
                }
                splitted.Add(text.Substring(0, temp));
                text = text.Remove(0, temp).Trim();
            }
        }

        public TimeSpan Uptime
        {
            get
            {
                return DateTime.Now.Subtract(_connecttime);
            }
        }

        public string GetCTCPString(string str)
        {
            return (char)1 + str + (char)1;
        }

        public string MessageBuilder(string method, string receiver, string content)
        {
            return String.Format("{0} {1} :{2}", method, receiver, content);
        }

        public bool Sendaction(string message, string receiver)
        {
            return Sendmsg(GetCTCPString(message), receiver);
        }

        public bool Sendnotice(string message, string receiver)
        {
            return Send("NOTICE", message, receiver);
        }

        public bool Sendmsg(string message, string receiver)
        {
            return Send("PRIVMSG", message, receiver);
        }

        private bool Send(string method, string message, string receiver)
        {
            WaitForReady();
            string output = MessageBuilder(method, receiver, "");
            string[] tosend = SplitLength(message, 500 - (output.Length));
            foreach (string send in tosend)
            {
                if (!Sendraw(output + send))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Sendraw(string message)
        {
            if (_connection != null)
            {
                if (_connection.Connected)
                {
                    try
                    {
                        StreamWriter stream = new StreamWriter(_connection.GetStream(), CharEncoding);
                        stream.AutoFlush = true;
                        stream.Write(message + "\r\n");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        toolbox.Logging("Sendraw failed: " + ex.Message);
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
            try
            {
                ReceiveDataLoop();
            }
            finally
            {
                if (!_disconnecting)
                {
                    ConnectionLost.BeginInvoke(this, EventArgs.Empty, null, null);
                }
            }
        }

        private void ReceiveDataLoop()
        {
            StreamReader stream = new StreamReader(_connection.GetStream(), CharEncoding);
            while (true)
            {
                try
                {
                    string Daten = stream.ReadLine();
                    if (String.IsNullOrEmpty(Daten))
                    {
                        return; //Connection Lost
                    }
                    if (Daten.StartsWith("PING"))
                    {
                        Sendraw("PONG " + Daten.Substring(Daten.IndexOf(':')));
                        continue;
                    }
                    ProcessRespond(Daten);
                }
                catch
                {
                    return;
                }
            }
        }

        public void RaiseReceived(IRCEvent Daten)
        {
            Action<IRCEvent> received = ReceivedEvent;
            if (received != null)
            {
                received.BeginInvoke(Daten, null, null);
            }
        }

        Regex MessageRegex = new Regex(@":(?<nick>[A-Za-z0-9<\-_\[\]\\\^{}]{2,15})!~?(?<realname>.*)@(?<host>.*) (?<action>[A-z]+) (?<origin>.*) :(?<message>.*)", RegexOptions.Compiled);
        Regex GenericIRCAction = new Regex(@":(?<sender>.*) (?<action>\d\d\d)( \*)? (?<nick>[A-Za-z0-9<\-_\[\]\\\^{}]{2,15}) :?(?<message>.*)", RegexOptions.Compiled);

        private void ProcessRespond(string message)
        {
            //Beispiel einer v6 Nachricht: ":User!~info@2001:67c:1401:2100:5ab0:35fa:fe76:feb0 PRIVMSG #eingang :hehe"
            //Beispiel einer Nachricht: ":Suchiman!~Suchiman@Robin-PC PRIVMSG #eingang :hi"
            //Beispiel einer PRIVMSG: ":Suchi!~email@91-67-134-206-dynip.superkabel.de PRIVMSG Suchiman :hi"
            //Beispiel eines Joins: ":Suchiman!~robinsue@91-67-134-206-dynip.superkabel.de JOIN :#eingang"
            //Action: ":FritzBot!~FritzBot@91-67-134-206-dynip.superkabel.de PRIVMSG #fritzbox :\001ACTION rennt los zum channel #eingang\001"
            //Rename: :Suchi!~email@91-67-134-206-dynip.superkabel.de NICK :testi
            //KICK: :Suchiman!~email@91-67-134-206-dynip.superkabel.de KICK #fritzbox FritzBot :Suchiman
            //Ping anforderung des Servers: "PING :fritz.box"
            //WHO #fritzbot ":calvino.freenode.net 352 TESTIBOA #fritzbot uid3778 gateway/web/irccloud.com/x-emoebkdpqrellyuq verne.freenode.net Suchiman H :0 Suchiman"
            //WHO Suchiman ":niven.freenode.net 352 TESTIBOA * uid3778 gateway/web/irccloud.com/x-wfssqeobhqhlpxba adams.freenode.net Suchiman H :0 Suchiman"
            
            Match regex;

            regex = GenericIRCAction.Match(message);
            if (regex.Success)
            {
                string sender = regex.Groups["sender"].Value;
                string action = regex.Groups["action"].Value;
                string nick = regex.Groups["nick"].Value;
                string messie = regex.Groups["message"].Value;

                switch (action)
                {
                    case "433": //Nickname in use
                        Nickname = Nickname + "_"; //Sendet die Nick Anfrage neu
                        return;
                    case "375": //MOTD beginn
                        MOTD = String.Empty;
                        return;
                    case "372": //MOTD
                        MOTD += messie.Substring(messie.IndexOf('-') + 2) + Environment.NewLine;
                        return;
                    case "376": //MOTD Ende
                        Ready = true;
                        return;
                    case "352": //WHO Reply
                        Match M352 = Regex.Match(messie, @"(?<channel>#.*) .* .* .* (?<nick>.*) (?<modes>.*) :(?<hopcount>\d) (?<realname>.*)");
                        Channel M352chan = GetChannel(M352.Groups["channel"].Value);
                        User M352User = UserManager.GetInstance()[M352.Groups["nick"].Value];
                        M352User.LastUsedNick = M352.Groups["nick"].Value;
                        if (M352chan.EndOfWho)
                        {
                            M352chan.User.Clear();
                            M352chan.EndOfWho = false;
                        }
                        M352chan.User.Add(M352User);
                        return;
                    case "315": //WHO End
                        Match M315 = Regex.Match(messie, @"(?<channel>#.*) :");
                        Channel M315chan = GetChannel(M315.Groups["channel"].Value);
                        M315chan.EndOfWho = true;
                        return;
                    default:
                        return;
                }
            }

            regex = MessageRegex.Match(message);
            if (regex.Success)
            {
                string nick = regex.Groups["nick"].Value;
                string realname = regex.Groups["realname"].Value;
                string host = regex.Groups["host"].Value;
                string action = regex.Groups["action"].Value;
                string origin = regex.Groups["origin"].Value;
                string messie = regex.Groups["message"].Value;

                Match MCTCP = Regex.Match(messie, GetCTCPString("(?<action>[A-z]*)(?<data> .*)?"));
                if (MCTCP.Success) //CTCP Special Character
                {
                    string CTCPAction = MCTCP.Groups["action"].Value;
                    switch (CTCPAction)
                    {
                        case "ACTION":
                            RaiseReceived(new ircMessage(nick, origin, MCTCP.Groups["data"].Value, this));
                            return;
                        case "CLIENTINFO":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("CLIENTINFO Supported CTCP Commands: VERSION, TIME, SOURCE, USERINFO, CLIENTINFO")));
                            return;
                        case "FINGER":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("FINGER " + Nickname)));
                            return;
                        case "PING":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("PING" + MCTCP.Groups["data"].Value)));
                            return;
                        case "SOURCE":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("SOURCE svn://suchiman.selfip.org")));
                            return;
                        case "TIME":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("TIME " + DateTime.Now.ToString("r"))));
                            return;
                        case "USERINFO":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("USERINFO Ich bin ein automatisch denkendes Wesen auch bekannt als Bot")));
                            return;
                        case "VERSION":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString("VERSION FritzBot:v3:" + Environment.OSVersion.Platform.ToString())));
                            return;
                        case "ERRMSG":
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString(String.Format("ERRMSG {0} :OK", messie.Substring(1, messie.Length - 2)))));
                            return;
                        default:
                            Sendraw(MessageBuilder("NOTICE", nick, GetCTCPString(String.Format("ERRMSG {0} :unknown query", messie.Substring(1, messie.Length - 2)))));
                            return;
                    }
                }

                switch (action)
                {
                    case "NOTICE":
                    case "PRIVMSG":
                        RaiseReceived(new ircMessage(nick, origin, messie, this));
                        return;
                    case "JOIN":
                        Channel Jchan = GetChannel(origin);
                        Jchan.User.Add(UserManager.GetInstance()[nick]);
                        RaiseReceived(new Join(this)
                        {
                            Channel = Jchan,
                            Nickname = nick
                        });
                        return;
                    case "QUIT":
                        User usr = UserManager.GetInstance()[nick];
                        Channels.ForEach(x => x.User.Remove(usr));
                        RaiseReceived(new Quit(this)
                        {
                            Nickname = nick
                        });
                        return;
                    case "PART":
                        Channel Pchan = GetChannel(origin);
                        Pchan.User.Remove(UserManager.GetInstance()[nick]);
                        RaiseReceived(new Part(this)
                        {
                            Channel = Pchan,
                            Nickname = nick
                        });
                        return;
                    case "NICK":
                        RaiseReceived(new Nick(this)
                        {
                            NewNickname = messie,
                            Nickname = nick
                        });
                        return;
                    case "KICK":
                        Channel Kchan = GetChannel(origin);
                        Kchan.User.Remove(UserManager.GetInstance()[messie]);
                        RaiseReceived(new Kick(this)
                        {
                            Channel = Kchan,
                            KickedBy = nick,
                            Nickname = messie
                        });
                        return;
                }
            }
        }
    }
}