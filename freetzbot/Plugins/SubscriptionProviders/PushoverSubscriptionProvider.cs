using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Module.Name("Pushover")]
    [Module.Help("Stellt dir die benachrichtigungen via Pushover zu. !subscribe setup Pushover <userToken>")]
    [Module.Hidden]
    class PushoverSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message)
        {
            string userToken;
            using (DBProvider db = new DBProvider())
            {
                SimpleStorage storage = GetSettings(db, user);
                userToken = storage.Get<string>("token", null);
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

        public override void AddSubscription(ircMessage theMessage, PluginBase plugin)
        {
            using (DBProvider db = new DBProvider())
            {
                SimpleStorage storage = GetSettings(db, theMessage.TheUser);
                if (String.IsNullOrEmpty(storage.Get<string>("token", null)))
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
