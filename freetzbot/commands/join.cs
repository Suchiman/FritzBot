using System;
using FritzBot;

namespace FritzBot.commands
{
    class join : ICommand
    {
        public String[] Name { get { return new String[] { "join" }; } }
        public String HelpText { get { return "Daraufhin werde ich den angegebenen Channel betreten, Operator Befehl: z.b. !join #testchannel"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            connection.Sendaction("rennt los zum channel " + message, receiver);
            connection.JoinChannel(message);
        }
    }
}