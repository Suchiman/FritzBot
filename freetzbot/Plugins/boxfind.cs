using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("boxfind")]
    [Help("Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".")]
    [ParameterRequired]
    class boxfind : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (var context = new BotContext())
            {
                Box result;
                IQueryable<BoxEntry> filtered;
                if (BoxDatabase.TryFindExactBox(theMessage.CommandLine, out result))
                {
                    filtered = context.BoxEntries.Where(x => x.Box == result);
                }
                else
                {
                    filtered = context.BoxEntries.Where(x => x.Text.Contains(theMessage.CommandLine));
                }
                List<string> usernames = filtered.Select(x => x.User.LastUsedName.Name).Where(x => !String.IsNullOrEmpty(x)).ToList();
                string besitzer = usernames.Join(", ");
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