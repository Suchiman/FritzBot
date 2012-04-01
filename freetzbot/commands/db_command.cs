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
            String output = "Okay";
            try
            {
                String[] split = message.Split(' ');
                if (split[1] == "reload")
                {
                    switch (split[0])
                    {
                        case "box":
                            toolbox.getDatabaseByName("box.db").Reload();
                            break;
                        case "user":
                            toolbox.getDatabaseByName("user.db").Reload();
                            break;
                        case "witz":
                            toolbox.getDatabaseByName("witze.db").Reload();
                            break;
                        case "ignore":
                            toolbox.getDatabaseByName("ignore.db").Reload();
                            break;
                        case "server":
                            toolbox.getDatabaseByName("servers.cfg").Reload();
                            break;
                        case "fw":
                            toolbox.getDatabaseByName("fwdb.db").Reload();
                            break;
                        case "alias":
                            toolbox.getDatabaseByName("alias.db").Reload();
                            break;
                        case "all":
                            toolbox.getDatabaseByName("box.db").Reload();
                            toolbox.getDatabaseByName("user.db").Reload();
                            toolbox.getDatabaseByName("witze.db").Reload();
                            toolbox.getDatabaseByName("ignore.db").Reload();
                            toolbox.getDatabaseByName("servers.cfg").Reload();
                            toolbox.getDatabaseByName("fwdb.db").Reload();
                            toolbox.getDatabaseByName("alias.db").Reload();
                            break;
                        default:
                            output = "Wups, die Datenbank kenn ich nich";
                            break;
                    }
                }
                else if (split[1] == "flush")
                {
                    switch (split[0])
                    {
                        case "box":
                            toolbox.getDatabaseByName("box.db").Write();
                            break;
                        case "user":
                            toolbox.getDatabaseByName("user.db").Write();
                            break;
                        case "witz":
                            toolbox.getDatabaseByName("witze.db").Write();
                            break;
                        case "ignore":
                            toolbox.getDatabaseByName("ignore.db").Write();
                            break;
                        case "server":
                            toolbox.getDatabaseByName("servers.cfg").Write();
                            break;
                        case "fw":
                            toolbox.getDatabaseByName("fwdb.db").Write();
                            break;
                        case "alias":
                            toolbox.getDatabaseByName("alias.db").Write();
                            break;
                        case "all":
                            toolbox.getDatabaseByName("box.db").Write();
                            toolbox.getDatabaseByName("user.db").Write();
                            toolbox.getDatabaseByName("witze.db").Write();
                            toolbox.getDatabaseByName("ignore.db").Write();
                            toolbox.getDatabaseByName("servers.cfg").Write();
                            toolbox.getDatabaseByName("fwdb.db").Write();
                            toolbox.getDatabaseByName("alias.db").Write();
                            break;
                        default:
                            output = "Wups, die Datenbank kenn ich nich";
                            break;
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