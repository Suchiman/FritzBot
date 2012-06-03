using System;

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

        public void Run(ircMessage theMessage)
        {
            theMessage.Connection.Sendaction("rennt los zum channel " + theMessage.CommandLine, theMessage.Source);
            theMessage.Connection.JoinChannel(theMessage.CommandLine);
        }
    }
}