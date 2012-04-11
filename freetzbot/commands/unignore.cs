using System;

namespace freetzbot.commands
{
    class unignore : command
    {
        private String[] name = { "unignore" };
        private String helptext = "Die betroffene Person wird von der ignore Liste gestrichen, Operator Befehl: z.b. !unignore Testnick";
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
            freetzbot.Program.TheUsers[message].ignored = false;
        }
    }
}