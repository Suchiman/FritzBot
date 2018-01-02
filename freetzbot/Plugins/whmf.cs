using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("whmf", "w")]
    [Help("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber")]
    class whmf : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            string output = "http://wehavemorefun.de/fbwiki/index.php?search=";
            if (!theMessage.HasArgs)
            {
                output = "http://wehavemorefun.de/fbwiki";
            }
            else
            {
                output += Toolbox.UrlEncode(theMessage.CommandLine);
            }
            output = output.Replace("%23", "#");
            theMessage.Answer(output);
        }
    }
}