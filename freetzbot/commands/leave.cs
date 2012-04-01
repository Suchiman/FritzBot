using System;
using System.Threading;

namespace freetzbot.commands
{
    class leave : command
    {
        private String[] name = { "leave" };
        private String helptext = "Zum angegebenen Server werde ich die Verbindung trennen, Operator Befehl: !leave test.de";
        private Boolean op_needed = true;
        private Boolean parameter_needed = true;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        static private Mutex leave_safe = new Mutex();

        public void run(irc connection, String sender, String receiver, String message)
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
            for (int i = 0; i < freetzbot.Program.irc_connections.Count; i++)
            {
                if (freetzbot.Program.irc_connections[i].hostname == message)
                {
                    freetzbot.Program.irc_connections[i].disconnect();
                    freetzbot.Program.irc_connections[i] = null;
                    freetzbot.Program.irc_connections.RemoveAt(i);
                    break;
                }
            }
            leave_safe.ReleaseMutex();
        }
    }
}