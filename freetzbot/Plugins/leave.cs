using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
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
        public void Run(IrcMessage theMessage)
        {
            var server = ServerManager.Servers.FirstOrDefault(x => x.Settings.Address == theMessage.CommandLine);
            if (server == null)
            {
                theMessage.Answer("Den Server kenne ich nicht");
                return;
            }

            try
            {
                ServerManager.Remove(server);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Verlassen von Server {Server} fehlgeschlagen", theMessage.CommandLine);
            }

            theMessage.Answer($"Server {theMessage.CommandLine} verlassen");
        }
    }
}