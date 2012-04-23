using System;
using FritzBot;

namespace FritzBot.commands
{
    class part : ICommand
    {
        public String[] Name { get { return new String[] { "part" }; } }
        public String HelpText { get { return "Den angegebenen Channel werde ich verlassen, Operator Befehl: z.b. !part #testchannel"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            connection.Sendaction("verlässt den channel " + message, receiver);
            connection.Leave(message);
        }
    }
}