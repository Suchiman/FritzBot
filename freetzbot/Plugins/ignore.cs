using FritzBot.Database;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("ignore")]
    [Help("Schließt die angegebene Person von mir aus")]
    [ParameterRequired]
    class ignore : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                if (theMessage.Source == theMessage.CommandLine || Toolbox.IsOp(context.GetUser(theMessage.Nickname)))
                {
                    if (context.TryGetUser(theMessage.CommandLine) is { } u)
                    {
                        u.Ignored = true;
                        context.SaveChanges();
                        theMessage.Answer("Ich werde " + u.LastUsedName + " ab sofort keine beachtung mehr schenken");
                    }
                    else
                    {
                        theMessage.Answer("Huch den kenne ich nicht :o");
                    }
                }
                else
                {
                    theMessage.Answer("Du bist dazu nicht berechtigt");
                }
            }
        }
    }
}