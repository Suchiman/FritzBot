using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("witz", "joke")]
    [Help("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen")]
    class witz : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                if (theMessage.HasArgs && theMessage.CommandArgs[0].Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    WitzEntry w = new WitzEntry { Witz = theMessage.CommandArgs.Skip(1).Join(" "), Creator = context.GetUser(theMessage.Nickname) };
                    context.WitzEntries.Add(w);
                    context.SaveChanges();
                    theMessage.Answer("Ist notiert " + theMessage.Nickname);
                    return;
                }

                Random rand = new Random();
                IQueryable<WitzEntry> ws = context.WitzEntries;
                if (theMessage.HasArgs)
                {
                    ws = ws.Where(x => theMessage.CommandArgs.All(f => x.Witz.ToLower().Contains(f.ToLower())));
                }
                WitzEntry entry = ws.OrderBy(x => x.Frequency).Skip(rand.Next(0, 10)).FirstOrDefault();

                if (entry == null)
                {
                    entry = ws.OrderBy(x => x.Frequency).FirstOrDefault();
                }

                if (entry != null)
                {
                    entry.Frequency++;
                    context.SaveChanges();
                    theMessage.Answer(entry.Witz);
                    return;
                }

                theMessage.Answer(theMessage.HasArgs ? "Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet" : "Mir fällt gerade kein Fritz!Witz ein");
            }
        }
    }
}