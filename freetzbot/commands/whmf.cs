using System;
using System.Net;

namespace FritzBot.commands
{
    [Module.Name("whmf", "w")]
    [Module.Help("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber")]
    class whmf : ICommand
    {
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