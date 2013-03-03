using FritzBot.DataModel;
using System;
using System.Net;

namespace FritzBot.Plugins
{
    [Module.Name("whmf", "w")]
    [Module.Help("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber")]
    class whmf : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string output = "http://wehavemorefun.de/fbwiki/index.php?search=";
            if (!theMessage.HasArgs)
            {
                output = "http://wehavemorefun.de/fbwiki";
            }
            else
            {
                output += toolbox.UrlEncode(theMessage.CommandLine);
                if (PluginStorage.GetVariable("urlResolve", "false") == "true")
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