using System;

namespace FritzBot.commands
{
    class op : ICommand
    {
        public String[] Name { get { return new String[] { "op" }; } }
        public String HelpText { get { return "Erteilt einem Benutzer Operator rechte"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.TheUsers.Exists(theMessage.CommandLine))
            {
                theMessage.TheUsers[theMessage.CommandLine].IsOp = true;
                theMessage.Answer("Okay");
            }
            else
            {
                theMessage.Answer("Den Benutzer kenne ich nicht");
            }
        }
    }
}
