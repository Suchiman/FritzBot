using System;

namespace freetzbot.commands
{
    class alias : command
    {
        private String[] name = { "alias", "a" };
        private String helptext = "Legt einen Alias für einen Begriff fest, z.b. !alias oder !a, \"!a add freetz=Eine Modifikation für...\", \"!a edit freetz=DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen wie z.b. $1 sind möglich.";
        private Boolean op_needed = false;
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

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            alias_command(connection, sender, receiver, message);
        }

        public static Boolean alias_command(irc connection, String sender, String receiver, String message, Boolean not_answered = false)
        {
            String[] parameter = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
            String alias = "";
            Boolean[] cases = new Boolean[3];
            switch (parameter[0].ToLower())
            {
                case "add":
                    if (parameter[1].Contains("="))
                    {
                        cases[2] = true;
                    }
                    else
                    {
                        connection.sendmsg("Das habe ich jetzt nicht als gültigen Alias erkannt, es muss einmal \"=\" enthalten sein, damit ich weiß was der Alias ist", receiver);
                    }
                    break;
                case "edit":
                    if (parameter[1].Contains("="))
                    {
                        cases[1] = true;
                        cases[2] = true;
                    }
                    else
                    {
                        connection.sendmsg("Das habe ich jetzt nicht als gültigen Alias erkannt, es muss einmal \"=\" enthalten sein, damit ich weiß was der Alias ist", receiver);
                    }
                    break;
                case "remove":
                    cases[1] = true;
                    break;
                default:
                    String[] splitted = message.Split(' ');
                    if (splitted.Length > 0)
                    {
                        String[] aliase = toolbox.getDatabaseByName("alias.db").GetContaining(splitted[0] + "=");
                        if (aliase.Length > 0)
                        {
                            String[] thealias = aliase[0].Split(new String[] { "=" }, 2, StringSplitOptions.None);
                            String output = "";
                            int forindex = 0;
                            if (thealias[1].Split('$').Length - 1 < splitted.Length)
                            {
                                forindex = thealias[1].Split('$').Length - 1;
                            }
                            else
                            {
                                forindex = splitted.Length - 1;
                            }
                            output = thealias[1];
                            for (int i = 0; i < forindex; i++)
                            {
                                while (true)
                                {
                                    int index = output.IndexOf("$" + (i + 1));
                                    if (index == -1) break;
                                    output = output.Remove(index, 2);
                                    output = output.Insert(index, splitted[i + 1]);
                                }
                            }
                            connection.sendmsg(output, receiver);
                            return true;
                        }
                        else if(!not_answered)
                        {
                            connection.sendmsg("Diesen Alias gibt es nicht.", receiver);
                            return false;
                        }
                    }
                    return false;
            }
            if (message.Contains("="))
            {
                alias = parameter[1].Split(new String[] { "=" }, 2, StringSplitOptions.None)[0];
            }
            else
            {
                alias = parameter[1];
            }
            if (toolbox.getDatabaseByName("alias.db").GetContaining(alias + "=").Length > 0)
            {
                cases[0] = true;
            }
            if (!cases[0] && cases[1] && cases[2])
            {
                connection.sendmsg("Diesen Alias gibt es noch nicht, verwende \"add\" um ihn hinzuzufügen.", receiver);
                return false;
            }
            if (cases[1])
            {
                if (!cases[0])
                {
                    connection.sendmsg("Diesen Alias gibt es nicht.", receiver);
                    return false;
                }
                toolbox.getDatabaseByName("alias.db").Remove(toolbox.getDatabaseByName("alias.db").GetContaining(alias + "=")[0]);
            }
            if (cases[2])
            {
                if ((cases[1] && cases[2]) ^ cases[0])
                {
                    connection.sendmsg("Es gibt diesen Alias schon, wenn du ihn verändern möchtest verwende statt \"add\", \"edit\".", receiver);
                    return false;
                }
                toolbox.getDatabaseByName("alias.db").Add(parameter[1]);
            }
            if (!cases[1] && cases[2])
            {
                connection.sendmsg("Alias wurde erfolgreich hinzugefügt.", receiver);
            }
            if (cases[1] && cases[2])
            {
                connection.sendmsg("Alias wurde erfolgreich editiert!", receiver);
            }
            if (cases[1] && !cases[2])
            {
                connection.sendmsg("Alias wurde erfolgreich gelöscht!", receiver);
            }
            return false;
        }
    }
}