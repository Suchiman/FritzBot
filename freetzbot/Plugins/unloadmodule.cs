using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
using System;

namespace FritzBot.Plugins
{
    [Name("rmmod", "unloadmodule")]
    [Help("Deaktiviert einen meiner Befehle")]
    [ParameterRequired]
    [Authorize]
    class unloadmodule : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            try
            {
                string UnloadModuleName = theMessage.CommandArgs[0];
                int unloaded = PluginManager.Remove(x => x.Names.Contains(UnloadModuleName));
                theMessage.Answer($"{unloaded} Plugin{((unloaded == 0 || unloaded > 1) ? "s" : "")} entladen");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fehler beim entladen des Plugins");
            }
        }
    }
}