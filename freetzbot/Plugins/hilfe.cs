using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("hilfe", "help", "faq", "info", "man", "lsmod")]
    [Help("Die Hilfe!")]
    class hilfe : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                var plugins = PluginManager.Plugins.Where(x => !x.IsHidden && x.Names.Count > 0);
                using (var context = new BotContext())
                {
                    if (!toolbox.IsOp(context.GetUser(theMessage.Nickname)))
                    {
                        plugins = plugins.Where(x => !x.AuthenticationRequired);
                    }
                }

                List<string> befehle = plugins.Select(x => x.Names[0]).OrderBy(x => x).ToList();

                theMessage.Answer("Derzeit verfügbare Befehle: " + String.Join(", ", befehle));
                theMessage.Answer("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen");
            }
            else
            {
                PluginInfo info = PluginManager.Get(theMessage.CommandArgs[0]);
                if (info != null && !String.IsNullOrEmpty(info.HelpText))
                {
                    theMessage.Answer(info.HelpText);
                }
                else
                {
                    theMessage.Answer("Ich konnte keinen Befehl finden der so heißt");
                }
            }
        }
    }
}