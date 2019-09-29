﻿using FritzBot.Database;
using Meebey.SmartIrc4net;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FritzBot.Core
{
    /// <summary>
    /// Der ServerManger verwaltet die ServerConnetions
    /// </summary>
    public static class ServerManager
    {
        private static readonly List<ServerConnection> _servers;

        public static IEnumerable<ServerConnection> Servers => _servers;

        /// <summary>
        /// Erstellt eine neue Instanz des ServerManagers und lädt die ServerConnetions aus der Datenbank
        /// </summary>
        static ServerManager()
        {
            using (var context = new BotContext())
            {
                _servers = context.Servers.Include(x => x.Channels).AsEnumerable().Select(x => new ServerConnection(x)).ToList();
            }
        }

        /// <summary>
        /// Die Anzahl der verwalteten Verbindungen
        /// </summary>
        public static int ConnectionCount => _servers.Count;

        /// <summary>
        /// Erstellt eine neue IRC Verbindung zu der Verbunden wird und zur Verwaltung hinzugefügt wird
        /// </summary>
        /// <param name="HostName">Der Hostname des Servers z.b. example.com</param>
        /// <param name="Port">Der numerische Port des Servers, der Standard IRC Port ist 6667</param>
        /// <param name="Nickname">Der Nickname den der IRCbot für diese Verbindung verwenden soll</param>
        /// <param name="QuitMessage">Legt die Nachricht beim Verlassen des Servers fest</param>
        /// <param name="Channels">Alle Channels die Betreten werden sollen</param>
        public static ServerConnection NewConnection(string HostName, int Port, string Nickname, string QuitMessage, List<string> Channels)
        {
            Server server = new Server
            {
                Address = HostName,
                Port = Port,
                Nickname = Nickname,
                QuitMessage = QuitMessage,
                Channels = Channels.Select(x => new ServerChannel { Name = x }).ToList()
            };

            using (var context = new BotContext())
            {
                context.Servers.Add(server);
                context.SaveChanges();
            }

            ServerConnection serverConnetion = new ServerConnection(server);
            _servers.Add(serverConnetion);
            return serverConnetion;
        }

        /// <summary>
        /// Trennt die Verbindung zu dieser ServerConnetion und entfernt sie aus der Datenbank
        /// </summary>
        /// <param name="serverConnetion"></param>
        public static void Remove(ServerConnection serverConnetion)
        {
            serverConnetion.Disconnect();
            _servers.Remove(serverConnetion);
            using (var context = new BotContext())
            {
                context.Servers.Attach(serverConnetion.Settings);
                context.Servers.Remove(serverConnetion.Settings);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Ruft die Connect Methode aller Verwalteten Verbindungen auf
        /// </summary>
        public static void ConnectAll()
        {
            foreach (ServerConnection theServer in _servers.Where(x => !x.Connected))
            {
                try
                {
                    theServer.Connect();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Herstellen der Verbindung zu Server {ServerAddress} fehlgeschlagen", theServer.Settings.Address);
                }
            }
        }

        /// <summary>
        /// Gibt an ob mindestens eine ServerConnetion verbunden ist
        /// </summary>
        public static bool Connected => _servers.Any(x => x.Connected);

        /// <summary>
        /// Trennt die Verbindung zu allen Servern
        /// </summary>
        public static void DisconnectAll()
        {
            foreach (ServerConnection theServer in _servers.Where(x => x.Connected))
            {
                theServer.Disconnect();
            }
        }

        /// <summary>
        /// Gibt eine Nachricht in allen Channels auf allen Servern bekannt
        /// </summary>
        /// <param name="message">Die Nachricht</param>
        public static void AnnounceGlobal(string message)
        {
            foreach (ServerConnection theServer in _servers.Where(x => x.Connected))
            {
                theServer.Announce(message);
            }
        }
    }

    /// <summary>
    /// Ein ServerConnetion kapselt die Verbindung zu einem IRC Server und speichert die Verbindungsdaten
    /// </summary>
    public class ServerConnection
    {
        /// <summary>
        /// Wird ausgelöst, wenn ein User einen Channel betritt
        /// </summary>
        public static event JoinEventHandler? OnJoin;

        /// <summary>
        /// Wird ausgelöst, wenn ein User einen Channel verlässt
        /// </summary>
        public static event PartEventHandler? OnPart;

        /// <summary>
        /// Wird ausgelöst, wenn ein User den IRC Server verlässt
        /// </summary>
        public static event QuitEventHandler? OnQuit;

        /// <summary>
        /// Wird ausgelöst, wenn ein User seinen Nickname ändert
        /// </summary>
        public static event NickChangeEventHandler? OnNickChange;

        /// <summary>
        /// Wird ausgelöst, wenn ein User gekickt wird
        /// </summary>
        public static event KickEventHandler? OnKick;

        /// <summary>
        /// Wird ausgelöst, bevor versucht wird, die Nachricht mit einem Command zu behandeln
        /// </summary>
        public static event IrcMessageEventHandler? OnPreProcessingMessage;

        /// <summary>
        /// Wird ausgelöst, nachdem die Nachricht die Verarbeitung durchlaufen hat aber noch vor dem Logging
        /// </summary>
        public static event IrcMessageEventHandler? OnPostProcessingMessage;

        public delegate void IrcMessageEventHandler(object sender, IrcMessage theMessage);

        public Server Settings { get; set; }

        /// <summary>
        /// Gibt an ob eine Verbindung mit dem Server hergestellt ist
        /// </summary>
        public bool Connected { get; protected set; }

        private IrcFeatures? _connection;

        private Thread? _listener;

        /// <summary>
        /// Erstellt ein neues ServerConnetion Objekt welches die Verbindung zu einem IRC Server kapselt
        /// </summary>
        public ServerConnection(Server srv)
        {
            Settings = srv;
            Connected = false;
        }

        /// <summary>
        /// Betritt den gewünschten Channel und speichert ihn in der Datenbank
        /// </summary>
        /// <param name="channel">Der #channel</param>
        public void JoinChannel(string channel)
        {
            if (Settings.Channels.Any(x => x.Name == channel))
            {
                return;
            }

            IrcClient.RfcJoin(channel);

            using (var context = new BotContext())
            {
                context.Servers.Attach(Settings);
                Settings.Channels.Add(new ServerChannel { Name = channel });
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Verlässt den Channel und löscht ihn aus der Datenbank
        /// </summary>
        /// <param name="channel">Der #Channel</param>
        public void PartChannel(string channel)
        {
            ServerChannel chan = Settings.Channels.FirstOrDefault(x => x.Name == channel);
            if (chan == null)
            {
                return;
            }

            IrcClient.RfcPart(channel);

            using (var context = new BotContext())
            {
                context.Servers.Attach(Settings);
                Settings.Channels.Remove(chan);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Baut eine Verbindung mit den angegebenen Daten auf
        /// </summary>
        public void Connect()
        {
            _connection = new IrcFeatures();

            _connection.ActiveChannelSyncing = true;

            _connection.CtcpSource = "Frag Suchiman in freenode";
            _connection.CtcpUrl = _connection.CtcpSource;
            _connection.CtcpUserInfo = "Ich bin ein automatisch denkendes Wesen auch bekannt als Bot";
            _connection.CtcpVersion = "FritzBot:v3:" + Environment.OSVersion.Platform.ToString();
            _connection.Encoding = Encoding.GetEncoding("iso-8859-1");
            _connection.EnableUTF8Recode = true;

            _connection.OnChannelAction += _connection_OnMessage;
            _connection.OnChannelMessage += _connection_OnMessage;
            _connection.OnChannelNotice += _connection_OnMessage;

            _connection.OnQueryAction += _connection_OnMessage;
            _connection.OnQueryMessage += _connection_OnMessage;
            _connection.OnQueryNotice += _connection_OnMessage;

            _connection.OnNickChange += _connection_OnNickChange;
            _connection.OnJoin += _connection_OnJoin;
            _connection.OnKick += _connection_OnKick;
            _connection.OnPart += _connection_OnPart;
            _connection.OnQuit += _connection_OnQuit;

            _connection.OnConnectionError += _connection_OnConnectionError;

            _connection.Connect(Settings.Address, Settings.Port);
            _connection.Login(Settings.Nickname, Settings.Nickname, 0, Settings.Nickname);

            if (Settings.NickServPassword != null)
            {
                _connection.SendMessage(SendType.Message, "nickserv", "identify " + Settings.NickServPassword);
            }

            foreach (ServerChannel channel in Settings.Channels)
            {
                _connection.RfcJoin(channel.Name, Priority.Critical);
            }

            _listener = Toolbox.SafeThreadStart("ListenThread " + Settings.Address, true, _connection.Listen);

            Connected = true;
        }

        void _connection_OnConnectionError(object? sender, EventArgs e)
        {
            Connected = false;
            var timeConnectionLost = DateTime.Now;

            Log.Error("Verbindung zu Server {ServerAddress} verloren. Versuche Verbindung in 5 Sekunden wiederherzustellen", Settings.Address);
            Thread.Sleep(5000);

            //Wenn wir bis hierhin gekommen sind, wurde eine bestehende Verbindung aus einem externen Grund terminiert
            //Intern wurde bereits Disconnect aufgerufen, das heißt die Write, Read und Idle Threads wurden beendet.
            //Wir befinden uns in dieser Methode dann im _listener Thread, wenn wir diese Methode verlassen und _connection.IsConnected == true
            //wird die Listen(bool) Methode weiter loopen
            int connectionAttempt = 1;
            do
            {
                try
                {
                    Connect();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Verbindungsversuch zu {ServerAddress} gescheitert", Settings.Address);
                }

                if (!_connection!.IsConnected)
                {
                    ++connectionAttempt;
                    int delay = (connectionAttempt - 1) % 3 == 0 ? 30000 : 5000;
                    Log.Information("Nächster Verbindungsversuch ({ConnectionAttempt}) zu {ServerAddress}:{ServerPort} in {Delay} Sekunden", connectionAttempt, Settings.Address, Settings.Port, delay / 1000);
                    Thread.Sleep(delay);
                }
            }
            while (!_connection.IsConnected);
            Log.Information("Verbindung mit {ServerAddress} nach {TimeConnectionLost} Sekunden ohne Verbindung und {ConnectionAttempt} Verbindungsversuchen wiederhergestellt.", Settings.Address, DateTime.Now.Subtract(timeConnectionLost).TotalSeconds, connectionAttempt);
        }

        /// <summary>
        /// Stellt sicher, dass der User in der Datenbank für die Anforderung existiert und mit den richtigen Namen ausgestattet ist
        /// </summary>
        /// <param name="Name">Der Nickname des Users</param>
        private User MaintainUser(string Name)
        {
            using (var context = new BotContext())
            {
                Nickname nick = context.Nicknames.Include(x => x.User).FirstOrDefault(x => x.Name == Name);
                if (nick == null)
                {
                    nick = new Nickname { Name = Name, User = new User() };
                    context.Nicknames.Add(nick);
                    context.SaveChanges();
                    //LastUsedName kann nicht gesetzt werden während ein neuer Benutzer hinzugefügt wird
                }
                nick.User.LastUsedName = nick;
                context.SaveChanges();
                return nick.User;
            }
        }

        /// <summary>
        /// Verarbeitet das Betreten eines Users eines Channels
        /// </summary>
        /// <seealso cref="OnJoin"/>
        private void _connection_OnJoin(object sender, JoinEventArgs e)
        {
            Log.Information("{Nickname} hat den Raum {Channel} betreten", e.Who, e.Channel);
            MaintainUser(e.Who);

            ThreadPool.QueueUserWorkItem(x =>
            {
                OnJoin?.Invoke(this, e);
            });
        }

        /// <summary>
        /// Verarbeitet das Verlassen eines Users des Servers
        /// </summary>
        /// <seealso cref="OnQuit"/>
        private void _connection_OnQuit(object sender, QuitEventArgs e)
        {
            Log.Information("{Nickname} hat den Server verlassen ({QuitMessage})", e.Who, e.QuitMessage);
            MaintainUser(e.Who);

            ThreadPool.QueueUserWorkItem(x =>
            {
                OnQuit?.Invoke(this, e);
            });
        }

        /// <summary>
        /// Verarbeitet das Verlassen eines Users eines Channels
        /// </summary>
        /// <seealso cref="OnPart"/>
        private void _connection_OnPart(object sender, PartEventArgs e)
        {
            Log.Information("{Nickname} hat den Raum {Channel} verlassen", e.Who, e.Channel);
            MaintainUser(e.Who);

            ThreadPool.QueueUserWorkItem(x =>
            {
                OnPart?.Invoke(this, e);
            });
        }

        /// <summary>
        /// Verarbeitet einen Nickname wechsel
        /// </summary>
        /// <seealso cref="OnNickChange"/>
        private void _connection_OnNickChange(object sender, NickChangeEventArgs e)
        {
            Log.Information("{OldNickname} heißt jetzt {NewNickname}", e.OldNickname, e.NewNickname);
            using (var context = new BotContext())
            {
                User? oldNick = context.TryGetUser(e.OldNickname);
                User? newNick = context.TryGetUser(e.NewNickname);
                if (oldNick != null && newNick == null)
                {
                    oldNick.LastUsedName = new Nickname { Name = e.NewNickname, User = oldNick };
                    context.SaveChanges();
                }
                //else if (oldNick == null && newNick != null)
                //{
                //    newNick.Names.Add(e.OldNickname);
                //    newNick.LastUsedName = e.NewNickname;
                //    context.SaveChanges();
                //}
            }

            ThreadPool.QueueUserWorkItem(x =>
            {
                OnNickChange?.Invoke(this, e);
            });
        }

        /// <summary>
        /// Verarbeitet einen Kick
        /// </summary>
        /// <seealso cref="OnKick"/>
        private void _connection_OnKick(object sender, KickEventArgs e)
        {
            Log.Information("{Nickname} wurde von {Operator} aus dem Raum {Channel} geworfen", e.Who, e.Whom, e.Channel);
            MaintainUser(e.Who);

            ThreadPool.QueueUserWorkItem(x =>
            {
                OnKick?.Invoke(this, e);
            });
        }

        /// <summary>
        /// Verarbeitet eine IRC Nachricht
        /// </summary>
        /// <seealso cref="OnPreProcessingMessage"/>
        /// <seealso cref="OnPostProcessingMessage"/>
        private void _connection_OnMessage(object sender, IrcEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Data.Nick))
            {
                return;
            }

            DateTimeOffset received = DateTimeOffset.Now;

            ThreadPool.QueueUserWorkItem(x =>
            {
                User user = MaintainUser(e.Data.Nick);
                IrcMessage message = new IrcMessage(e.Data, this);

                if (!user.Ignored && !message.IsIgnored)
                {
                    OnPreProcessingMessage?.Invoke(this, message);

                    if (message.IsCommand && !message.HandledByEvent)
                    {
                        try
                        {
                            Program.HandleCommand(message);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "HandleCommand fehlgeschlagen");
                        }
                    }

                    OnPostProcessingMessage?.Invoke(this, message);

                    if (message.IsPrivate && !(message.ProcessedByCommand || message.Answered))
                    {
                        if (message.IsCommand)
                        {
                            message.Answer("Dieser Befehl kommt mir nicht bekannt vor... Probiers doch mal mit !hilfe");
                        }
                        else
                        {
                            message.Answer("Hallo du da, ich bin nicht so menschlich wie ich aussehe. Probier doch mal !hilfe");
                        }
                    }

                    if (!message.Hidden)
                    {
                        if (message.IsPrivate)
                        {
                            Log.Write(SerilogHack.CreateLogEvent(received, LogEventLevel.Information, null, "Von {Sender:l}: {Message:l}", message.Source, message.Message));
                        }
                        else
                        {
                            Log.Write(SerilogHack.CreateLogEvent(received, LogEventLevel.Information, null, "{Channel:l} {Sender:l}: {Message:l}", message.Source, message.Nickname, message.Message));
                        }
                        foreach (var OneMessage in message.UnloggedMessages)
                        {
                            Log.Write(OneMessage);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Trennt die Verbindung zum Server
        /// </summary>
        public void Disconnect()
        {
            Connected = false;
            if (_connection != null)
            {
                if (_connection.IsConnected)
                {
                    try
                    {
                        _connection.RfcQuit(Settings.QuitMessage);
                        _connection.Disconnect();
                    }
                    catch (NotConnectedException) { }
                    Log.Information("Verbindung zu Server {ServerAddress} getrennt", Settings.Address);
                }
                _connection = null;
            }
        }

        /// <summary>
        /// Gibt das der Verbindung zugrunde liegende IRC Objekt zurück
        /// </summary>
        public IrcFeatures IrcClient => _connection ?? throw new InvalidOperationException("Not connected");

        /// <summary>
        /// Gibt eine Nachricht in allen Channels bekannt
        /// </summary>
        /// <param name="message">Die Nachricht</param>
        public void Announce(string message)
        {
            foreach (ServerChannel channel in Settings.Channels)
            {
                IrcClient.SendMessage(SendType.Message, channel.Name, message);
                Log.Information("An {Receiver:l}: {Message:l}", channel.Name, message);
            }
        }
    }
}