using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("zeit")]
    [Module.Help("Das gibt die aktuelle Uhrzeit aus.")]
    [Module.ParameterRequired(false)]
    class zeit : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                theMessage.Answer("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss") + " am " + DateTime.Now.ToShortDateString());
            }
            catch
            {
                theMessage.Answer("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(");
            }
        }
    }
}