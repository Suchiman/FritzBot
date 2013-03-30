using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("bier")]
    class bier : PluginBase, IBackgroundTask
    {
        public void Start()
        {
            Program.UserMessaged += Run;
        }

        public void Stop()
        {
            Program.UserMessaged -= Run;
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