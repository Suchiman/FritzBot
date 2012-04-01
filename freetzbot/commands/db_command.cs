using System;

namespace freetzbot.commands
{
    class db_command : command
    {
        private String[] name = { "db" };
        private String helptext = "Führt Operationen an meiner Datenbank aus, Operator Befehl: !db dbname reload / flush";
        private Boolean op_needed = true;
        private Boolean parameter_needed = true;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            String output = "Ich konnte die gewünschte Datenbank nicht finden";
            try
            {
                String[] split = message.Split(' ');
                if (split[1] == "reload")
                {
                    foreach (db database in freetzbot.Program.databases)
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
                    foreach (db database in freetzbot.Program.databases)
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
                connection.sendmsg(output, receiver);
            }
            catch (Exception ex)
            {
                toolbox.logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                connection.sendmsg("Wups, das hat eine Exception verursacht", receiver);
            }
        }
    }
}