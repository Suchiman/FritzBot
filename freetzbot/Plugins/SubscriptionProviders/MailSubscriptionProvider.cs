using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Net.Mail;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Name("Mail")]
    [Help("Stellt dir die benachrichtigungen via E-Mail zu. !subscribe setup Mail example@web.de")]
    [Hidden]
    public class MailSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message)
        {
            string receiver = null;
            using (var context = new BotContext())
            {
                UserKeyValueEntry entry = context.GetStorage(user, PluginID);
                if (entry != null)
                {
                    receiver = entry.Value;
                }
            }
            if (receiver == null)
            {
                return;
            }

            SmtpClient client = new SmtpClient();
            MailAddress from = new MailAddress(ConfigHelper.GetString("SMTPFrom", ""), "FritzBot");

            MailAddress to = new MailAddress(receiver);
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = "FritzBot Notification";
            mailMessage.Body = message;

            client.Send(mailMessage);
        }

        public override void AddSubscription(ircMessage theMessage, PluginBase plugin)
        {
            UserKeyValueEntry entry;
            using (var context = new BotContext())
            {
                entry = context.GetStorage(theMessage.Nickname, PluginID);
            }
            if (entry == null || String.IsNullOrWhiteSpace(entry.Value))
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
