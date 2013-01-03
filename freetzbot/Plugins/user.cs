using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("user")]
    [Module.Help("Führt Operationen an meiner Benutzerdatenbank aus, Operator Befehl: !user remove, reload, flush, add <name>, box <name> <box>")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class user : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                if (theMessage.CommandArgs[0] == "add")
                {
                    UserManager.GetInstance().Add(theMessage.CommandArgs[1]);
                }
                if (theMessage.CommandArgs[0] == "box")
                {
                    XElement boxen = UserManager.GetInstance()[theMessage.CommandArgs[1]].GetModulUserStorage("box").Storage;
                    if (!boxen.Elements("box").Any(x => x.Value == theMessage.CommandArgs[2]))
                    {
                        boxen.Add(new XElement("box", theMessage.CommandArgs[2]));
                    }
                }
                if (theMessage.CommandArgs[0] == "remove")
                {
                    UserManager.GetInstance().Remove(theMessage.CommandArgs[1]);
                }
                if (theMessage.CommandArgs[0] == "maintain")
                {
                    UserManager.GetInstance().Maintain();
                }
                theMessage.Answer("Okay");
            }
            catch (Exception ex)
            {
                toolbox.Logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                theMessage.Answer("Wups, das hat eine Exception verursacht");
            }
        }
    }
}
