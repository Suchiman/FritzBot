using System;

namespace FritzBot.commands
{
    [Module.Name("unignore")]
    [Module.Help("Die betroffene Person wird von der ignore Liste gestrichen, Operator Befehl: z.b. !unignore Testnick")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class unignore : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.TheUsers[theMessage.CommandArgs[0]].ignored = false;
        }
    }
}