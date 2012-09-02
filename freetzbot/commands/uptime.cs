using System;

namespace FritzBot.commands
{
    [Module.Name("uptime", "laufzeit")]
    [Module.Help("Das zeigt meine aktuelle Laufzeit an und wie lange ich mit diesem Server verbunden bin.")]
    [Module.ParameterRequired(false)]
    class uptime : ICommand
    {
        private DateTime startzeit;

        public uptime()
        {
            startzeit = DateTime.Now;
        }

        public void Run(ircMessage theMessage)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            TimeSpan connecttime = theMessage.Connection.Uptime;
            theMessage.Answer("Meine Laufzeit beträgt " + laufzeit.Days + " Tage, " + laufzeit.Hours + " Stunden, " + laufzeit.Minutes + " Minuten und " + laufzeit.Seconds + " Sekunden und bin mit diesem Server seit " + connecttime.Days + " Tage, " + connecttime.Hours + " Stunden, " + connecttime.Minutes + " Minuten und " + connecttime.Seconds + " Sekunden verbunden");
        }
    }
}