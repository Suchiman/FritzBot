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

        public void Run(IrcMessage theMessage)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            theMessage.Answer($"Meine Laufzeit beträgt {laufzeit.Days} Tage, {laufzeit.Hours} Stunden, {laufzeit.Minutes} Minuten und {laufzeit.Seconds} Sekunden");
        }
    }
}