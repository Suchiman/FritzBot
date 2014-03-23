using FritzBot.Core;
using System;
using System.Linq;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Module.Name("RSS")]
    [Module.Help("Stellt die Informationen über einen RSS Feed bereit: " + webinterface.Address + "rss")]
    [Module.Hidden]
    public class RSSSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message) { } //Anzeige erfolgt über Webinterface, Nothing ToDo here

        public override void ParseSubscriptionSetup(ircMessage theMessage)
        {
            theMessage.Answer(String.Format("Der RSS Subscription Provider benötigt keine Konfiguration. Deinen Personalisierten RSS Feed bekommst du unter {0}rss?user={1} und den allgemein unter {0}rss", webinterface.Address, theMessage.TheUser.Names.First()));
        }
    }
}