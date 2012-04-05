using System;
using System.Text;

namespace freetzbot.commands
{
    class google : command
    {
        private String[] name = { "google", "g" };
        private String helptext = "Syntax: (!g) !google etwas das du suchen möchtest";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
        private Boolean accept_every_param = true;

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
            String output = "https://www.google.de/search?q=";
            if (message == "")
            {
                output = "http://www.google.de/";
            }
            else
            {
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes("\"" + message + "\""));
            }
            connection.sendmsg(output, receiver);
        }
    }
}