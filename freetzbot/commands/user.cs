using System;
using System.Collections.Generic;
using System.Text;

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

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            try
            {
                if (message == "reload")
                {
                    FritzBot.Program.TheUsers.Reload();
                }
                if (message == "flush")
                {
                    FritzBot.Program.TheUsers.Flush();
                }
                if (message.Contains("add"))
                {
                    String[] split = message.Split(' ');
                    FritzBot.Program.TheUsers.Add(split[1]);
                }
                if (message.Contains("box"))
                {
                    String[] split = message.Split(' ');
                    FritzBot.Program.TheUsers[split[1]].AddBox(split[2]);
                }
                if (message.Contains("cleanup"))
                {
                    FritzBot.Program.TheUsers.CleanUp();
                }
                if (message.Contains("remove"))
                {
                    FritzBot.Program.TheUsers.Remove(message.Split(' ')[1]);
                }
                connection.Sendmsg("Okay", receiver);
            }
            catch (Exception ex)
            {
                toolbox.Logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                connection.Sendmsg("Wups, das hat eine Exception verursacht", receiver);
            }
        }
    }
}
