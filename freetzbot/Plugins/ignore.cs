using FritzBot.Core;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("ignore")]
    [Module.Help("Schließt die angegebene Person von mir aus")]
    [Module.ParameterRequired]
    class ignore : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.Source == theMessage.CommandLine || toolbox.IsOp(theMessage.TheUser))
            {
                using (DBProvider db = new DBProvider())
                {
                    User u = db.GetUser(theMessage.CommandLine);
                    if (u != null)
                    {
                        u.Ignored = true;
                        db.SaveOrUpdate(u);
                        theMessage.Answer("Ich werde " + u.LastUsedName + " ab sofort keine beachtung mehr schenken");
                    }
                    else
                    {
                        theMessage.Answer("Huch den kenne ich nicht :o");
                    }
                }
            }
        }
    }
}