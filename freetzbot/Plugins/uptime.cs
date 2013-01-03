using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("uptime", "laufzeit")]
    [Module.Help("Das zeigt meine aktuelle Laufzeit an und wie lange ich mit diesem Server verbunden bin.")]
    [Module.ParameterRequired(false)]
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
            TimeSpan connecttime = theMessage.IRC.Uptime;
            theMessage.Answer("Meine Laufzeit beträgt " + laufzeit.Days + " Tage, " + laufzeit.Hours + " Stunden, " + laufzeit.Minutes + " Minuten und " + laufzeit.Seconds + " Sekunden und bin mit diesem Server seit " + connecttime.Days + " Tage, " + connecttime.Hours + " Stunden, " + connecttime.Minutes + " Minuten und " + connecttime.Seconds + " Sekunden verbunden");
        }
    }
}