using System;
using System.Net;
using System.Text;

namespace freetzbot.commands
{
    class whmf : command
    {
        private String[] name = { "whmf", "w" };
        private String helptext = "Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber";
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
            String output = "http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=";
            if (message == "")
            {
                output = "http://www.wehavemorefun.de/fritzbox/index.php";
            }
            else
            {
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(message));
                if (freetzbot.Program.configuration.get("whmf_url_resolve") == "true")
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(output);
                    request.Timeout = 10000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    output = response.ResponseUri.ToString();
                }
            }
            output = output.Replace("%23", "#");
            connection.sendmsg(output, receiver);
        }
    }
}