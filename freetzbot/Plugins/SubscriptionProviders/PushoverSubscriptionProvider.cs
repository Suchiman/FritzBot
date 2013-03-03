using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Module.Name("Pushover")]
    [Module.Help("Stellt dir die benachrichtigungen via Pushover zu. !subscribe setup Pushover <userToken>")]
    [Module.Hidden]
    class PushoverSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message)
        {
            XElement settings = GetSettings(user);
            if (settings == null)
            {
                return;
            }
            string userToken = settings.Value;
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

        public override void AddSubscription(ircMessage theMessage, PluginBase plugin, XElement storage)
        {
            if (GetSettings(theMessage.TheUser) == null)
            {
                theMessage.Answer("Du musst diesen SubscriptionProvider zuerst Konfigurieren (!subscribe setup)");
            }
            else
            {
                base.AddSubscription(theMessage, plugin, storage);
            }
        }
    }
}
