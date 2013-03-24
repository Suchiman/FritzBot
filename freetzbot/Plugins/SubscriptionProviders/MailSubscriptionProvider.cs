using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.Plugins.SubscriptionProviders
{
    [Module.Name("Mail")]
    [Module.Help("Stellt dir die benachrichtigungen via E-Mail zu. !subscribe setup Mail example@web.de")]
    [Module.Hidden]
    public class MailSubscriptionProvider : SubscriptionProvider
    {
        public override void SendNotification(User user, string message)
        {
            SimpleStorage settings = GetSettings(new DBProvider(), user);
            string receiver = settings.Get<string>(PluginID, null);
            if (receiver == null)
            {
                return;
            }

            SimpleStorage storage = GetPluginStorage(new DBProvider());
            SmtpClient client = new SmtpClient(storage.Get("SMTPServer", "localhost"), storage.Get("SMTPPort", 25));
            client.EnableSsl = storage.Get("SMTPSSL", false);
            client.Credentials = new NetworkCredential(storage.Get("SMTPAccount", ""), storage.Get("SMTPPasswort", ""));
            MailAddress from = new MailAddress(storage.Get("SMTPFrom", ""), "FritzBot");


            MailAddress to = new MailAddress(receiver);
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = "FritzBot Notification";
            mailMessage.Body = message;

            client.Send(mailMessage);
        }

        public override void AddSubscription(ircMessage theMessage, PluginBase plugin)
        {
            if (GetSettings(new DBProvider(), theMessage.TheUser).Get<string>(PluginID, null) == null)
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
