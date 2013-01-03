using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("ping")]
    [Module.Help("Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin")]
    [Module.ParameterRequired(false)]
    class ping : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Pong " + theMessage.Nickname);
        }
    }
}