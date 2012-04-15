using System;

namespace FritzBot.commands
{
    class db_command : ICommand
    {
        public String[] Name { get { return new String[] { "db" }; } }
        public String HelpText { get { return "Führt Operationen an meiner Datenbank aus, Operator Befehl: !db dbname reload / flush"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String output = "Ich konnte die gewünschte Datenbank nicht finden";
            try
            {
                String[] split = message.Split(' ');
                if (split[1] == "reload")
                {
                    foreach (db database in FritzBot.Program.databases)
                    {
                        if (database.datenbank_name == split[0] || split[0] == "all")
                        {
                            toolbox.getDatabaseByName(database.datenbank_name).Reload();
                            output = "Okay";
                        }
                    }
                }
                else if (split[1] == "flush")
                {
                    foreach (db database in FritzBot.Program.databases)
                    {
                        if (database.datenbank_name == split[0] || split[0] == "all")
                        {
                            toolbox.getDatabaseByName(database.datenbank_name).Write();
                            output = "Okay";
                        }
                    }
                }
                else
                {
                    output = "Das hat nicht funktioniert, denk dran: !db datenbank befehl";
                }
                connection.Sendmsg(output, receiver);
            }
            catch (Exception ex)
            {
                toolbox.Logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                connection.Sendmsg("Wups, das hat eine Exception verursacht", receiver);
            }
        }
    }
}