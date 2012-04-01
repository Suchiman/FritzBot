using System;

namespace freetzbot.commands
{
    class ping : command
    {
        private String[] name = { "ping" };
        private String helptext = "Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
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
            connection.sendmsg("Pong " + sender, receiver);
        }
    }
}