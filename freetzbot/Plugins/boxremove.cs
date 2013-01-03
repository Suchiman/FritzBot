using FritzBot.DataModel;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxremove")]
    [Module.Help("Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".")]
    [Module.ParameterRequired]
    class boxremove : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            String BoxName = BoxDatabase.GetInstance().GetShortName(theMessage.CommandLine);
            XElement boxToDelete = theMessage.TheUser.GetModulUserStorage("box").Storage.Elements("box").FirstOrDefault(x => x.Value == BoxName);
            if (boxToDelete != null)
            {
                boxToDelete.Remove();
                theMessage.Answer("Erledigt!");
                return;
            }
            theMessage.Answer("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht");
        }
    }
}