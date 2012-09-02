using System;

namespace FritzBot.commands
{
    [Module.Name("restart")]
    [Module.Help("Ich werde versuchen mich selbst neuzustarten, Operator Befehl: kein parameter")]
    [Module.ParameterRequired(false)]
    [Module.Authorize]
    class restart : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Program.restart = true;
            Program.TheServers.DisconnectAll();
        }
        
    }
}