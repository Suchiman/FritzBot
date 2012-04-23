using System;
using System.Collections.Generic;
using FritzBot;

namespace FritzBot.commands
{
    class alias : ICommand
    {
        public String[] Name { get { return new String[] { "alias", "a" }; } }
        public String HelpText { get { return "Legt einen Alias für einen Begriff fest, z.b. !alias oder !a, \"!a add freetz Eine Modifikation für...\", \"!a edit freetz DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen wie z.b. $1 sind möglich."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            AliasCommand(connection, sender, receiver, message);
        }

        public static Boolean AliasCommand(Irc connection, String sender, String receiver, String message, Boolean not_answered = false)
        {
            String[] parameter = message.Split(' ');
            switch (parameter[0].ToLower())
            {
                case "add":
                    if (String.IsNullOrEmpty(Program.TheUsers.AllAliases()[parameter[1]]))
                    {
                        Program.TheUsers[sender].alias[parameter[1]] = String.Join(" ", parameter, 2, parameter.Length - 2);
                        connection.Sendmsg("Der Alias wurde erfolgreich hinzugefügt", receiver);
                        return true;
                    }
                    connection.Sendmsg("Diesen Alias gibt es bereits", receiver);
                    return false;
                case "edit":
                    Program.TheUsers[sender].alias[parameter[1]] = String.Join(" ", parameter, 2, parameter.Length - 2);
                    connection.Sendmsg("Der Alias wurde erfolgreich bearbeitet", receiver);
                    return true;
                case "remove":
                    if (!String.IsNullOrEmpty(Program.TheUsers[sender].alias[parameter[1]]))
                    {
                        Program.TheUsers[sender].alias[parameter[1]] = "";
                        connection.Sendmsg("Alias wurde gelöscht", receiver);
                    }
                    else if (toolbox.IsOp(sender))
                    {
                        foreach (User oneuser in Program.TheUsers)
                        {
                            if (!String.IsNullOrEmpty(oneuser.alias[parameter[1]]))
                            {
                                oneuser.alias[parameter[1]] = "";
                                connection.Sendmsg("Alias wurde gelöscht", receiver);
                                return true;
                            }
                        }
                        connection.Sendmsg("Alias wurde nicht gefunden", receiver);
                    }
                    else
                    {
                        connection.Sendmsg("Du scheinst keinen solchen Alias definiert zu haben", receiver);
                    }
                    return true;
                default:
                    String thealias = Program.TheUsers.AllAliases()[parameter[0]];
                    if (!String.IsNullOrEmpty(thealias))
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
                        connection.Sendmsg(thealias, receiver);
                        return true;
                    }
                    if (!not_answered)
                    {
                        connection.Sendmsg("Diesen Alias gibt es nicht.", receiver);
                    }
                    return false;
            }
        }
    }
}

namespace FritzBot
{
    public class AliasDB
    {
        public List<String> alias;
        public List<String> description;
        public AliasDB()
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
                        if (String.IsNullOrEmpty(value))
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
                if (!String.IsNullOrEmpty(value))
                {
                    alias.Add(thealias);
                    description.Add(value);
                }
            }
        }
    }
}