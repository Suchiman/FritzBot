using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("lmgtfy")]
    [Module.Help("Die Funktion benötigt einen Parameter!")]
    [Module.ParameterRequired]
    class lmgtfy : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("http://lmgtfy.com/?q=" + toolbox.UrlEncode(theMessage.CommandLine));
        }
    }
}