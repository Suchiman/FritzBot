using System;

namespace freetzbot.commands
{
    class join : command
    {
        private String[] name = { "join" };
        private String helptext = "Daraufhin werde ich den angegebenen Channel betreten, Operator Befehl: z.b. !join #testchannel";
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
            connection.sendaction("rennt los zum channel " + message, receiver);
            connection.join(message);
        }
    }
}