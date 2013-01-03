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
            if (theMessage.Source == theMessage.CommandLine || toolbox.IsOp(theMessage.Nickname))
            {
                UserManager.GetInstance()[theMessage.Nickname].ignored = true;
                theMessage.Answer("Ich werde " + theMessage.CommandLine + " ab sofort keine beachtung mehr schenken");
            }
        }
    }
}