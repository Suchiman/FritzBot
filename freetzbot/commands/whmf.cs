using System;
using System.Net;

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

        public void Run(ircMessage theMessage)
        {
            String output = "http://wehavemorefun.de/fbwiki/index.php?search=";
            if (!theMessage.HasArgs)
            {
                output = "http://wehavemorefun.de/fbwiki";
            }
            else
            {
                output += toolbox.UrlEncode(theMessage.CommandLine);
                if (Properties.Settings.Default.WhmfUrlResolve)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(output);
                    request.Timeout = 10000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    output = response.ResponseUri.ToString();
                }
            }
            output = output.Replace("%23", "#");
            theMessage.Answer(output);
        }
    }
}