using System;

namespace freetzbot.commands
{
    class lmgtfy : command
    {
        private String[] name = { "lmgtfy" };
        private String helptext = "Die Funktion benötigt einen Parameter!";
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

        public void run(irc connection, String sender, String receiver, String message)
        {
            if (message.Contains("\""))
            {
                String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                split[1] = split[1].Replace(' ', '+');
                String[] nick = split[2].Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (nick.Length > 1)
                {
                    connection.sendmsg("@" + split[2] + ": Siehe: http://lmgtfy.com/?q=" + split[1], receiver);
                }
                else
                {
                    connection.sendmsg("http://lmgtfy.com/?q=" + split[1], receiver);
                }
            }
            else
            {
                String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (split.Length > 1)
                {
                    connection.sendmsg("@" + split[1] + ": Siehe: http://lmgtfy.com/?q=" + split[0], receiver);
                }
                else
                {
                    connection.sendmsg("http://lmgtfy.com/?q=" + split[0], receiver);
                }
            }
        }
    }
}