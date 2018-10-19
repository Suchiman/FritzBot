using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("whmf", "w")]
    [Help("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber")]
    class whmf : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            string output = "https://boxmatrix.info/wiki?search=";
            if (!theMessage.HasArgs)
            {
                output = "https://boxmatrix.info";
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