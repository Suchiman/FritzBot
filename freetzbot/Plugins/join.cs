using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("join")]
    [Module.Help("Daraufhin werde ich den angegebenen Channel betreten, Operator Befehl: z.b. !join #testchannel")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class join : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.AnswerAction("rennt los zum channel " + theMessage.CommandLine);
            theMessage.Server.JoinChannel(theMessage.CommandLine);
        }
    }
}