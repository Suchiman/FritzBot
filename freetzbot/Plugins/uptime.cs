using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("uptime", "laufzeit")]
    [Help("Das zeigt meine aktuelle Laufzeit an und wie lange ich mit diesem ServerConnetion verbunden bin.")]
    [ParameterRequired(false)]
    class uptime : PluginBase, ICommand
    {
        private DateTime startzeit;

        public uptime()
        {
            startzeit = DateTime.Now;
        }

        public void Run(ircMessage theMessage)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            theMessage.Answer(String.Format("Meine Laufzeit beträgt {0} Tage, {1} Stunden, {2} Minuten und {3} Sekunden", laufzeit.Days, laufzeit.Hours, laufzeit.Minutes, laufzeit.Seconds));
        }
    }
}