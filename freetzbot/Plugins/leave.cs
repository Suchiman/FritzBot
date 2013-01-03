using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("leave")]
    [Module.Help("Zum angegebenen Server werde ich die Verbindung trennen, Operator Befehl: !leave test.de")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class leave : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            ServerManager.GetInstance().Remove(ServerManager.GetInstance()[theMessage.CommandLine]);
            theMessage.Answer(String.Format("Server {0} verlassen", theMessage.CommandLine));
        }
    }
}