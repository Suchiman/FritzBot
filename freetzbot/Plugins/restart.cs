using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("restart")]
    [Help("Ich werde versuchen mich selbst neuzustarten, Operator Befehl: kein parameter")]
    [ParameterRequired(false)]
    [Authorize]
    class restart : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            Program.Shutdown(true);
        }
    }
}