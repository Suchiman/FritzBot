using System;
using System.Reflection;

namespace FritzBot.commands
{
    class loadmodule : ICommand
    {
        public String[] Name { get { return new String[] { "modprobe", "insmod", "loadmodule" }; } }
        public String HelpText { get { return "Aktiviert einen meiner Befehle"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            try
            {
                Type t = Assembly.GetExecutingAssembly().GetType("FritzBot.commands." + message);
                if (t == null)
                {
                    connection.Sendmsg("Modul wurde nicht gefunden", receiver);
                    return;
                }
                FritzBot.Program.Commands.Add((ICommand)Activator.CreateInstance(t));
                connection.Sendmsg("Modul erfolgreich geladen", receiver);
            }
            catch
            {
                connection.Sendmsg("Das hat eine Exception ausgelöst", receiver);
            }
        }
    }
}
