using FritzBot.Core;
using FritzBot.Database;
using Meebey.SmartIrc4net;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Name("IRC")]
    [Help("Stellt dir die Nachrichten über IRC zu, wahlweise über NOTICE oder PRIVMSG. !subscribe setup IRC NOTICE / PRIVMSG")]
    [Hidden]
    public class IRCSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message)
        {
            using (var context = new BotContext())
            {
                List<string> names = context.Nicknames.Where(x => x.User.Id == user.Id).Select(x => x.Name).ToList();
                ServerConnetion UserConnection = ServerManager.GetInstance().FirstOrDefault(x => x.IrcClient.GetChannels().Select(c => x.IrcClient.GetChannel(c)).Any(c => c.Users.Keys.OfType<string>().Any(names.Contains)));
                if (UserConnection != null)
                {
                    string method = "PRIVMSG";
                    UserKeyValueEntry entry = context.GetStorage(user, PluginID);
                    if (entry != null)
                    {
                        method = entry.Value;
                    }
                    if (method == "PRIVMSG")
                    {
                        UserConnection.IrcClient.SendMessage(SendType.Message, user.LastUsedName.Name, message);
                    }
                    else
                    {
                        UserConnection.IrcClient.SendMessage(SendType.Notice, user.LastUsedName.Name, message);
                    }
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