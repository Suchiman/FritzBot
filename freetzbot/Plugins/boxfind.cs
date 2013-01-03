using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxfind")]
    [Module.Help("Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".")]
    [Module.ParameterRequired]
    class boxfind : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            String BoxName = BoxDatabase.GetInstance().GetShortName( theMessage.CommandLine);
            String besitzer = String.Join(", ", UserManager.GetInstance().Where(x => x.GetModulUserStorage("box").Storage.Elements("box").Any(y => y.Value == BoxName)).Select(x => x.names.FirstOrDefault()).ToArray<String>());
            if (!String.IsNullOrEmpty(besitzer))
            {
                theMessage.SendPrivateMessage("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer);
            }
            else
            {
                theMessage.SendPrivateMessage("Diese Box scheint niemand zu besitzen!");
            }
        }
    }
}