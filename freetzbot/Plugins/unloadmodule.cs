using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("rmmod", "unloadmodule")]
    [Module.Help("Deaktiviert einen meiner Befehle")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class unloadmodule : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                string UnloadModuleName = theMessage.CommandArgs[0];
                int unloaded = PluginManager.GetInstance().Remove(x => Module.NameAttribute.IsNamed(x, UnloadModuleName));
                theMessage.Answer(String.Format("{0} Plugin{1} entladen", unloaded, (unloaded == 0 || unloaded > 1) ? "s" : ""));
            }
            catch (Exception ex)
            {
                toolbox.Logging(ex);
            }
        }
    }
}