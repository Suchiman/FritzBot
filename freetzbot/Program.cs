using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace FritzBot
{
    class Program
    {
        #region Events
        public delegate void JoinEventHandler(Irc connection, String nick, String Room);
        public delegate void PartEventHandler(Irc connection, String nick, String Room);
        public delegate void QuitEventHandler(Irc connection, String nick);
        public delegate void NickEventHandler(Irc connection, String Oldnick, String Newnick);
        public delegate void KickEventHandler(Irc connection, String nick, String Room);
        public delegate void MessageEventHandler(ircMessage theMessage);
        public static event JoinEventHandler UserJoined;
        public static event PartEventHandler UserPart;
        public static event QuitEventHandler UserQuit;
        public static event NickEventHandler UserNickChanged;
        public static event KickEventHandler BotKicked;
        public static event MessageEventHandler UserMessaged; 
        #endregion

        public static Boolean restart;
        public static UserCollection TheUsers;
        public static ServerCollection TheServers;
        public static List<ICommand> Commands;

        private static Thread AntiFloodingThread;
        private static int AntiFloodingCount;
        private static Boolean FloodingNotificated;

        private static void HandleCommand(ircMessage theMessage)
        {
            #region Antiflooding checks
            if (!toolbox.IsOp(theMessage.Nick))
            {
                if (AntiFloodingCount >= Properties.Settings.Default.FloodingCount)
                {
                    if (FloodingNotificated == false)
                    {
                        FloodingNotificated = true;
                        theMessage.Answer("Flooding Protection aktiviert");
                    }
                    return;
                }
                else
                {
                    AntiFloodingCount++;
                }
            }
            #endregion

            try
            {
                ICommand theCommand = toolbox.getCommandByName(theMessage.CommandName);
                if (!(theCommand.OpNeeded && !toolbox.IsOp(theMessage.Nick)))
                {
                    if ((theCommand.ParameterNeeded && !theMessage.hasArgs || !theCommand.ParameterNeeded && theMessage.hasArgs) && !theCommand.AcceptEveryParam)
                    {
                        theMessage.Answer(theCommand.HelpText);
                    }
                    else
                    {
                        theCommand.Run(theMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Command not found")
                {
                    String theAlias = FritzBot.commands.alias.GetAlias(theMessage);
                    if (!String.IsNullOrEmpty(theAlias))
                    {
                        theMessage.Answer(theAlias);
                    }
                }
                else
                {
                    toolbox.Logging("Eine Exception ist beim Ausführen eines Befehles abgefangen worden: " + ex.Message);
                }
            }

            if (!theMessage.Answered)
            {
                theMessage.Answer("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe");
            }
        }

        public static void HandleIncomming(Irc connection, String source, String nick, String message)
        {
            message = message.Trim();
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
            if (source.ToCharArray()[0] != '#' || Properties.Settings.Default.Silence)
            {
                source = nick;
            }
            ircMessage theMessage = new ircMessage(nick, source, message, TheUsers, connection);
            if (!theMessage.isIgnored)
            {
                UserMessaged(theMessage);
                if (theMessage.isCommand && !theMessage.Handled)
                {
                    HandleCommand(theMessage);
                }
                if (theMessage.isPrivate && !theMessage.Answered)
                {
                    connection.Sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", nick);
                }
            }
            if (!theMessage.Hidden)
            {
                if (theMessage.isPrivate)
                {
                    toolbox.Logging("Von " + nick + ": " + message);
                }
                else
                {
                    toolbox.Logging(source + " " + nick + ": " + message);
                }
            }
        }

        private static void AntiFlooding()
        {
            while (true)
            {
                Thread.Sleep(Properties.Settings.Default.FloodingCountReduction);
                if (AntiFloodingCount > 0)
                {
                    AntiFloodingCount--;
                }
                if (AntiFloodingCount == 0)
                {
                    FloodingNotificated = false;
                }
            }
        }

        private static void HandleConsoleInput()
        {
            while (true)
            {
                String ConsoleInput = Console.ReadLine();
                String[] ConsoleSplitted = ConsoleInput.Split(new String[] { " " }, 2, StringSplitOptions.None);
                switch (ConsoleSplitted[0])
                {
                    case "exit":
                        TheServers.DisconnectAll();
                        break;
                    case "connect":
                        AskConnection();
                        break;
                    case "leave":
                        TheServers[ConsoleSplitted[1]] = null;
                        break;
                }
            }
        }

        private static void AskConnection()
        {
            Console.Write("Hostname: ");
            String Hostname = Console.ReadLine();
            int port = 0;
            do
            {
                Console.Write("Port: ");
            }
            while (!int.TryParse(Console.ReadLine(), out port));
            Console.Write("Nickname: ");
            String nickname = Console.ReadLine();
            Console.Write("QuitMessage: ");
            String QuitMessage = Console.ReadLine();
            Console.Write("InitialChannel: ");
            String channel = Console.ReadLine();
            toolbox.InstantiateConnection(Hostname, port, nickname, QuitMessage, channel);
        }

        private static void Init()
        {
            Properties.Settings.Default.Reload();
            restart = false;
            Commands = new List<ICommand>();
            TheUsers = new UserCollection();
            UserJoined = delegate { };
            UserMessaged = delegate { };
            UserNickChanged = delegate { };
            UserPart = delegate { };
            UserQuit = delegate { };
            BotKicked = delegate { };

            // Dynamisches hinzufügen der Funktionen
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.Namespace == "FritzBot.commands" && !Properties.Settings.Default.IgnoredModules.Contains(t.Name))
                {
                    Commands.Add((ICommand)Activator.CreateInstance(t));
                }
            }

            TheServers = new ServerCollection(HandleIncomming);
            if (TheServers.ConnectionCount == 0)
            {
                Console.WriteLine("Keine Verbindungen bekannt, starte Verbindungsassistent");
                AskConnection();
            }
            TheServers.ConnectAll();

            Thread ConsolenThread = new Thread(new ThreadStart(HandleConsoleInput));
            ConsolenThread.Name = "ConsolenThread";
            ConsolenThread.IsBackground = true;
            ConsolenThread.Start();
            AntiFloodingCount = 0;
            AntiFloodingThread = new Thread(new ThreadStart(AntiFlooding));
            AntiFloodingThread.Name = "AntifloodingThread";
            AntiFloodingThread.IsBackground = true;
            AntiFloodingThread.Start();
        }

        private static void Deinit()
        {
            Properties.Settings.Default.Save();
            while (Commands.Count > 0)
            {
                Commands[0].Destruct();
                Commands[0] = null;
                Commands.RemoveAt(0);
            }
            TheServers.Flush();
            TheUsers.Flush();
        }

        private static void Main()
        {
            Init();
            while (TheServers.Connected)
            {
                Thread.Sleep(2000);
            }
            Deinit();
            if (restart == true)
            {
                try
                {
                    System.Diagnostics.Process.Start("FritzBot.exe");
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