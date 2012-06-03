using System;

namespace FritzBot.commands
{
    class ping : ICommand
    {
        public String[] Name { get { return new String[] { "ping" }; } }
        public String HelpText { get { return "Damit kannst du Testen ob ich noch Ansprechbar bin oder ob ich gestorben bin"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Pong " + theMessage.Nick);
        }
    }
}