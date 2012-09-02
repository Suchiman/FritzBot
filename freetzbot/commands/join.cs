using System;

namespace FritzBot.commands
{
    [Module.Name("join")]
    [Module.Help("Daraufhin werde ich den angegebenen Channel betreten, Operator Befehl: z.b. !join #testchannel")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class join : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.AnswerAction("rennt los zum channel " + theMessage.CommandLine);
            theMessage.Connection.JoinChannel(theMessage.CommandLine);
        }
    }
}