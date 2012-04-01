using System;
using System.Text;

namespace freetzbot.commands
{
    class freetz : command
    {
        private String[] name = { "freetz", "f" };
        private String helptext = "Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz \"Build System\", !freetz FAQ Benutzer";
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

        public void run(irc connection, String sender, String receiver, String message)
        {
            String output = "http://freetz.org/search?q=";
            String nick = "";
            String uri = "";
            if (message == "")
            {
                output = "http://freetz.org/wiki";
            }
            else
            {
                if (message.Contains("\""))
                {
                    String[] split = message.Split(new String[] { "\"" }, 3, StringSplitOptions.None);
                    uri = split[1];
                    if (split[2] != "")
                    {
                        nick = split[2].Remove(0, 1);
                    }
                }
                else
                {
                    String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                    uri = split[0];
                    if (split.Length > 1)
                    {
                        nick = split[1];
                    }
                }
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(uri)) + "&wiki=on";
                if (nick != "")
                {
                    output = nick + ": Siehe: " + output;
                }
            }
            output = output.Replace("%23", "#");
            connection.sendmsg(output, receiver);
        }
    }
}