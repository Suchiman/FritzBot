using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("seen", "said")]
    [Help("Gibt aus wann der Nutzer zuletzt gesehen wurde und wann er was zuletzt sagte.")]
    [ParameterRequired]
    class seen : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            ServerConnection.OnJoin += Server_OnJoin;
            ServerConnection.OnQuit += Server_OnQuit;
            ServerConnection.OnPart += Server_OnQuit;
            ServerConnection.OnPreProcessingMessage += Server_OnPreProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnJoin -= Server_OnJoin;
            ServerConnection.OnQuit -= Server_OnQuit;
            ServerConnection.OnPart -= Server_OnQuit;
            ServerConnection.OnPreProcessingMessage -= Server_OnPreProcessingMessage;
        }

        private void Server_OnPreProcessingMessage(object sender, IrcMessage theMessage)
        {
            if (theMessage.IsIgnored)
            {
                return;
            }
            using (var context = new BotContext())
            {
                SeenEntry entry = GetSeenEntry(context, theMessage.Nickname);
                entry.LastMessaged = DateTime.Now;
                entry.LastMessage = theMessage.Message;
                context.SaveChanges();
            }
        }

        private void Server_OnQuit(object sender, IrcEventArgs e)
        {
            using (var context = new BotContext())
            {
                SeenEntry entry = GetSeenEntry(context, e.Data.Nick);
                entry.LastSeen = DateTime.Now;
                context.SaveChanges();
            }
        }

        private void Server_OnJoin(object sender, JoinEventArgs e)
        {
            using (var context = new BotContext())
            {
                SeenEntry entry = GetSeenEntry(context, e.Who);
                entry.LastSeen = null;
                context.SaveChanges();
            }
        }

        private SeenEntry GetSeenEntry(BotContext context, string nick)
        {
            User u = context.GetUser(nick);
            SeenEntry entry = context.SeenEntries.FirstOrDefault(x => x.User!.Id == u.Id);
            if (entry == null)
            {
                entry = new SeenEntry { User = u };
                context.SeenEntries.Add(entry);
            }
            return entry;
        }

        public void Run(IrcMessage theMessage)
        {
            if (String.Equals(theMessage.CommandLine, theMessage.ServerConnetion.IrcClient.Nickname, StringComparison.OrdinalIgnoreCase))
            {
                theMessage.Answer("Ich bin gerade hier und laut meinem Logik System solltest du auch sehen können was ich schreibe");
                return;
            }
            using (var context = new BotContext())
            {
                SeenEntry entry = context.SeenEntries.FirstOrDefault(x => x.User == context.Nicknames.FirstOrDefault(n => n.Name == theMessage.CommandLine).User);
                string output = "";
                if (entry != null)
                {
                    if (entry.LastSeen.HasValue)
                    {
                        output = "Den/Die habe ich hier zuletzt am " + entry.LastSeen.Value.ToString("dd.MM.yyyy ") + "um" + entry.LastSeen.Value.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                    }
                    if (entry.LastMessaged.HasValue)
                    {
                        output += " Am " + entry.LastMessaged.Value.ToString("dd.MM.yyyy ") + "um" + entry.LastMessaged.Value.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + entry.LastMessage + "\".";
                    }
                    else
                    {
                        output += " Den habe ich hier noch nie etwas schreiben sehen.";
                    }
                }
                theMessage.Answer(!String.IsNullOrEmpty(output) ? output.Trim() : "Scheinbar sind meine Datensätze unvollständig, tut mir leid");
            }
        }
    }
}