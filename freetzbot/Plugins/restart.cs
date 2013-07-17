using FritzBot.Core;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("restart")]
    [Module.Help("Ich werde versuchen mich selbst neuzustarten, Operator Befehl: kein parameter")]
    [Module.ParameterRequired(false)]
    [Module.Authorize]
    class restart : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Program.Shutdown(true);
        }
    }
}