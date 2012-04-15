using System;

namespace FritzBot.commands
{
    class settings : ICommand
    {
        public String[] Name { get { return new String[] { "settings" }; } }
        public String HelpText { get { return "Ändert meine Einstellungen, Operator Befehl: !settings get name, !settings set name wert"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String[] split = message.Split(' ');
            switch (split[0])
            {
                case "set":
                    try
                    {
                        FritzBot.Program.configuration[split[1]] = split[2];
                    }
                    catch
                    {
                        connection.Sendmsg("Wups, das hat eine Exception ausgelöst", receiver);
                        return;
                    }
                    connection.Sendmsg("Okay", receiver);
                    break;
                case "get":
                    try
                    {
                        connection.Sendmsg(FritzBot.Program.configuration[split[1]], receiver);
                    }
                    catch
                    {
                        connection.Sendmsg("Wups, das hat eine Exception ausgelöst", receiver);
                        return;
                    }
                    break;
                default:
                    connection.Sendmsg("Wups, da stimmt wohl etwas mit der Syntax nicht", receiver);
                    break;
            }
        }
    }
}