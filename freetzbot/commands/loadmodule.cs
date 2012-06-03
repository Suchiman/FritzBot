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

        public void Run(ircMessage theMessage)
        {
            try
            {
                Type t = Assembly.GetExecutingAssembly().GetType("FritzBot.commands." + theMessage.CommandLine);
                if (t == null)
                {
                    theMessage.Answer("Modul wurde nicht gefunden");
                    return;
                }
                Program.Commands.Add((ICommand)Activator.CreateInstance(t));
                Properties.Settings.Default.IgnoredModules.Remove(theMessage.CommandLine);
                Properties.Settings.Default.Save();
                theMessage.Answer("Modul erfolgreich geladen");
            }
            catch
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
            }
        }
    }
}
