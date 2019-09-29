using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("remind")]
    [Help("Hinterlasse einem Benutzer eine Nachricht. Sobald er wiederkommt oder etwas schreibt werde ich sie ihm Zustellen. !remind <Benutzer> <Nachricht>")]
    class remind : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            ServerConnection.OnJoin += Server_OnJoin;
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnJoin -= Server_OnJoin;
            ServerConnection.OnPostProcessingMessage -= Server_OnPostProcessingMessage;
        }

        private void Server_OnPostProcessingMessage(object sender, IrcMessage theMessage)
        {
            SendIt(theMessage.Nickname, theMessage.SendPrivateMessage);
        }

        private void Server_OnJoin(object sender, JoinEventArgs e)
        {
            SendIt(e.Who, x => e.Data.Irc.SendMessage(SendType.Message, e.Who, x));
        }

        private void SendIt(string nickname, Action<string> SendAction)
        {
            using (var context = new BotContext())
            {
                foreach (ReminderEntry item in context.ReminderEntries.Include(x => x.Creator.LastUsedName).Where(x => x.User.Names.Any(n => n.Name == nickname)).ToList())
                {
                    SendAction($"{item.Creator.LastUsedName} hat für dich am {item.Created:dd.MM.yyyy 'um' HH:mm:ss} eine Nachricht hinterlassen: {item.Message}");
                    context.ReminderEntries.Remove(item);
                }
                context.SaveChanges();
            }
        }

        public void Run(IrcMessage theMessage)
        {
            if (theMessage.CommandArgs.Count > 1)
            {
                using (var context = new BotContext())
                {
                    if (theMessage.ServerConnetion.IrcClient.IsMe(theMessage.CommandArgs[0]))
                    {
                        theMessage.Answer("Wieso sollte ich mich selbst an etwas erinnern ;) ?");
                        return;
                    }
                    if (context.TryGetUser(theMessage.CommandArgs[0]) is { } u)
                    {
                        ReminderEntry r = new ReminderEntry();
                        r.Created = DateTime.Now;
                        r.Creator = context.GetUser(theMessage.Nickname);
                        r.Message = theMessage.CommandArgs.Skip(1).Join(" ");
                        r.User = u;
                        context.ReminderEntries.Add(r);
                        context.SaveChanges();
                        theMessage.Answer("Okay ich werde es sobald wie möglich zustellen");
                    }
                    else
                    {
                        theMessage.Answer("Den Benutzer habe ich aber noch nie gesehen");
                    }
                }
            }
            else
            {
                theMessage.Answer("Die Eingabe war nicht korrekt: !remind <Benutzer> <Nachricht>");
            }
        }
    }
}