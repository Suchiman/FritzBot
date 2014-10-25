using FritzBot.DataModel;
using NCalc;
using System;

namespace FritzBot.Plugins
{
    [Name("calc")]
    [Help("Ich kann sogar Rechnen :-) !calc 42*13+1 !calc 42*(42-(24+24)+1*3)/2")]
    [ParameterRequired]
    class calc : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            try
            {
                Expression exp = new Expression(theMessage.CommandLine);
                object result = exp.Evaluate();
                theMessage.Answer(String.Format("{0} ergibt {1}", exp.ParsedExpression, result));
            }
            catch (Exception ex)
            {
                theMessage.Answer("Die Eingabe ist ungültig oder konnte nicht interpretiert werden: " + ex.Message);
            }
        }
    }
}