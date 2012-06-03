using System;

namespace FritzBot.commands
{
    class unloadmodule : ICommand
    {
        public String[] Name { get { return new String[] { "rmmod", "unloadmodule" }; } }
        public String HelpText { get { return "Deaktiviert einen meiner Befehle"; } }
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
                for (int i = 0; i < Program.Commands.Count; i++)
                {
                    if (Program.Commands[i].Name[0] == theMessage.CommandArgs[0])
                    {
                        Program.Commands[i].Destruct();
                        Program.Commands[i] = null;
                        Program.Commands.RemoveAt(i);
                        Properties.Settings.Default.IgnoredModules.Add(theMessage.CommandArgs[0]);
                        Properties.Settings.Default.Save();
                        theMessage.Answer("Modul erfolgreich entladen");
                        return;
                    }
                }
                theMessage.Answer("Modul wurde nicht gefunden");
            }
            catch (Exception ex)
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
                toolbox.Logging("Unloadmodule Exception " + ex.Message);
            }
        }
    }
}
