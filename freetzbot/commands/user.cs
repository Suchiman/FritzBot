using System;

namespace FritzBot.commands
{
    class user : ICommand
    {
        public String[] Name { get { return new String[] { "user" }; } }
        public String HelpText { get { return "Führt Operationen an meiner Benutzerdatenbank aus, Operator Befehl: !user remove, reload, flush, add <name>, box <name> <box>"; } }
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
                if (theMessage.CommandArgs[0] == "reload")
                {
                    Program.TheUsers.Reload();
                }
                if (theMessage.CommandArgs[0] == "flush")
                {
                    Program.TheUsers.Flush();
                }
                if (theMessage.CommandArgs[0] == "add")
                {
                    theMessage.TheUsers.Add(theMessage.CommandArgs[1]);
                }
                if (theMessage.CommandArgs[0] == "box")
                {
                    theMessage.TheUsers[theMessage.CommandArgs[1]].AddBox(theMessage.CommandArgs[2]);
                }
                if (theMessage.CommandArgs[0] == "cleanup")
                {
                    theMessage.TheUsers.CleanUp();
                }
                if (theMessage.CommandArgs[0] == "remove")
                {
                    theMessage.TheUsers.Remove(theMessage.CommandArgs[1]);
                }
                if (theMessage.CommandArgs[0] == "maintain")
                {
                    theMessage.TheUsers.Maintain();
                }
                theMessage.Answer("Okay");
            }
            catch (Exception ex)
            {
                toolbox.Logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                theMessage.Answer("Wups, das hat eine Exception verursacht");
            }
        }
    }
}
