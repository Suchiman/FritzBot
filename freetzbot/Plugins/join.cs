using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("join")]
    [Help("Daraufhin werde ich den angegebenen Channel betreten, Operator Befehl: z.b. !join #testchannel")]
    [ParameterRequired]
    [Authorize]
    class join : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.AnswerAction("rennt los zum channel " + theMessage.CommandLine);
            theMessage.ServerConnetion.JoinChannel(theMessage.CommandLine);
        }
    }
}