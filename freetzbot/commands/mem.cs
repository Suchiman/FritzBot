using System;

namespace FritzBot.commands
{
    class mem : ICommand
    {
        public String[] Name { get { return new String[] { "sys", "mem", "ram" }; } }
        public String HelpText { get { return "Ein wenig Systeminfos"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Betriebssystem: " + Environment.OSVersion.ToString() + " CPU's: " + Environment.ProcessorCount + " Mein RAM Verbrauch: " + System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024 + "MByte");
        }
    }
}