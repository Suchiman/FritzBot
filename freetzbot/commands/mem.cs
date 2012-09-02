using System;

namespace FritzBot.commands
{
    [Module.Name("sys", "mem", "ram")]
    [Module.Help("Ein wenig Systeminfos")]
    [Module.ParameterRequired(false)]
    class mem : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Betriebssystem: " + Environment.OSVersion.ToString() + " CPU's: " + Environment.ProcessorCount + " Mein RAM Verbrauch: " + System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024 + "MByte");
        }
    }
}