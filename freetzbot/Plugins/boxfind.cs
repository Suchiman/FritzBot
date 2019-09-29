using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("boxfind")]
    [Help("Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".")]
    [ParameterRequired]
    class boxfind : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                IQueryable<BoxEntry> filtered;
                if (BoxDatabase.TryFindExactBox(theMessage.CommandLine, out Box? result))
                {
                    filtered = context.BoxEntries.Where(x => x.Box!.Id == result.Id);
                }
                else
                {
                    filtered = context.BoxEntries.Where(x => x.Text.Contains(theMessage.CommandLine));
                }
                string besitzer = filtered.Select(x => x.User.LastUsedName.Name).Where(x => !String.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).Join(", ");
                if (!String.IsNullOrEmpty(besitzer))
                {
                    theMessage.SendPrivateMessage("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer);
                }
                else
                {
                    theMessage.SendPrivateMessage("Diese Box scheint niemand zu besitzen!");
                }
            }
        }
    }
}