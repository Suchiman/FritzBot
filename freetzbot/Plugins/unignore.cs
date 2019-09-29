using FritzBot.Database;
using FritzBot.DataModel;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("unignore")]
    [Help("Die betroffene Person wird von der ignore Liste gestrichen, Operator Befehl: z.b. !unignore Testnick")]
    [ParameterRequired]
    [Authorize]
    class unignore : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                string nickname = theMessage.CommandArgs.FirstOrDefault();
                if (context.TryGetUser(nickname) is { } u)
                {
                    u.Ignored = false;
                    context.SaveChanges();
                    theMessage.Answer($"Ignoranz für {nickname} aufgehoben");
                    return;
                }
                theMessage.Answer("Oh... Dieser User ist mir nicht bekannt");
            }
        }
    }
}