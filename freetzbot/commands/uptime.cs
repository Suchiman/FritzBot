using System;

namespace FritzBot.commands
{
    class uptime : ICommand
    {
        public String[] Name { get { return new String[] { "uptime", "laufzeit" }; } }
        public String HelpText { get { return "Das zeigt meine aktuelle Laufzeit an und wie lange ich mit diesem Server verbunden bin."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public uptime()
        {
            startzeit = DateTime.Now;
        }

        public void Destruct()
        {

        }

        private DateTime startzeit;

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            TimeSpan connecttime = connection.Uptime();
            connection.Sendmsg("Meine Laufzeit beträgt " + laufzeit.Days + " Tage, " + laufzeit.Hours + " Stunden, " + laufzeit.Minutes + " Minuten und " + laufzeit.Seconds + " Sekunden und bin mit diesem Server seit " + connecttime.Days + " Tage, " + connecttime.Hours + " Stunden, " + connecttime.Minutes + " Minuten und " + connecttime.Seconds + " Sekunden verbunden", receiver);
        }
    }
}