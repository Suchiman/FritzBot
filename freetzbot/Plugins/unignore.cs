using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("unignore")]
    [Help("Die betroffene Person wird von der ignore Liste gestrichen, Operator Befehl: z.b. !unignore Testnick")]
    [ParameterRequired]
    [Authorize]
    class unignore : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (var context = new BotContext())
            {
                User u = context.GetUser(theMessage.CommandArgs.FirstOrDefault());
                if (u != null)
                {
                    u.Ignored = false;
                    context.SaveChanges();
                    theMessage.Answer(String.Format("Ignoranz für {0} aufgehoben", u.LastUsedName));
                    return;
                }
                theMessage.Answer("Oh... Dieser User ist mir nicht bekannt");
            }
        }
    }
}