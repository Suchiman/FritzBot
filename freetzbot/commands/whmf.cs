using System;
using System.Net;
using System.Text;

namespace FritzBot.commands
{
    class whmf : ICommand
    {
        public String[] Name { get { return new String[] { "whmf", "w" }; } }
        public String HelpText { get { return "Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String output = "http://wehavemorefun.de/fritzbox/index.php/Special:Search?search=";
            if (String.IsNullOrEmpty(message))
            {
                output = "http://www.wehavemorefun.de/fritzbox/index.php";
            }
            else
            {
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(message));
                if (FritzBot.Program.configuration["whmf_url_resolve"] == "true")
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(output);
                    request.Timeout = 10000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    output = response.ResponseUri.ToString();
                }
            }
            output = output.Replace("%23", "#");
            connection.Sendmsg(output, receiver);
        }
    }
}