using System;

namespace FritzBot.commands
{
    [Module.Name("op")]
    [Module.Help("Erteilt einem Benutzer Operator rechte")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class op : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.TheUsers.Exists(theMessage.CommandLine))
            {
                theMessage.TheUsers[theMessage.CommandLine].IsOp = true;
                theMessage.Answer("Okay");
            }
            else
            {
                theMessage.Answer("Den Benutzer kenne ich nicht");
            }
        }
    }
}
