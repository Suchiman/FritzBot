using FritzBot.Core;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("quit")]
    [Help("Das beendet mich X_x")]
    [ParameterRequired(false)]
    [Authorize]
    class quit : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            ServerManager.GetInstance().DisconnectAll();
        }
    }
}