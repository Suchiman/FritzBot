using System;

namespace freetzbot.commands
{
    class uptime : command
    {
        private String[] name = { "uptime", "laufzeit" };
        private String helptext = "Das zeigt meine aktuelle Laufzeit an und wie lange ich mit diesem Server verbunden bin.";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public uptime()
        {
            startzeit = DateTime.Now;
        }

        private DateTime startzeit;

        public void run(irc connection, String sender, String receiver, String message)
        {
            TimeSpan laufzeit = DateTime.Now.Subtract(startzeit);
            TimeSpan connecttime = connection.uptime();
            connection.sendmsg("Meine Laufzeit beträgt " + laufzeit.Days + " Tage, " + laufzeit.Hours + " Stunden, " + laufzeit.Minutes + " Minuten und " + laufzeit.Seconds + " Sekunden und bin mit diesem Server seit " + connecttime.Days + " Tage, " + connecttime.Hours + " Stunden, " + connecttime.Minutes + " Minuten und " + connecttime.Seconds + " Sekunden verbunden", receiver);
        }
    }
}