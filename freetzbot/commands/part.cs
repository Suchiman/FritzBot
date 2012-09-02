using System;

namespace FritzBot.commands
{
    [Module.Name("part")]
    [Module.Help("Den angegebenen Channel werde ich verlassen, Operator Befehl: z.b. !part #testchannel")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class part : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.AnswerAction("verlässt den channel " + theMessage.CommandLine);
            theMessage.Connection.Leave(theMessage.CommandLine);
        }
    }
}