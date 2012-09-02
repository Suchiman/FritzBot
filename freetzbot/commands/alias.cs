using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("alias", "a")]
    [Module.Help("Legt einen Alias für einen Begriff fest, Beispiele, \"!a add freetz Eine Modifikation für...\", \"!a edit freetz DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen: $1 ... $99 für Leerzeichen getrennte Argumente und $X für alle Argumente. encode(url) für URL Konforme Zeichencodierung")]
    [Module.ParameterRequired]
    class alias : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            AliasCommand(theMessage);
        }

        public static String GetAlias(ircMessage theMessage)
        {
            List<String> SplitString = new List<String>(theMessage.Message.Remove(0, 1).Split(' '));
            if (SplitString[0] == "alias" || SplitString[0] == "a")
            {
                SplitString.RemoveAt(0);
            }
            String thealias = theMessage.TheUsers.AllAliases()[SplitString[0]];
            if (!String.IsNullOrEmpty(thealias))
            {
                for (int i = 0; thealias.Contains("$"); i++)
                {
                    if (SplitString.Count > 1)
                    {
                        String commandline = String.Join(" ", SplitString.ToArray(), 1, SplitString.Count - 1);
                        thealias = thealias.Replace("$x", commandline).Replace("$X", commandline);
                    }
                    else
                    {
                        thealias = thealias.Replace("$x", "").Replace("$X", "");
                    }
                    while (true)
                    {
                        int index = thealias.IndexOf("$" + (i + 1));
                        if (index == -1) break;
                        thealias = thealias.Remove(index, 2);
                        if (SplitString.Count - 1 > i)
                        {
                            thealias = thealias.Insert(index, SplitString[i + 1]);
                        }
                        else
                        {
                            thealias = thealias.Insert(index, "");
                        }
                    }
                }
                while (thealias.Contains("encode("))
                {
                    int start = thealias.LastIndexOf("encode(");
                    int end = thealias.Remove(0, start).IndexOf(')') + 1 + start;

                    String second = thealias.Substring(start + 7, end - (start + 8));
                    second = toolbox.UrlEncode(second);
                    second = second.Replace("%23", "#").Replace("%3a", ":").Replace("%2f", "/").Replace("%3f", "?");

                    thealias = thealias.Substring(0, start) + second + thealias.Substring(end);
                }
                return thealias;
            }
            return "";
        }

        public static Boolean AliasCommand(ircMessage theMessage)
        {
            switch (theMessage.CommandArgs[0].ToLower())
            {
                case "add":
                    if (String.IsNullOrEmpty(Program.TheUsers.AllAliases()[theMessage.CommandArgs[1]]))
                    {
                        theMessage.TheUser.alias[theMessage.CommandArgs[1]] = String.Join(" ", theMessage.CommandArgs.ToArray(), 2, theMessage.CommandArgs.Count - 2);
                        theMessage.Answer("Der Alias wurde erfolgreich hinzugefügt");
                        return true;
                    }
                    theMessage.Answer("Diesen Alias gibt es bereits");
                    return false;
                case "edit":
                    theMessage.TheUser.alias[theMessage.CommandArgs[1]] = theMessage.CommandLine.Substring(theMessage.CommandLine.IndexOf(' '));
                    theMessage.Answer("Der Alias wurde erfolgreich bearbeitet");
                    return true;
                case "remove":
                    if (!String.IsNullOrEmpty(theMessage.TheUser.alias[theMessage.CommandArgs[1]]))
                    {
                        theMessage.TheUser.alias[theMessage.CommandArgs[1]] = "";
                        theMessage.Answer("Alias wurde gelöscht");
                    }
                    else if (toolbox.IsOp(theMessage.Nick))
                    {
                        foreach (User oneuser in Program.TheUsers)
                        {
                            if (!String.IsNullOrEmpty(oneuser.alias[theMessage.CommandArgs[1]]))
                            {
                                oneuser.alias[theMessage.CommandArgs[1]] = "";
                                theMessage.Answer("Alias wurde gelöscht");
                                return true;
                            }
                        }
                        theMessage.Answer("Alias wurde nicht gefunden");
                    }
                    else
                    {
                        theMessage.Answer("Du scheinst keinen solchen Alias definiert zu haben");
                    }
                    return true;
                default:
                    String theAlias = GetAlias(theMessage);
                    if (!String.IsNullOrEmpty(theAlias))
                    {
                        theMessage.Answer(theAlias);
                        return true;
                    }
                    theMessage.Answer("Wups, diesen Alias gibt es nicht");
                    return false;
            }
        }
    }
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