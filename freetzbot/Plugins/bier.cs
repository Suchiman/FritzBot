using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("bier")]
    class bier : PluginBase, IBackgroundTask
    {
        public void Start()
        {
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Server_OnPostProcessingMessage(object sender, ircMessage theMessage)
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