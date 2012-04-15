using System;
using System.Collections.Generic;
using System.Text;

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

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            try
            {
                for (int i = 0; i < FritzBot.Program.Commands.Count; i++)
                {
                    if (FritzBot.Program.Commands[i].Name[0] == message)
                    {
                        FritzBot.Program.Commands[i].Destruct();
                        FritzBot.Program.Commands[i] = null;
                        FritzBot.Program.Commands.RemoveAt(i);
                        connection.Sendmsg("Modul erfolgreich entladen", receiver);
                        return;
                    }
                }
                connection.Sendmsg("Modul wurde nicht gefunden", receiver);
            }
            catch (Exception ex)
            {
                connection.Sendmsg("Das hat eine Exception ausgelöst", receiver);
                toolbox.Logging("Unloadmodule Exception " + ex.Message);
            }
        }
    }
}
