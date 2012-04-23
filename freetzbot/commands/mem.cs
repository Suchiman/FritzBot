using System;
using FritzBot;

namespace FritzBot.commands
{
    class mem : ICommand
    {
        public String[] Name { get { return new String[] { "mem" }; } }
        public String HelpText { get { return "Meine aktuelle Speicherlast berechnet vom GC (Gargabe Collector) und die insgesamt Last"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            connection.Sendmsg("GC Totalmem: " + GC.GetTotalMemory(true).ToString() + "Byte, WorkingSet: " + (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024).ToString() + "kB", receiver);
        }
    }
}