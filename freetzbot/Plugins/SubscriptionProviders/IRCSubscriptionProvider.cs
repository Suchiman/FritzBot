using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Module.Name("IRC")]
    [Module.Help("Stellt dir die Nachrichten über IRC zu, wahlweise über NOTICE oder PRIVMSG. !subscribe setup IRC NOTICE / PRIVMSG")]
    [Module.Hidden]
    public class IRCSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, String message)
        {
            Irc UserConnection = ServerManager.GetInstance().GetAllConnections().FirstOrDefault(x => x.Channels.Any(y => y.User.Contains(user)));
            if (UserConnection != null)
            {
                if (GetSettings(user) == null || GetSettings(user).Value == "PRIVMSG")
                {
                    UserConnection.Sendmsg(message, user.LastUsedNick);
                }
                else
                {
                    UserConnection.Sendnotice(message, user.LastUsedNick);
                }
            }
        }

        public override void ParseSubscriptionSetup(ircMessage theMessage, XElement storage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Zu wenig Parameter, probier mal: !subscribe setup <SubscriptionProvider> <Einstellung>");
                return;
            }
            if (theMessage.CommandArgs[2] == "NOTICE" || theMessage.CommandArgs[2] == "PRIVMSG")
            {
                base.ParseSubscriptionSetup(theMessage, storage);
            }
            else
            {
                theMessage.Answer("Ungültige Option, mögliche Optionen sind NOTICE oder PRIVMSG");
            }
        }
    }
}
