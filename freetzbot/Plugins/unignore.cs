using FritzBot.Core;
using FritzBot.DataModel;
using System;

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
            UserManager um = UserManager.GetInstance();
            if (um.Exists(theMessage.CommandArgs[0]))
            {
                um[theMessage.CommandArgs[0]].ignored = false;
                theMessage.Answer(String.Format("Ingoranz für {0} aufgehoben", um[theMessage.CommandArgs[0]].LastUsedNick));
            }
            theMessage.Answer("Oh... Dieser User ist mir nicht bekannt");
        }
    }
}