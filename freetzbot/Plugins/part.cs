using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("part")]
    [Module.Help("Den angegebenen Channel werde ich verlassen, Operator Befehl: z.b. !part #testchannel")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class part : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.AnswerAction("verlässt den channel " + theMessage.CommandLine);
            theMessage.Server.PartChannel(theMessage.CommandLine);
        }
    }
}