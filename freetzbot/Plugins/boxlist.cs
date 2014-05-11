using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("boxlist")]
    [Help("Dies listet alle registrierten Boxtypen auf.")]
    [ParameterRequired(false)]
    class boxlist : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (var context = new BotContext())
            {
                string boxen = String.Join(", ", context.BoxEntries.Where(x => x.Box != null).Select(x => x.Box.ShortName).Distinct());
                theMessage.Answer("Folgende Boxen wurden bei mir registriert: " + boxen);
            }
        }
    }
}