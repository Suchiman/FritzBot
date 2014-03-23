using FritzBot.Core;
using FritzBot.DataModel;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxremove", "boxdel")]
    [Module.Help("Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".")]
    [Module.ParameterRequired]
    class boxremove : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (DBProvider db = new DBProvider())
            {
                BoxEntry entry = db.QueryLinkedData<BoxEntry, User>(theMessage.TheUser).FirstOrDefault();
                if (entry.RemoveBox(theMessage.CommandLine))
                {
                    theMessage.Answer("Erledigt!");
                    db.SaveOrUpdate(entry);
                }
                else
                {
                    theMessage.Answer("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht");
                }
            }
        }
    }
}