using System;

namespace freetzbot.commands
{
    class ignore : command
    {
        private String[] name = { "ignore" };
        private String helptext = "Schließt die angegebene Person von mir aus";
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
            if (sender == message || toolbox.op_check(sender))
            {
                toolbox.getDatabaseByName("ignore.db").Add(message);
                connection.sendmsg("Ich werde " + message + " ab sofort keine beachtung mehr schenken", receiver);
            }
        }
    }
}