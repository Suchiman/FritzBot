using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.commands
{
    class user : command
    {
        private String[] name = { "user" };
        private String helptext = "Führt Operationen an meiner Benutzerdatenbank aus, Operator Befehl: !user remove, reload, flush";
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

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            try
            {
                if (message == "reload")
                {
                    freetzbot.Program.TheUsers.Reload();
                }
                if (message == "flush")
                {
                    freetzbot.Program.TheUsers.Flush();
                }
                if (message.Contains("remove"))
                {
                    freetzbot.Program.TheUsers.Remove(message.Split(' ')[1]);
                }
                connection.sendmsg("Okay", receiver);
            }
            catch (Exception ex)
            {
                toolbox.logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                connection.sendmsg("Wups, das hat eine Exception verursacht", receiver);
            }
        }
    }
}
