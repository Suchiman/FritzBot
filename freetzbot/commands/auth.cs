using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.commands
{
    class auth : command
    {
        private String[] name = { "auth" };
        private String helptext = "Authentifiziert dich wenn du ein Passwort festgelegt hast. z.b. !auth passwort";
        private Boolean op_needed = false;
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
            if (sender != receiver)
            {
                connection.sendmsg("Ohje das solltest du besser im Query tuen", receiver);
                return;
            }
            if (freetzbot.Program.TheUsers[sender].CheckPassword(message))
            {
                freetzbot.Program.TheUsers[sender].authenticated = true;
                connection.sendmsg("Du bist jetzt authentifiziert", sender);
            }
            else
            {
                connection.sendmsg("Das Passwort war falsch", sender);
            }
        }
    }
}
