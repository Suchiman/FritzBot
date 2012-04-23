using System;
using System.Threading;
using FritzBot;

namespace FritzBot.commands
{
    class leave : ICommand
    {
        public String[] Name { get { return new String[] { "leave" }; } }
        public String HelpText { get { return "Zum angegebenen Server werde ich die Verbindung trennen, Operator Befehl: !leave test.de"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        static private Mutex leave_safe = new Mutex();

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            leave_safe.WaitOne();
            String[] config_servers_array = toolbox.getDatabaseByName("servers.cfg").GetAll();
            for (int i = 0; i < config_servers_array.Length; i++)
            {
                if (config_servers_array[i].Split(',')[0] == message)
                {
                    toolbox.getDatabaseByName("servers.cfg").Remove(toolbox.getDatabaseByName("servers.cfg").GetAt(i));
                    break;
                }
            }
            for (int i = 0; i < Program.irc_connections.Count; i++)
            {
                if (Program.irc_connections[i].HostName == message)
                {
                    Program.irc_connections[i].Disconnect();
                    Program.irc_connections[i] = null;
                    Program.irc_connections.RemoveAt(i);
                    break;
                }
            }
            leave_safe.ReleaseMutex();
        }
    }
}