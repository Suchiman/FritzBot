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

                foreach (var connection in ServerManager.Servers)
                {
                    foreach (var channel in connection.IrcClient.GetChannels().Select(connection.IrcClient.GetChannel))
                    {
                        var userInChannel = channel.Users.Keys.Intersect(names).FirstOrDefault();
                        if (userInChannel != null)
                        {
                            UserKeyValueEntry entry = context.GetStorage(user, PluginID);

                            connection.IrcClient.SendMessage((entry?.Value ?? "PRIVMSG") == "PRIVMSG" ? SendType.Message : SendType.Notice, userInChannel, message);
                        }
                    }
                }
            }
        }

        public override void ParseSubscriptionSetup(IrcMessage theMessage)
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