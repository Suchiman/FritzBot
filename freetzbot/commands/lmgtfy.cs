using System;
using System.Text;

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
            connection.sendmsg("http://lmgtfy.com/?q=" + System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(message)), receiver);
        }
    }
}