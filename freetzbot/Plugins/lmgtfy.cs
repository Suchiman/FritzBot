using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("lmgtfy")]
    [Help("Die Funktion benötigt einen Parameter!")]
    [ParameterRequired]
    class lmgtfy : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            theMessage.Answer("http://lmgtfy.com/?q=" + Toolbox.UrlEncode(theMessage.CommandLine));
        }
    }
}