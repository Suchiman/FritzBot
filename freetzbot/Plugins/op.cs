using FritzBot.Database;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("op")]
    [Help("Erteilt einem Benutzer Operator rechte")]
    [ParameterRequired]
    [Authorize]
    class op : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                if (context.TryGetUser(theMessage.CommandLine) is { } u)
                {
                    u.Admin = true;
                    context.SaveChanges();
                    theMessage.Answer("Okay");
                    return;
                }
                theMessage.Answer("Den Benutzer kenne ich nicht");
            }
        }
    }
}