using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("unignore")]
    [Module.Help("Die betroffene Person wird von der ignore Liste gestrichen, Operator Befehl: z.b. !unignore Testnick")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class unignore : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(theMessage.CommandArgs.FirstOrDefault());
                if (u != null)
                {
                    u.Ignored = false;
                    theMessage.Answer(String.Format("Ignoranz für {0} aufgehoben", u.LastUsedName));
                    return;
                }
                theMessage.Answer("Oh... Dieser User ist mir nicht bekannt");
            }
        }
    }
}