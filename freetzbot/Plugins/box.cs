using FritzBot.Core;
using FritzBot.DataModel;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxadd")]
    [Module.Help("Dies trägt deine Boxdaten ein, Beispiel: \"!boxadd 7270\", bitte jede Box einzeln angeben.")]
    [Module.ParameterRequired]
    class boxadd : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (DBProvider db = new DBProvider())
            {
                BoxEntry boxen = db.QueryLinkedData<BoxEntry, User>(theMessage.TheUser).FirstOrDefault();
                if (boxen == null)
                {
                    boxen = new BoxEntry();
                    boxen.Reference = theMessage.TheUser;
                }

                if (!boxen.HasBox(theMessage.CommandLine))
                {
                    boxen.AddBox(theMessage.CommandLine);
                    theMessage.Answer("Okay danke, ich werde mir deine \"" + theMessage.CommandLine + "\" notieren.");
                }
                else
                {
                    theMessage.Answer("Wups, danke aber du hast mir deine \"" + theMessage.CommandLine + "\" bereits mitgeteilt ;-).");
                }

                db.SaveOrUpdate(boxen);
            }
        }
    }
}