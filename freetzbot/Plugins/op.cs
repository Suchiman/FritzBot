using FritzBot.Core;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("op")]
    [Module.Help("Erteilt einem Benutzer Operator rechte")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class op : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(theMessage.CommandLine);
                if (u == null)
                {
                    theMessage.Answer("Den Benutzer kenne ich nicht");
                    return;
                }
                u.Admin = true;
                db.SaveOrUpdate(u);
                theMessage.Answer("Okay");
            }
        }
    }
}