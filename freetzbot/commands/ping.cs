using System;

namespace FritzBot.commands
{
    [Module.Name("ping")]
    [Module.Help("Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin")]
    [Module.ParameterRequired(false)]
    class ping : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Pong " + theMessage.Nick);
        }
    }
}