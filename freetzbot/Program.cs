using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace FritzBot
{
    class Program
    {
        public static Boolean restart = false;

        public static Boolean await_response = false;
        public static String awaited_response = "";
        public static String awaited_nick = "";

        public static UserCollection TheUsers;

        public delegate void JoinEventHandler(Irc connection, String nick, String Room);
        public delegate void PartEventHandler(Irc connection, String nick, String Room);
        public delegate void QuitEventHandler(Irc connection, String nick);
        public delegate void NickEventHandler(Irc connection, String Oldnick, String Newnick);
        public delegate void KickEventHandler(Irc connection, String nick, String Room);
        public delegate void MessageEventHandler(Irc connection, String sender, String receiver, String message);
        public static event JoinEventHandler UserJoined;
        public static event PartEventHandler UserPart;
        public static event QuitEventHandler UserQuit;
        public static event NickEventHandler UserNickChanged;
        public static event KickEventHandler BotKicked;
        public static event MessageEventHandler UserMessaged;

        public static List<db> databases = new List<db>();
        public static settings configuration = new settings("config.cfg");
        public static List<ICommand> commands = new List<ICommand>();
        public static List<Irc> irc_connections = new List<Irc>();

        private static Thread antifloodingthread;
        private static int antifloodingcount;
        private static Boolean floodingnotificated;

        private static void process_command(Irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
            Boolean answered = false;

            #region Antiflooding checks
            if (!toolbox.IsOp(sender))
            {
                int floodingcount;
                if (!int.TryParse(configuration["floodingcount"], out floodingcount))
                {
                    floodingcount = 5;//Default wert
                }
                if (antifloodingcount >= floodingcount)
                {
                    if (floodingnotificated == false)
                    {
                        floodingnotificated = true;
                        connection.Sendmsg("Flooding Protection aktiviert", receiver);
                    }
                    return;
                }
                else
                {
                    antifloodingcount++;
                }
                if (configuration["klappe"] == "true") receiver = sender;
            }
            #endregion

            try
            {
                ICommand theCommand = toolbox.getCommandByName(parameter[0].ToLower());
                if (!(theCommand.OpNeeded && !toolbox.IsOp(sender)))
                {
                    if ((theCommand.ParameterNeeded && !(parameter.Length > 1) || !theCommand.ParameterNeeded && parameter.Length > 1) && !theCommand.AcceptEveryParam)
                    {
                        connection.Sendmsg(theCommand.HelpText, receiver);
                    }
                    else if (parameter.Length > 1)
                    {
                        theCommand.Run(connection, sender, receiver, parameter[1]);
                    }
                    else
                    {
                        theCommand.Run(connection, sender, receiver, "");
                    }
                    answered = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "Command not found")
                {
                    toolbox.Logging("Eine Exception ist beim Ausführen eines Befehles abgefangen worden: " + ex.Message);
                }
            }

            if (!answered)
            {
                if (!FritzBot.commands.alias.AliasCommand(connection, sender, receiver, message, true) && !receiver.Contains("#") && receiver != connection.Nickname)
                {
                    connection.Sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", receiver);
                }
            }
        }

        public static void process_incomming(Irc connection, String source, String nick, String message)
        {
            while (message.ToCharArray()[0] == ' ')
            {
                message = message.Remove(0, 1);
            }
            while (message.ToCharArray()[message.Length - 1] == ' ')
            {
                message = message.Remove(message.Length - 1);
            }
            switch (source)
            {
                case "LOG":
                    toolbox.Logging(message);
                    return;
                case "JOIN":
                    toolbox.Logging(nick + " hat den Raum " + message + " betreten");
                    if (toolbox.IsIgnored(nick)) return;
                    if (nick != connection.Nickname)
                    {
                        UserJoined(connection, nick, message);
                    }
                    return;
                case "QUIT":
                    toolbox.Logging(nick + " hat den Server verlassen");
                    if (toolbox.IsIgnored(nick)) return;
                    UserQuit(connection, nick);
                    return;
                case "PART":
                    toolbox.Logging(nick + " hat den Raum " + message + " verlassen");
                    if (toolbox.IsIgnored(nick)) return;
                    UserPart(connection, nick, message);
                    return;
                case "NICK":
                    toolbox.Logging(nick + " heißt jetzt " + message);
                    if (toolbox.IsIgnored(nick)) return;
                    UserNickChanged(connection, nick, message);
                    return;
                case "KICK":
                    toolbox.Logging(nick + " hat mich aus dem Raum " + message + " geworfen");
                    BotKicked(connection, nick, message);
                    connection.Leave(message);
                    return;
                default:
                    break;
            }
            if (source.ToCharArray()[0] == '#')
            {
                toolbox.Logging(source + " " + nick + ": " + message);
            }
            else
            {
                toolbox.Logging("Von " + nick + ": " + message);
                source = nick;
            }
            if (!toolbox.IsIgnored(nick) && !nick.Contains(".") && nick != connection.Nickname)
            {
                if (await_response && (String.IsNullOrEmpty(awaited_nick) || awaited_nick == nick))
                {
                    awaited_response = message;
                    return;
                }
                else if (message.ToCharArray()[0] == '!')
                {
                    process_command(connection, nick, source, message.Remove(0, 1));
                }
                else if (source == nick)
                {
                    connection.Sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", nick);
                }
                UserMessaged(connection, nick, source, message);
            }
        }

        private static void antiflooding()
        {
            while (true)
            {
                int time;
                if (!int.TryParse(configuration["floodingcount_reduction"], out time))
                {
                    time = 5000;//Standard Wert wenn die Konvertierung fehlschlägt
                }
                Thread.Sleep(time);
                if (antifloodingcount > 0)
                {
                    antifloodingcount--;
                }
                if (antifloodingcount == 0)
                {
                    floodingnotificated = false;
                }
            }
        }

        private static void consoleread()
        {
            while (true)
            {
                String console_input = Console.ReadLine();
                String[] console_splitted = console_input.Split(new String[] { " " }, 2, StringSplitOptions.None);
                switch (console_splitted[0])
                {
                    case "exit":
                        Trennen();
                        break;
                    case "connect":
                        String[] parameter = console_splitted[1].Split(new String[] { "," }, 5, StringSplitOptions.None);
                        toolbox.InstantiateConnection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                        toolbox.getDatabaseByName("servers.cfg").Add(console_splitted[1]);
                        break;
                    case "leave":
                        toolbox.getCommandByName("leave").Run(irc_connections[0], "console", "console", console_splitted[1]);
                        break;
                }
            }
        }

        public static void Trennen()
        {
            foreach (Irc connections in irc_connections)
            {
                String raumliste = null;
                foreach (String raum in connections.rooms)
                {
                    if (raumliste == null)
                    {
                        raumliste = raum;
                    }
                    else
                    {
                        raumliste += ":" + raum;
                    }
                }
                int position = toolbox.getDatabaseByName("servers.cfg").Find(toolbox.getDatabaseByName("servers.cfg").GetContaining(connections.HostName)[0]);
                String[] substr = toolbox.getDatabaseByName("servers.cfg").GetAt(position).Split(new String[] { "," }, 5, StringSplitOptions.None);
                substr[4] = raumliste;
                toolbox.getDatabaseByName("servers.cfg").Remove(toolbox.getDatabaseByName("servers.cfg").GetAt(position));
                toolbox.getDatabaseByName("servers.cfg").Add(substr[0] + "," + substr[1] + "," + substr[2] + "," + substr[3] + "," + substr[4]);
                connections.Disconnect();
            }
        }

        private static void init()
        {
            UserJoined = delegate { };
            UserMessaged = delegate { };
            UserNickChanged = delegate { };
            UserPart = delegate { };
            UserQuit = delegate { };
            BotKicked = delegate { };
            TheUsers = new UserCollection();
            String[] config = toolbox.getDatabaseByName("servers.cfg").GetAll();
            try
            {
                foreach (String connection_server in config)
                {
                    if (connection_server.Length > 0)
                    {
                        String[] parameter = connection_server.Split(new String[] { "," }, 5, StringSplitOptions.None);
                        toolbox.InstantiateConnection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                    }
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging("Exception in der Initialesierung der Server: " + ex.Message);
            }

            Thread consolenthread = new Thread(new ThreadStart(consoleread));
            consolenthread.Name = "ConsolenThread";
            consolenthread.IsBackground = true;
            consolenthread.Start();
            antifloodingcount = 0;
            antifloodingthread = new Thread(new ThreadStart(antiflooding));
            antifloodingthread.Name = "AntifloodingThread";
            antifloodingthread.IsBackground = true;
            antifloodingthread.Start();

            // Dynamisches hinzufügen der Funktionen
            foreach(Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.Namespace == "FritzBot.commands")
                {
                    commands.Add((ICommand)Activator.CreateInstance(t));
                }
            }
        }

        private static void Main()
        {
            init();
            while (toolbox.IsRunning())
            {
                Thread.Sleep(2000);
            }
            if (restart == true)
            {
                try
                {
                    System.Diagnostics.Process.Start("/bin/sh", "/home/suchi/ircbot/start");
                }
                catch (Exception ex)
                {
                    toolbox.Logging("Exception beim restart aufgetreten: " + ex.Message);
                }
            }
        }
    }
}