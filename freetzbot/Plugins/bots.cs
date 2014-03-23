using FritzBot.Core;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("bots")]
    [Module.Help("Findet User die länger als <Tage> inaktiv waren. Benutzung: !bots <Tage> oder !bots <Tage> <Channel>")]
    [Module.ParameterRequired(true)]
    class bots : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
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
                else
                {
                    channel = theMessage.Source;
                }
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

            using (DBProvider db = new DBProvider())
            {
                var UserImChannel = theMessage.Data.Irc.GetChannel(channel).Users.Values.OfType<ChannelUser>().Select(x =>
                {
                    User u = db.GetUser(x.Nick);
                    SeenEntry entry = u != null ? db.QueryLinkedData<SeenEntry, User>(u).FirstOrDefault() : null;
                    return new { Nickname = x.Nick, User = u, SeenEntry = entry };
                }).ToList();

                theMessage.Answer(String.Format("User die wahrscheinlich inaktiv sind: {0}", String.Join(", ", UserImChannel.Where(x => x.User == null || x.SeenEntry == null).Select(x => x.Nickname))));
                theMessage.Answer(String.Format("User die definitiv länger als {0} inaktiv sind: {1}", tage, String.Join(", ", UserImChannel.Where(x => x.SeenEntry != null && x.SeenEntry.LastMessaged < DateTime.Now.AddDays(-tage)).Select(x => x.Nickname))));
            }
        }
    }
}