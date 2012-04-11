using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.commands
{
    class op : command
    {
        private String[] name = { "op" };
        private String helptext = "Erteilt einem Benutzer Operator rechte";
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
            if (freetzbot.Program.TheUsers.Exists(message))
            {
                freetzbot.Program.TheUsers[message].is_op = true;
                connection.sendmsg("Okay", receiver);
            }
            else
            {
                connection.sendmsg("Den Benutzer kenne ich nicht", receiver);
            }
        }
    }
}
