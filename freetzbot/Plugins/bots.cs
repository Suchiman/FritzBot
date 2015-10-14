using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("bots")]
    [Help("Findet User die länger als <Tage> inaktiv waren. Benutzung: !bots <Tage> oder !bots <Tage> <Channel>")]
    [ParameterRequired(true)]
    class bots : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            int tage;
            string channel;

            if (theMessage.CommandArgs.Count < 2 | !Int32.TryParse(theMessage.CommandArgs.FirstOrDefault(), out tage))
            {
                if (theMessage.IsPrivate)
                {
                    theMessage.Answer("Da du mir Privat schreibst, kann ich den Channel nicht erraten. Verwende: !bots <Tage> <Channel>");
                    return;
                }
                channel = theMessage.Source;
            }
            else if (theMessage.CommandArgs.Count == 2)
            {
                channel = theMessage.CommandArgs[1];
            }
            else
            {
                theMessage.AnswerHelp(this);
                return;
            }

            if (!theMessage.Data.Irc.JoinedChannels.Contains(channel))
            {
                theMessage.Answer("In diesem Channel befinde ich mich nicht und kann daher keine Auskunft geben.");
                return;
            }

            using (var context = new BotContext())
            {
                var UserImChannel = theMessage.Data.Irc.GetChannel(channel).Users.Values.OfType<ChannelUser>().Select(x =>
                {
                    User u = context.GetUser(x.Nick);
                    SeenEntry entry = context.SeenEntries.FirstOrDefault(s => s.User.Id == u.Id);
                    return new { Nickname = x.Nick, User = u, SeenEntry = entry };
                }).ToList();

                string wahrscheinlichInaktiv = UserImChannel.Where(x => x.User == null || x.SeenEntry == null).Select(x => x.Nickname).Join(", ");
                string bestimmtInaktiv = UserImChannel.Where(x => x.SeenEntry != null && x.SeenEntry.LastMessaged < DateTime.Now.AddDays(-tage)).Select(x => x.Nickname).Join(", ");

                if (!String.IsNullOrWhiteSpace(wahrscheinlichInaktiv))
                {
                    theMessage.Answer($"User die wahrscheinlich inaktiv sind: {wahrscheinlichInaktiv}");
                }

                if (!String.IsNullOrWhiteSpace(bestimmtInaktiv))
                {
                    theMessage.Answer($"User die definitiv länger als {tage} Tage inaktiv sind: {bestimmtInaktiv}");
                }

                if (String.IsNullOrWhiteSpace(wahrscheinlichInaktiv) && String.IsNullOrWhiteSpace(bestimmtInaktiv))
                {
                    theMessage.Answer("Keinen Benutzer gefunden der in diese Kriterien fällt");
                }
            }
        }
    }
}