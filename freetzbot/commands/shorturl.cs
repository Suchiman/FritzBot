using System;
using System.Net;

namespace freetzbot.commands
{
    class shorturl : command
    {
        private String[] name = { "shorturl", "urlshort", "urlshortener" };
        private String helptext = "Kürzt eine URL bei einem URL shortener";
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

        public shorturl()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            connection.sendmsg(toolbox.short_url(message), receiver);
        }
    }
}
