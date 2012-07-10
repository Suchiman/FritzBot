using System;

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

        public void Run(ircMessage theMessage)
        {
            theMessage.AnswerAction("verlässt den channel " + theMessage.CommandLine);
            theMessage.Connection.Leave(theMessage.CommandLine);
        }
    }
}