using System;
using System.Collections.Generic;
using System.Text;

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

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (FritzBot.Program.TheUsers.Exists(message))
            {
                FritzBot.Program.TheUsers[message].is_op = true;
                connection.Sendmsg("Okay", receiver);
            }
            else
            {
                connection.Sendmsg("Den Benutzer kenne ich nicht", receiver);
            }
        }
    }
}
