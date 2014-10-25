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
                UserKeyValueEntry entry = context.GetStorage(user, "pushover_token");
                if (entry != null)
                {
                    userToken = entry.Value;
                }
            }
            if (String.IsNullOrEmpty(userToken))
            {
                return;
            }
            Dictionary<String, String> Parameter = new Dictionary<String, String>()
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
                if (entry == null || String.IsNullOrEmpty(entry.Value))
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
