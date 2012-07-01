using System;

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

        public void Run(ircMessage theMessage)
        {
            if (theMessage.Message.Contains("#96*6*") && !theMessage.IsIgnored)
            {
                if (DateTime.Now.Hour > 5 && DateTime.Now.Hour < 16)
                {
                    theMessage.Answer("Kein Bier vor 4");
                }
                else
                {
                    theMessage.Answer("Bier holen");
                }
            }
        }
    }
}
