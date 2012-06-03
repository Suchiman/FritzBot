using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FritzBot
{
    class ServerCollection
    {
        private List<Server> _servers;
        private Irc.ReceivedEventHandler _eventhandler;

        public Server this[String Hostname]
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
        /// Erstellt ein neues ServerCollection Objekt das multiple Server Objekte verwaltet
        /// </summary>
        public ServerCollection(Irc.ReceivedEventHandler theEventHandler)
        {
            _eventhandler = theEventHandler;
            _servers = new List<Server>();
            if (File.Exists("servers.cfg"))
            {
                Reload();
            }
        }

        /// <summary>
        /// Schreibt die Server Verbindungsdaten in die Konfigurationsdatei
        /// </summary>
        public void Flush()
        {
            Boolean failed = false;
            do
            {
                XmlTextWriter configfile = null;
                XmlSerializer serializer = null;
                try
                {
                    configfile = new XmlTextWriter("servers.cfg", Encoding.GetEncoding("iso-8859-1"));
                    configfile.Formatting = Formatting.Indented;
                    serializer = new XmlSerializer(_servers.GetType());
                    serializer.Serialize(configfile, _servers);
                    configfile.Flush();
                    failed = false;
                }
                catch
                {
                    failed = true;
                    Thread.Sleep(50);
                }
                finally
                {
                    configfile.Close();
                }
            } while (failed);
        }

        /// <summary>
        /// Liest alle Server aus der Konfigurationsdatei ein
        /// </summary>
        public void Reload()
        {
            if (File.Exists("servers.cfg"))
            {
                FileInfo FI = new FileInfo("servers.cfg");
                if (FI.Length > 0)
                {
                    StreamReader configfile = null;
                    XmlSerializer serializer = null;
                    try
                    {
                        configfile = new StreamReader("servers.cfg", Encoding.GetEncoding("iso-8859-1"));
                        serializer = new XmlSerializer(_servers.GetType());
                        _servers = (List<Server>)serializer.Deserialize(configfile);
                    }
                    finally
                    {
                        configfile.Close();
                    }
                }
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
        public void NewConnection(String HostName, int Port, String Nickname, String QuitMessage, List<String> Channels)
        {
            Server theConnection = new Server(HostName, Port, Nickname, QuitMessage, Channels);
            theConnection.Connect(_eventhandler);
            _servers.Add(theConnection);
            Flush();
        }

        /// <summary>
        /// Ruft die Connect Methode aller Verwalteten Verbindungen auf
        /// </summary>
        public void ConnectAll()
        {
            foreach (Server theServer in _servers)
            {
                if (!theServer.Connected)
                {
                    theServer.Connect(_eventhandler);
                }
            }
        }

        /// <summary>
        /// Gibt an ob mindestens ein Server verbunden ist
        /// </summary>
        public Boolean Connected
        {
            get
            {
                foreach (Server oneServer in _servers)
                {
                    if (oneServer.Connected)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Trennt die Verbindung zu allen Servern
        /// </summary>
        public void DisconnectAll()
        {
            foreach (Server theServer in _servers)
            {
                theServer.Disconnect();
            }
        }

        /// <summary>
        /// Gibt alle IRC Objekte der Verwalteten Server zurück
        /// </summary>
        /// <returns>Eine Liste aller IRC Objekte</returns>
        public List<Irc> GetAllConnections()
        {
            List<Irc> AllConnections = new List<Irc>();
            foreach (Server theServer in _servers)
            {
                AllConnections.Add(theServer.GetConnection);
            }
            return AllConnections;
        }

        /// <summary>
        /// Gibt eine Nachricht in allen Channels auf allen Servern bekannt
        /// </summary>
        /// <param name="message">Die Nachricht</param>
        public void AnnounceGlobal(String message)
        {
            foreach (Server theServer in _servers)
            {
                theServer.Announce(message);
            }
        }
    }

    public class Server : IXmlSerializable
    {
        private String _address;
        private int _port;
        private String _nick;
        private String _quit_message;
        private List<String> _channels;
        private Irc _connection;

        /// <summary>
        /// Erstellt ein neues Server Objekt welches die Verbindungsdaten zu einem Server verwaltet
        /// </summary>
        /// <param name="HostName">Der Hostname des Servers z.b. example.com</param>
        /// <param name="Port">Der numerische Port des Servers, der Standard IRC Port ist 6667</param>
        /// <param name="Nickname">Der Nickname den diese Verbindung verwenden soll</param>
        /// <param name="QuitMessage">Legt die Nachricht beim Verlassen des Servers fest</param>
        /// <param name="Channels">Alle Channels die Betreten werden sollen</param>
        public Server(String HostName, int Port, String Nickname, String QuitMessage, List<String> Channels)
        {
            _address = HostName;
            _port = Port;
            _nick = Nickname;
            _quit_message = QuitMessage;
            _channels = Channels;
        }

        /// <summary>
        /// Erstellt ein neues Server Objekt welches die Verbindungsdaten zu einem Server verwaltet
        /// </summary>
        /// <param name="HostName">Der Hostname des Servers z.b. example.com</param>
        /// <param name="Port">Der numerische Port des Servers, der Standard IRC Port ist 6667</param>
        /// <param name="Nickname">Der Nickname den der IRCbot für diese Verbindung verwenden soll</param>
        public Server(String HostName, int Port, String Nickname)
        {
            _address = HostName;
            _port = Port;
            _nick = Nickname;
            _quit_message = _nick;
            _channels = new List<String>();
        }
        
        /// <summary>
        /// Erstellt ein neues Server Objekt welches die Verbindungsdaten zu einem Server verwaltet
        /// </summary>
        public Server()
        {
            _address = "";
            _port = 6667;
            _nick = "";
            _quit_message = "";
            _channels = new List<String>();
        }

        /// <summary>
        /// Legt die Nachricht beim Verlassen des Servers fest
        /// </summary>
        public String QuitMessage
        {
            get
            {
                return _quit_message;
            }
            set
            {
                _quit_message = value;
                _connection._quitmessage = value;
            }
        }

        /// <summary>
        /// Der Hostname des Servers zu dem die Verbindung aufgebaut wird
        /// </summary>
        public String Hostname
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// Gibt an ob eine Verbindung mit dem Server herrgestellt ist
        /// </summary>
        public Boolean Connected
        {
            get
            {
                if (_connection == null)
                {
                    return false;
                }
                else if (_connection.Running)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Baut eine Verbindung mit den angegebenen Daten auf
        /// </summary>
        /// <param name="theEventHandler">Die Methode für das ReceivedEvent</param>
        public void Connect(Irc.ReceivedEventHandler theEventHandler)
        {
            _connection = new Irc(_address, _port, _nick);
            _connection._quitmessage = _quit_message;
            _connection.Received += theEventHandler;
            _connection._autoreconnect = true;
            _connection.Connect();
            foreach (String channel in _channels)
            {
                _connection.JoinChannel(channel);
            }
        }

        /// <summary>
        /// Trennt die Verbindung zum Angegebenen Server
        /// </summary>
        public void Disconnect()
        {
            _connection.Disconnect();
            _connection = null;
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
        public void Announce(String message)
        {
            foreach (String channel in _channels)
            {
                _connection.Sendmsg(message, channel);
            }
        }

        public XmlSchema GetSchema()
        {
            return (null);
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement("Server");
            _address = reader.ReadElementString("Hostname");
            _port = Convert.ToInt32(reader.ReadElementString("Port"));
            _nick = reader.ReadElementString("Nickname");
            _quit_message = reader.ReadElementString("QuitMessage");
            reader.ReadStartElement("Channels");
            XmlSerializer serializer = new XmlSerializer(_channels.GetType());
            _channels = (List<String>)serializer.Deserialize(reader);
            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Hostname", _address);
            writer.WriteElementString("Port", _port.ToString());
            writer.WriteElementString("Nickname", _nick);
            writer.WriteElementString("QuitMessage", _quit_message);
            writer.WriteStartElement("Channels");
            XmlSerializer serializer = new XmlSerializer(_channels.GetType());
            serializer.Serialize(writer, _channels);
            writer.WriteEndElement();
        }
    }
}
