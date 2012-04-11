using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace freetzbot
{
    class Program
    {
        public static Boolean restart = false;

        public static Boolean await_response = false;
        public static String awaited_response = "";
        public static String awaited_nick = "";

        public static UserCollection TheUsers;
        
        public static List<db> databases = new List<db>();
        public static settings configuration = new settings("config.cfg");
        public static List<command> commands = new List<command>();
        public static List<irc> irc_connections = new List<irc>();

        private static Thread antifloodingthread;
        private static int antifloodingcount;
        private static Boolean floodingnotificated;

        private static void process_command(irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
            Boolean answered = false;

            #region Antiflooding checks
            if (!toolbox.op_check(sender))
            {
                int floodingcount;
                if (!int.TryParse(configuration.get("floodingcount"), out floodingcount))
                {
                    floodingcount = 5;//Default wert
                }
                if (antifloodingcount >= floodingcount)
                {
                    if (floodingnotificated == false)
                    {
                        floodingnotificated = true;
                        connection.sendmsg("Flooding Protection aktiviert", receiver);
                    }
                    return;
                }
                else
                {
                    antifloodingcount++;
                }
                if (configuration.get("klappe") == "true") receiver = sender;
            }
            #endregion

            foreach (command thecommand in commands)
            {
                if (!(thecommand.get_op_needed() && !toolbox.op_check(sender)))
                {
                    foreach (String name in thecommand.get_name())
                    {
                        if (parameter[0].ToLower() == name)
                        {
                            if ((thecommand.get_parameter_needed() && !(parameter.Length > 1) || !thecommand.get_parameter_needed() && parameter.Length > 1) && !thecommand.get_accept_every_param())
                            {
                                connection.sendmsg(thecommand.get_helptext(), receiver);
                            }
                            else if (parameter.Length > 1)
                            {
                                thecommand.run(connection, sender, receiver, parameter[1]);
                            }
                            else
                            {
                                thecommand.run(connection, sender, receiver, "");
                            }
                            answered = true;
                        }
                    }
                }
            }
            if (!answered)
            {
                if (!freetzbot.commands.alias.alias_command(connection, sender, receiver, message, true) && !receiver.Contains("#") && receiver != connection.nickname)
                {
                    connection.sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", receiver);
                }
            }
        }

        public static void process_incomming(irc connection, String source, String nick, String message)
        {
            switch (source)
            {
                case "LOG":
                    toolbox.logging(message);
                    return;
                case "JOIN":
                    if (!(nick == connection.nickname))
                    {
                        toolbox.logging(nick + " hat den Raum " + message + " betreten");
                        if (toolbox.ignore_check(nick)) return;
                        freetzbot.commands.frag.boxfrage(connection, nick, nick, nick);
                        TheUsers[nick].last_seen = DateTime.MinValue;
                    }
                    return;
                case "QUIT":
                    toolbox.logging(nick + " hat den Server verlassen");
                    TheUsers[nick].SetSeen();
                    TheUsers[nick].authenticated = false;
                    return;
                case "PART":
                    toolbox.logging(nick + " hat den Raum " + message + " verlassen");
                    TheUsers[nick].SetSeen();
                    TheUsers[nick].authenticated = false;
                    return;
                case "NICK":
                    toolbox.logging(nick + " heißt jetzt " + message);
                    TheUsers[nick].AddName(message);
                    TheUsers[nick].authenticated = false;
                    return;
                case "KICK":
                    toolbox.logging(nick + " hat mich aus dem Raum " + message + " geworfen");
                    connection.leave(message);
                    return;
                default:
                    break;
            }
            if (message.Contains("#96*6*") && !toolbox.ignore_check(nick))
            {
                if (DateTime.Now.Hour > 5 && DateTime.Now.Hour < 16)
                {
                    connection.sendmsg("Kein Bier vor 4", source);
                }
                else
                {
                    connection.sendmsg("Bier holen", source);
                }
            }
            if (source.ToCharArray()[0] == '#')
            {
                toolbox.logging(source + " " + nick + ": " + message);
                if (toolbox.ignore_check(nick)) return;
                if (!nick.Contains(".") && nick != connection.nickname)
                {
                    TheUsers[nick].SetMessage(message);
                }
            }
            else
            {
                toolbox.logging("Von " + nick + ": " + message);
                if (toolbox.ignore_check(nick)) return;
                if (message.ToCharArray()[0] != '!' && !nick.Contains(".") && nick != connection.nickname && !await_response)
                {
                    connection.sendmsg("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe", nick);
                }
                if (!nick.Contains(".") && nick != connection.nickname)
                {
                    TheUsers[nick].SetMessage(message);
                }
                if (await_response && (awaited_nick == "" || awaited_nick == nick))
                {
                    await_response = false;
                    awaited_nick = "";
                    awaited_response = message;
                }
                source = nick;
            }
            if (message.ToCharArray()[0] == '!')
            {
                if (toolbox.ignore_check(nick)) return;
                process_command(connection, nick, source, message.Remove(0, 1));
            }
        }

        private static void antiflooding()
        {
            while (true)
            {
                int time;
                if (!int.TryParse(configuration.get("floodingcount_reduction"), out time))
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
                        toolbox.instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                        toolbox.getDatabaseByName("servers.cfg").Add(console_splitted[1]);
                        break;
                    case "leave":
                        toolbox.getCommandByName("leave").run(irc_connections[0], "console", "console", console_splitted[1]);
                        break;
                }
            }
        }

        public static void Trennen()
        {
            foreach (irc connections in irc_connections)
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
                int position = toolbox.getDatabaseByName("servers.cfg").Find(toolbox.getDatabaseByName("servers.cfg").GetContaining(connections.hostname)[0]);
                String[] substr = toolbox.getDatabaseByName("servers.cfg").GetAt(position).Split(new String[] { "," }, 5, StringSplitOptions.None);
                substr[4] = raumliste;
                toolbox.getDatabaseByName("servers.cfg").Remove(toolbox.getDatabaseByName("servers.cfg").GetAt(position));
                toolbox.getDatabaseByName("servers.cfg").Add(substr[0] + "," + substr[1] + "," + substr[2] + "," + substr[3] + "," + substr[4]);
                connections.disconnect();
            }
        }

        private static void init()
        {
            TheUsers = new UserCollection();
            String[] config = toolbox.getDatabaseByName("servers.cfg").GetAll();
            try
            {
                foreach (String connection_server in config)
                {
                    if (connection_server.Length > 0)
                    {
                        String[] parameter = connection_server.Split(new String[] { "," }, 5, StringSplitOptions.None);
                        toolbox.instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                    }
                }
            }
            catch (Exception ex)
            {
                toolbox.logging("Exception in der Initialesierung der Server: " + ex.Message);
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
            List<Type> typelist = new List<Type>();
            foreach(Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.Namespace == "freetzbot.commands")
                {
                    commands.Add((command)Activator.CreateInstance(t));
                }
            }
        }

        private static void Main(String[] args)
        {
            init();
            while (toolbox.running_check())
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
                    toolbox.logging("Exception beim restart aufgetreten: " + ex.Message);
                }
            }
        }
    }
}