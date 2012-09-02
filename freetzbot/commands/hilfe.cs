using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("hilfe", "help", "faq", "info", "man", "lsmod")]
    [Module.Help("Die Hilfe!")]
    class hilfe : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                List<String> befehle = new List<String>();
                foreach (ICommand theCommand in Program.Commands)
                {
                    Module.AuthorizeAttribute Authorization = (Attribute.GetCustomAttribute(theCommand.GetType(), typeof(Module.AuthorizeAttribute)) as Module.AuthorizeAttribute);
                    Module.HelpAttribute Hilfe = (Attribute.GetCustomAttribute(theCommand.GetType(), typeof(Module.HelpAttribute)) as Module.HelpAttribute);
                    Module.NameAttribute Namen = (Attribute.GetCustomAttribute(theCommand.GetType(), typeof(Module.NameAttribute)) as Module.NameAttribute);
                    if (Authorization == null || toolbox.IsOp(theMessage.Nick))
                    {
                        befehle.Add(Namen.Names[0]);
                    }
                }
                befehle.Sort();
                String output = "";
                foreach (String data in befehle)
                {
                    output += ", " + data;
                }
                output = output.Remove(0, 2);
                theMessage.Answer("Derzeit verfügbare Befehle: " + output);
                theMessage.Answer("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen");
            }
            else
            {
                try
                {
                    Module.HelpAttribute Hilfe = (Attribute.GetCustomAttribute(toolbox.getCommandByName(theMessage.CommandArgs[0]).GetType(), typeof(Module.HelpAttribute)) as Module.HelpAttribute);
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