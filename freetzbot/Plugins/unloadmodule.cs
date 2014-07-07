using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("rmmod", "unloadmodule")]
    [Help("Deaktiviert einen meiner Befehle")]
    [ParameterRequired]
    [Authorize]
    class unloadmodule : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                string UnloadModuleName = theMessage.CommandArgs[0];
                int unloaded = PluginManager.Remove(x => x.Names.Contains(UnloadModuleName));
                theMessage.Answer(String.Format("{0} Plugin{1} entladen", unloaded, (unloaded == 0 || unloaded > 1) ? "s" : ""));
            }
            catch (Exception ex)
            {
                toolbox.Logging(ex);
            }
        }
    }
}