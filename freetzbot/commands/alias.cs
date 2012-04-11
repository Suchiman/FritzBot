using System;
using System.Collections.Generic;

namespace freetzbot.commands
{
    class alias : command
    {
        private String[] name = { "alias", "a" };
        private String helptext = "Legt einen Alias für einen Begriff fest, z.b. !alias oder !a, \"!a add freetz Eine Modifikation für...\", \"!a edit freetz DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen wie z.b. $1 sind möglich.";
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
            String[] parameter = message.Split(' ');
            Boolean[] cases = new Boolean[3];
            switch (parameter[0].ToLower())
            {
                case "add":
                    if (freetzbot.Program.TheUsers.AllAliases()[parameter[1]] == "")
                    {
                        freetzbot.Program.TheUsers[sender].alias[parameter[1]] = String.Join(" ", parameter, 2, parameter.Length - 2);
                        connection.sendmsg("Der Alias wurde erfolgreich hinzugefügt", receiver);
                        return true;
                    }
                    connection.sendmsg("Diesen Alias gibt es bereits", receiver);
                    return false;
                case "edit":
                    freetzbot.Program.TheUsers[sender].alias[parameter[1]] = String.Join(" ", parameter, 2, parameter.Length - 2);
                    connection.sendmsg("Der Alias wurde erfolgreich bearbeitet", receiver);
                    return true;
                case "remove":
                    if (freetzbot.Program.TheUsers[sender].alias[parameter[1]] != "")
                    {
                        freetzbot.Program.TheUsers[sender].alias[parameter[1]] = "";
                        connection.sendmsg("Alias wurde gelöscht", receiver);
                    }
                    else if (toolbox.op_check(sender))
                    {
                        foreach (User oneuser in freetzbot.Program.TheUsers)
                        {
                            if (oneuser.alias[parameter[1]] != "")
                            {
                                oneuser.alias[parameter[1]] = "";
                                connection.sendmsg("Alias wurde gelöscht", receiver);
                                return true;
                            }
                        }
                        connection.sendmsg("Alias wurde nicht gefunden", receiver);
                    }
                    else
                    {
                        connection.sendmsg("Du scheinst keinen solchen Alias definiert zu haben", receiver);
                    }
                    return true;
                default:
                    String thealias = freetzbot.Program.TheUsers.AllAliases()[parameter[0]];
                    if (thealias != "")
                    {
                        for (int i = 0; thealias.Contains("$") && parameter.Length > 1; i++)
                        {
                            while (true)
                            {
                                int index = thealias.IndexOf("$" + (i + 1));
                                if (index == -1) break;
                                thealias = thealias.Remove(index, 2);
                                thealias = thealias.Insert(index, parameter[i + 1]);
                            }
                        }
                        connection.sendmsg(thealias, receiver);
                        return true;
                    }
                    if (!not_answered)
                    {
                        connection.sendmsg("Diesen Alias gibt es nicht.", receiver);
                    }
                    return false;
            }
        }
    }
}

namespace freetzbot
{
    public class alias_db
    {
        public List<String> alias;
        public List<String> description;
        public alias_db()
        {
            alias = new List<String>();
            description = new List<String>();
        }
        public String this[String thealias]
        {
            get
            {
                for (int i = 0; i < alias.Count; i++)
                {
                    if (alias[i] == thealias)
                    {
                        return description[i];
                    }
                }
                return "";
            }
            set
            {
                for (int i = 0; i < alias.Count; i++)
                {
                    if (alias[i] == thealias)
                    {
                        if (value == "")
                        {
                            alias.RemoveAt(i);
                            description.RemoveAt(i);
                        }
                        else
                        {
                            description[i] = value;
                        }
                        return;
                    }
                }
                if (value != "")
                {
                    alias.Add(thealias);
                    description.Add(value);
                }
            }
        }
    }
}