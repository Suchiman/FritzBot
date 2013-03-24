using FritzBot.DataModel.IRC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Core
{
    public class ServerManager
    {
        private static ServerManager instance;
        private List<Server> _servers;
        public event Action<IRCEvent> MessageReceivedEvent;

        public static ServerManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ServerManager();
            }
            return instance;
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
        }

        public ServerManager()
        {
            using (DBProvider db = new DBProvider())
            {
                _servers = db.Query<Server>().ToList();
                foreach (Server srv in _servers)
                {
                    srv.OnIRCEvent += ReceivedInterceptor;
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
            Server server = new Server()
            {
                Hostname = HostName,
                Port = Port,
                Nickname = Nickname,
                QuitMessage = QuitMessage,
                Channels = Channels
            };
            server.OnIRCEvent += ReceivedInterceptor;
            _servers.Add(server);
            using (DBProvider db = new DBProvider())
            {
                db.SaveOrUpdate(server);
            }
            return server;
        }

        public void Remove(Server server)
        {
            server.Disconnect();
            _servers.Remove(server);
            using (DBProvider db = new DBProvider())
            {
                db.Remove(server);
            }
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
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Nickname { get; set; }
        private string _QuitMessage;
        public string QuitMessage
        {
            get
            {
                return _QuitMessage;
            }
            set
            {
                _QuitMessage = value;
                if (_connection != null)
                {
                    _connection.QuitMessage = value;
                }
            }
        }
        public List<string> Channels { get; set; }
        private Irc _connection;
        public event Action<IRCEvent> OnIRCEvent;
        private bool _running;

        /// <summary>
        /// Erstellt ein neues Server Objekt welches die Verbindungsdaten zu einem Server verwaltet
        /// </summary>
        public Server()
        {
            Channels = new List<string>();
        }

        public void JoinChannel(string channel)
        {
            if (!Channels.Contains(channel))
            {
                Channels.Add(channel);
                _connection.JoinChannel(channel);
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
            _connection.ReceivedEvent += _connection_ReceivedEvent;
            _connection.Connect();
            foreach (string channel in Channels)
            {
                _connection.JoinChannel(channel);
            }
            _running = true;
        }

        void _connection_ReceivedEvent(IRCEvent obj)
        {
            Action<IRCEvent> ev = OnIRCEvent;
            if (ev != null)
            {
                ev(obj);
                if (obj is Quit && Nickname != _connection.Nickname && (obj as Quit).Nickname == Nickname)
                {
                    _connection.Nickname = Nickname;
                }
            }
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