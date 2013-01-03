using FritzBot.Core;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("quit")]
    [Module.Help("Das beendet mich X_x")]
    [Module.ParameterRequired(false)]
    [Module.Authorize]
    class quit : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            ServerManager.GetInstance().DisconnectAll();
        }
    }
}