using System;

namespace freetzbot.commands
{
    class zeit : command
    {
        private String[] name = { "zeit" };
        private String helptext = "Das gibt die aktuelle Uhrzeit aus.";
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

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            try
            {
                connection.sendmsg("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss") + ".", receiver);
            }
            catch
            {
                connection.sendmsg("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(", receiver);
            }
        }
    }
}