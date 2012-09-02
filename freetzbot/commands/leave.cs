using System;

namespace FritzBot.commands
{
    [Module.Name("leave")]
    [Module.Help("Zum angegebenen Server werde ich die Verbindung trennen, Operator Befehl: !leave test.de")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class leave : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Program.TheServers[theMessage.CommandLine] = null;
        }
    }
}