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
            using (DBProvider db = new DBProvider())
            {
                string boxen = String.Join(", ", db.Query<BoxEntry>().SelectMany(x => x.GetMapAbleBoxen()).Distinct().Select(x => x.ShortName).ToArray());
                theMessage.Answer("Folgende Boxen wurden bei mir registriert: " + boxen);
            }
        }
    }
}