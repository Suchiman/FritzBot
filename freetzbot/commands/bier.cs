using System;

namespace FritzBot.commands
{
    [Module.Name("bier")]
    class bier : IBackgroundTask
    {
        public void Start()
        {
            Program.UserMessaged -= new Program.MessageEventHandler(Run);
        }

        public void Stop()
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
