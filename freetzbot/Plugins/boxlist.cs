using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("boxlist")]
    [Help("Dies listet alle registrierten Boxtypen auf.")]
    [ParameterRequired(false)]
    class boxlist : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                string boxen = context.BoxEntries.Select(x => x.Box!.ShortName ?? x.Text).Where(x => x != null).Distinct().Join(", ");
                theMessage.Answer("Folgende Boxen wurden bei mir registriert: " + boxen);
            }
        }
    }
}