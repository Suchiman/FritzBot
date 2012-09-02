using System;

namespace FritzBot.commands
{
    [Module.Name("quit")]
    [Module.Help("Das beendet mich X_x")]
    [Module.ParameterRequired(false)]
    [Module.Authorize]
    class quit : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Program.TheServers.DisconnectAll();
        }
    }
}