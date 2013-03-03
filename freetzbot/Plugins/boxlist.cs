using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxlist")]
    [Module.Help("Dies listet alle registrierten Boxtypen auf.")]
    [Module.ParameterRequired(false)]
    class boxlist : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string boxen = String.Join(", ", UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage("box").Storage.Elements("box")).Select(x => x.Value).Distinct().ToArray<string>());
            theMessage.Answer("Folgende Boxen wurden bei mir registriert: " + boxen);
        }
    }
}