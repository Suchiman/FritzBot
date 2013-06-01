using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("hilfe", "help", "faq", "info", "man", "lsmod")]
    [Module.Help("Die Hilfe!")]
    class hilfe : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                List<string> befehle = new List<string>();

                foreach (PluginInfo theCommand in PluginManager.GetInstance().Where(x => !x.IsHidden && x.Names.Count > 0))
                {
                    if (!theCommand.AuthenticationRequired || toolbox.IsOp(theMessage.TheUser))
                    {
                        befehle.Add(theCommand.Names[0]);
                    }
                }
                befehle.Sort();
                theMessage.Answer("Derzeit verfügbare Befehle: " + String.Join(", ", befehle));
                theMessage.Answer("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen");
            }
            else
            {
                PluginInfo info = PluginManager.GetInstance().Get(theMessage.CommandArgs[0]);
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