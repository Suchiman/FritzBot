using FritzBot.Core;
using FritzBot.DataModel.IRC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Core
{
    public class ServerManager
    {
        private static ServerManager instance;
        private List<Server> _servers;
        private XElement _storage;
        public event Action<IRCEvent> MessageReceivedEvent;

        public static ServerManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ServerManager(XMLStorageEngine.GetManager().GetElement("Servers"));
            }
            return instance;
        }

        /// <summary>
        /// Erstellt ein neues ServerCollection Objekt das multiple Server Objekte verwaltet
        /// </summary>
        public ServerManager(XElement DataStorage)
        {
            _storage = DataStorage;
            _servers = _storage.Elements("Server").Select(x => new Server(x, ReceivedInterceptor)).ToList<Server>();
        }

        public Server this[string Hostname]
        {
            get
            {
                foreach (Server oneServer in _servers)
                {
                    if (oneServer.Hostname == Hostname)
                    {
                        return oneServer;
                    }
                }
                throw new ArgumentException("Unknown Server");
            }
            set
            {
                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].Hostname == Hostname)
                    {
                        if (value == null)
                        {
                            _servers[i].Disconnect();
                            _servers[i] = null;
                            _servers.RemoveAt(i);
                        }
                        else
                        {
                            _servers[i] = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Zwischenmann der alle Events übernimmt und das dem Manager zugewiesene Event aufruft.
        /// </summary>
        private void ReceivedInterceptor(IRCEvent Daten)
        {
            Action<IRCEvent> handler = MessageReceivedEvent;
            if (handler != null)
            {
                handler(Daten);
            }
        }

        /// <summary>
        /// Die Anzahl der verwalteten Verbindungen
        /// </summary>
        public int ConnectionCount
        {
            get
            {
                return _servers.Count;
            }
        }

        /// <summary>
        /// Erstellt eine neue IRC Verbindung zu der Verbunden wird und zur Verwaltung hinzugefügt wird
        /// </summary>
        /// <param name="HostName">Der Hostname des Servers z.b. example.com</param>
        /// <param name="Port">Der numerische Port des Servers, der Standard IRC Port ist 6667</param>
        /// <param name="Nickname">Der Nickname den der IRCbot für diese Verbindung verwenden soll</param>
        /// <param name="QuitMessage">Legt die Nachricht beim Verlassen des Servers fest</param>
        /// <param name="Channels">Alle Channels die Betreten werden sollen</param>
        public Server NewConnection(string HostName, int Port, string Nickname, string QuitMessage, List<string> Channels)
        {
            XElement channels = new XElement("Channels");
            foreach (string channel in Channels)
            {
                channels.Add(new XElement("Channel", channel));
            }
            XElement server = new XElement("Server",
                new XElement("Hostname", HostName),
                new XElement("Port", Port),
                new XElement("Nickname", Nickname),
                new XElement("QuitMessage", QuitMessage),
                channels
                );
            _storage.Add(server);
            Server theConnection = new Server(server, ReceivedInterceptor);
            _servers.Add(theConnection);
            return theConnection;
        }

        public void Remove(Server server)
        {
            server.Remove();
            this._servers.Remove(server);
        }

        /// <summary>
        /// Ruft die Connect Methode aller Verwalteten Verbindungen auf
        /// </summary>
        public void ConnectAll()
        {
            foreach (Server theServer in _servers.Where(x => !x.Connected))
            {
                theServer.Connect();
            }
        }

        /// <summary>
        /// Gibt an ob mindestens ein Server verbunden ist
        /// </summary>
        public bool Connected
        {
            get
            {
                return _servers.Any(x => x.Connected);
            }
        }

        /// <summary>
        /// Trennt die Verbindung zu allen Servern
        /// </summary>
        public void DisconnectAll()
        {
            foreach (Server theServer in _servers.Where(x => x.Connected))
            {
                theServer.Disconnect();
            }
        }

        /// <summary>
        /// Gibt alle IRC Objekte der Verwalteten Server zurück
        /// </summary>
        /// <returns>Eine Liste aller IRC Objekte</returns>
        public IEnumerable<Irc> GetAllConnections()
        {
            return _servers.Select(x => x.GetConnection);
        }

        /// <summary>
        /// Gibt eine Nachricht in allen Channels auf allen Servern bekannt
        /// </summary>
        /// <param name="message">Die Nachricht</param>
        public void AnnounceGlobal(string message)
        {
            foreach (Server theServer in _servers)
            {
                theServer.Announce(message);
            }
        }
    }

    public class Server
    {
        /// <summary>
        /// Der Hostname des Servers zu dem die Verbindung aufgebaut wird
        /// </summary>
        public string Hostname
        {
            get
            {
                return _serverElement.Element("Hostname").Value;
            }
            set
            {
                _serverElement.Element("Hostname").Value = value;
            }
        }
        public int Port
        {
            get
            {
                return Convert.ToInt32(_serverElement.Element("Port").Value);
            }
            set
            {
                _serverElement.Element("Port").Value = value.ToString();
            }
        }
        public string Nickname
        {
            get
            {
                return _serverElement.Element("Nickname").Value;
            }
            set
            {
                _serverElement.Element("Nickname").Value = value;
            }
        }
        public string QuitMessage
        {
            get
            {
                return _serverElement.Element("QuitMessage").Value;
            }
            set
            {
        
                _serverElement.Element("QuitMessage").Value = value;
                _connection.QuitMessage = value;
            }
        }
        public ReadOnlyCollection<string> Channels
        {
            get
            {
                return _serverElement.Element("Channels").Elements("Channel").Select(x => x.Value).ToList<string>().AsReadOnly();
            }
        }
        private Irc _connection;
        private Action<IRCEvent> IRCEventHandler;
        private bool _running;
        private XElement _serverElement;

        /// <summary>
        /// Erstellt ein neues Server Objekt welches die Verbindungsdaten zu einem Server verwaltet
        /// </summary>
        public Server(XElement serverElement, Action<IRCEvent> theEventHandler)
        {
            IRCEventHandler = theEventHandler;
            _serverElement = serverElement;
        }

        public void JoinChannel(string channel)
        {
            XElement channels = _serverElement.Element("Channels");
            if (channels.Elements("Channel").FirstOrDefault(x => x.Value == channel) == null)
            {
                channels.Add(new XElement("Channel", channel));
            }
        }

        /// <summary>
        /// Gibt an ob eine Verbindung mit dem Server herrgestellt ist
        /// </summary>
        public bool Connected
        {
            get
            {
                return _running;
            }
        }

        /// <summary>
        /// Baut eine Verbindung mit den angegebenen Daten auf
        /// </summary>
        /// <param name="theEventHandler">Die Methode für das ReceivedEvent</param>
        public void Connect()
        {
            _connection = new Irc(Hostname, Port, Nickname);
            _connection.ConnectionLost += new EventHandler(_connection_ConnectionLost);
            _connection.QuitMessage = QuitMessage;
            _connection.ReceivedEvent += IRCEventHandler;
            _connection.Connect();
            foreach (string channel in Channels)
            {
                _connection.JoinChannel(channel);
            }
            _running = true;
        }

        /// <summary>
        /// Verarbeitet das ConnectionLost event und baut eine neue Verbindung auf
        /// </summary>
        /// <param name="sender">Das Objekt, welches das Event aufgerufen hat</param>
        /// <param name="e">EventArgs</param>
        void _connection_ConnectionLost(object sender, EventArgs e)
        {
            toolbox.Logging("Verbindung zu Server " + Hostname + " verloren");
            _connection.Dispose();
            Connect();
        }

        /// <summary>
        /// Trennt die Verbindung zum Angegebenen Server
        /// </summary>
        public void Disconnect()
        {
            _connection.Disconnect();
            _connection = null;
            _running = false;
            toolbox.Logging("Verbindung zu Server " + Hostname + " getrennt");
        }

        /// <summary>
        /// Trennt und entfernt diese Verbindung dauerhaft
        /// </summary>
        public void Remove()
        {
            if (_running)
            {
                Disconnect();
            }
            _serverElement.Remove();
        }

        /// <summary>
        /// Gibt das der Verbindung zugrunde liegende IRC Objekt zurück
        /// </summary>
        public Irc GetConnection
        {
            get
            {
                return _connection;
            }
        }

        /// <summary>
        /// Gibt eine Nachricht in allen Channels bekannt
        /// </summary>
        /// <param name="message">Die Nachricht</param>
        public void Announce(string message)
        {
            foreach (string channel in Channels)
            {
                _connection.Sendmsg(message, channel);
            }
        }
    }
}