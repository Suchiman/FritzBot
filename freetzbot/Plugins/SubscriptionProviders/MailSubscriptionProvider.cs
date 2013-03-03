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
            SmtpClient client = new SmtpClient(PluginStorage.GetVariable("SMTPServer"), Convert.ToInt32(PluginStorage.GetVariable("SMTPPort")));
            client.EnableSsl = Boolean.Parse(PluginStorage.GetVariable("SMTPSSL", "false"));
            client.Credentials = new NetworkCredential(PluginStorage.GetVariable("SMTPAccount"), PluginStorage.GetVariable("SMTPPasswort"));
            MailAddress from = new MailAddress(PluginStorage.GetVariable("SMTPFrom"), "FritzBot");

            MailAddress to = new MailAddress(GetSettings(user).Value);
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = "FritzBot Notification";
            mailMessage.Body = message;

            client.Send(mailMessage);
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
