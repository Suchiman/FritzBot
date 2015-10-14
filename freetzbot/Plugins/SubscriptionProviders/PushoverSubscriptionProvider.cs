using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Name("Pushover")]
    [Help("Stellt dir die benachrichtigungen via Pushover zu. !subscribe setup Pushover <userToken>")]
    [Hidden]
    class PushoverSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message)
        {
            string userToken = null;
            using (var context = new BotContext())
            {
                userToken = context.GetStorage(user, "pushover_token")?.Value;
            }
            if (String.IsNullOrEmpty(userToken))
            {
                return;
            }
            Dictionary<string, string> Parameter = new Dictionary<string, string>()
            {
                {"token", "b6p6augH1KDpxcRxyo4I35Yxl9XP5x"},
                {"user", userToken},
                {"message", message}
            };
            toolbox.GetWeb("https://api.pushover.net/1/messages.xml", Parameter);
            //XDocument antwort = XDocument.Parse(toolbox.GetWeb("https://api.pushover.net/1/messages.xml", Parameter));
            //<hash>
            //  <status type="integer">1</status>
            //</hash>
        }

        public override void AddSubscription(IrcMessage theMessage, PluginBase plugin)
        {
            using (var context = new BotContext())
            {
                UserKeyValueEntry entry = context.GetStorage(theMessage.Nickname, "pushover_token");
                if (String.IsNullOrEmpty(entry?.Value))
                {
                    theMessage.Answer("Du musst diesen SubscriptionProvider zuerst Konfigurieren (!subscribe setup)");
                }
                else
                {
                    base.AddSubscription(theMessage, plugin);
                }
            }
        }
    }
}
