using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("part")]
    [Help("Den angegebenen Channel werde ich verlassen, Operator Befehl: z.b. !part #testchannel")]
    [ParameterRequired]
    [Authorize]
    class part : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            theMessage.AnswerAction("verlässt den channel " + theMessage.CommandLine);
            theMessage.ServerConnetion.PartChannel(theMessage.CommandLine);
        }
    }
}