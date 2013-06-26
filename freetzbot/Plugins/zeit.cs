using FritzBot.DataModel;
using System;
using System.Globalization;

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
                DateTime now = DateTime.Now;
                theMessage.Answer("Laut meiner Uhr ist es gerade " + now.ToString("HH:mm:ss") + " am " + now.ToShortDateString() + " in der Kalenderwoche " + GetKalenderwoche(now));
            }
            catch
            {
                theMessage.Answer("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(");
            }
        }

        public int GetKalenderwoche(DateTime day)
        {
            CultureInfo CUI = CultureInfo.CurrentCulture;
            return CUI.Calendar.GetWeekOfYear(day, CUI.DateTimeFormat.CalendarWeekRule, CUI.DateTimeFormat.FirstDayOfWeek);
        }
    }
}