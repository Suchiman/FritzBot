using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("ping")]
    [Help("Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin")]
    [ParameterRequired(false)]
    class ping : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Pong " + theMessage.Nickname);
        }
    }
}