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
            string BoxName = BoxDatabase.GetInstance().GetShortName(theMessage.CommandLine);
            using (DBProvider db = new DBProvider())
            {
                string[] usernames = db.Query<BoxEntry>(x => x.HasBox(theMessage.CommandLine)).Select(x => x.Reference).NotNull().Select(x => x.LastUsedName).ToArray();
                string besitzer = String.Join(", ", usernames);
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
}