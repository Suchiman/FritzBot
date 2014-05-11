using FritzBot.Core;
using FritzBot.DataModel;
using System;

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
                ServerManager.GetInstance().Remove(ServerManager.GetInstance()[theMessage.CommandLine]);
                theMessage.Answer(String.Format("Server {0} verlassen", theMessage.CommandLine));
            }
            catch
            {
                theMessage.Answer("Den Server kenne ich nicht");
            }
        }
    }
}