using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("leave")]
    [Help("Zum angegebenen ServerConnetion werde ich die Verbindung trennen, Operator Befehl: !leave test.de")]
    [ParameterRequired]
    [Authorize]
    class leave : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                ServerManager.Remove(ServerManager.Servers.FirstOrDefault(x => x.Settings.Address == theMessage.CommandLine));
                theMessage.Answer(String.Format("Server {0} verlassen", theMessage.CommandLine));
            }
            catch
            {
                theMessage.Answer("Den Server kenne ich nicht");
            }
        }
    }
}