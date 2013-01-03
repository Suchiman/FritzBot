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
            if (UserManager.GetInstance().Exists(theMessage.CommandLine))
            {
                UserManager.GetInstance()[theMessage.CommandLine].IsOp = true;
                theMessage.Answer("Okay");
            }
            else
            {
                theMessage.Answer("Den Benutzer kenne ich nicht");
            }
        }
    }
}
