using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace freetzbot.commands
{
    class loadmodule : command
    {
        private String[] name = { "modprobe", "loadmodule" };
        private String helptext = "Aktiviert einen meiner Befehle";
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

        public void run(irc connection, String sender, String receiver, String message)
        {
            try
            {
                Type t = Assembly.GetExecutingAssembly().GetType(message);
                if (t == null)
                {
                    connection.sendmsg("Modul wurde nicht gefunden", receiver);
                    return;
                }
                freetzbot.Program.commands.Add((command)Activator.CreateInstance(t));
                connection.sendmsg("Modul erfolgreich geladen", receiver);
            }
            catch
            {
                connection.sendmsg("Das hat eine Exception ausgelöst", receiver);
            }
        }
    }
}
