using FritzBot.Core;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
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
        public override void SendNotification(User user, string message)
        {
            Server UserConnection = ServerManager.GetInstance().FirstOrDefault(x => x.IrcClient.GetChannels().Select(c => x.IrcClient.GetChannel(c)).Any(c => c.Users.Keys.OfType<string>().Any(cn => user.Names.Contains(cn))));
            if (UserConnection != null)
            {
                SimpleStorage storage = GetSettings(new DBProvider(), user);
                if (storage.Get(PluginID, "PRIVMSG") == "PRIVMSG")
                {
                    UserConnection.IrcClient.SendMessage(SendType.Message, user.LastUsedName, message);
                }
                else
                {
                    UserConnection.IrcClient.SendMessage(SendType.Notice, user.LastUsedName, message);
                }
            }
        }

        public override void ParseSubscriptionSetup(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Zu wenig Parameter, probier mal: !subscribe setup <SubscriptionProvider> <Einstellung>");
                return;
            }
            if (theMessage.CommandArgs[2] == "NOTICE" || theMessage.CommandArgs[2] == "PRIVMSG")
            {
                base.ParseSubscriptionSetup(theMessage);
            }
            else
            {
                theMessage.Answer("Ungültige Option, mögliche Optionen sind NOTICE oder PRIVMSG");
            }
        }
    }
}
