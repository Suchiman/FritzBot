using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.commands
{
    class unloadmodule : command
    {
        private String[] name = { "unloadmodule" };
        private String helptext = "Deaktiviert einen meiner Befehle";
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
                for (int i = 0; i < freetzbot.Program.commands.Count; i++)
                {
                    if (freetzbot.Program.commands[i].get_name()[0] == message)
                    {
                        freetzbot.Program.commands[i] = null;
                        freetzbot.Program.commands.RemoveAt(i);
                        connection.sendmsg("Modul erfolgreich entladen", receiver);
                        return;
                    }
                }
                connection.sendmsg("Modul wurde nicht gefunden", receiver);
            }
            catch
            {
                connection.sendmsg("Das hat eine Exception ausgelöst", receiver);
            }
        }
    }
}
