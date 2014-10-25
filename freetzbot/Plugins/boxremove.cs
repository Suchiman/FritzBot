using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("boxremove", "boxdel")]
    [Help("Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".")]
    [ParameterRequired]
    class boxremove : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                BoxManager mgr = new BoxManager(context.GetUser(theMessage.Nickname), context);
                if (mgr.RemoveBox(theMessage.CommandLine))
                {
                    theMessage.Answer("Erledigt!");
                }
                else
                {
                    theMessage.Answer("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht");
                }
            }
        }
    }
}