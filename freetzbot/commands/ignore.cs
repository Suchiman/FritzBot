using System;

namespace FritzBot.commands
{
    [Module.Name("ignore")]
    [Module.Help("Schließt die angegebene Person von mir aus")]
    [Module.ParameterRequired]
    class ignore : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.Source == theMessage.CommandLine || toolbox.IsOp(theMessage.Nick))
            {
                theMessage.TheUsers[theMessage.Nick].ignored = true;
                theMessage.Answer("Ich werde " + theMessage.CommandLine + " ab sofort keine beachtung mehr schenken");
            }
        }
    }
}