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
                List<String> befehle = new List<String>();
                foreach (ICommand theCommand in PluginManager.GetInstance().Get<ICommand>().Where(x => !Module.HiddenAttribute.CheckHidden(x) && toolbox.GetAttribute<Module.NameAttribute>(x) != null))
                {
                    Module.AuthorizeAttribute Authorization = toolbox.GetAttribute<Module.AuthorizeAttribute>(theCommand); 
                    Module.NameAttribute Namen = toolbox.GetAttribute<Module.NameAttribute>(theCommand);
                    if (Authorization == null || toolbox.IsOp(theMessage.Nickname))
                    {
                        befehle.Add(Namen.Names[0]);
                    }
                }
                befehle.Sort();
                theMessage.Answer("Derzeit verfügbare Befehle: " + String.Join(", ", befehle.ToArray()));
                theMessage.Answer("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen");
            }
            else
            {
                try
                {
                    Module.HelpAttribute Hilfe = toolbox.GetAttribute<Module.HelpAttribute>(PluginManager.GetInstance().Get<ICommand>(theMessage.CommandArgs[0]));
                    theMessage.Answer(Hilfe.Help);
                }
                catch
                {
                    theMessage.Answer("Ich konnte keinen Befehl finden der so heißt");
                }
            }
        }
    }
}