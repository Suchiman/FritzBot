using System;
using System.Collections.Generic;
using System.Text;

namespace FritzBot.commands
{
    class bier : ICommand
    {
        public String[] Name { get { return new String[] { "bier" }; } }
        public String HelpText { get { return "Bier!"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {
            Program.UserMessaged -= new Program.MessageEventHandler(Run);
        }

        public bier()
        {
            Program.UserMessaged += new Program.MessageEventHandler(Run);
        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (message.Contains("#96*6*") && !toolbox.IsIgnored(sender))
            {
                if (DateTime.Now.Hour > 5 && DateTime.Now.Hour < 16)
                {
                    connection.Sendmsg("Kein Bier vor 4", receiver);
                }
                else
                {
                    connection.Sendmsg("Bier holen", receiver);
                }
            }
        }
    }
}
