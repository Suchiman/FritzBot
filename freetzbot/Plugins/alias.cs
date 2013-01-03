using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("alias", "a")]
    [Module.Help("Legt einen Alias für einen Begriff fest, Beispiele, \"!a add freetz Eine Modifikation für...\", \"!a edit freetz DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen: $1 ... $99 für Leerzeichen getrennte Argumente und $X für alle Argumente. encode(url) für URL Konforme Zeichencodierung")]
    [Module.ParameterRequired]
    class alias : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            AliasCommand(theMessage);
        }

        public static string GetAlias(ircMessage theMessage)
        {
            List<string> SplitString = new List<string>(theMessage.Message.Remove(0, 1).Split(' '));
            if (SplitString[0] == "alias" || SplitString[0] == "a")
            {
                SplitString.RemoveAt(0);
            }
            string thealias = UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage("alias").Storage.Elements("alias")).Where(x => x.Element("name").Value == SplitString[0]).Select(x => x.Element("beschreibung").Value).FirstOrDefault();
            if (!String.IsNullOrEmpty(thealias))
            {
                for (int i = 0; thealias.Contains("$"); i++)
                {
                    if (SplitString.Count > 1)
                    {
                        string commandline = String.Join(" ", SplitString.ToArray(), 1, SplitString.Count - 1);
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

                    string second = thealias.Substring(start + 7, end - (start + 8));
                    second = toolbox.UrlEncode(second);
                    second = second.Replace("%23", "#").Replace("%3a", ":").Replace("%2f", "/").Replace("%3f", "?");

                    thealias = thealias.Substring(0, start) + second + thealias.Substring(end);
                }
                return thealias;
            }
            return "";
        }

        public static bool AliasCommand(ircMessage theMessage)
        {
            switch (theMessage.CommandArgs[0].ToLower())
            {
                case "add":
                    if (!UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage("alias").Storage.Elements("alias")).Any(x => x.Element("name").Value == theMessage.CommandArgs[1]))
                    {
                        theMessage.TheUser.GetModulUserStorage("alias").Storage.Add(new XElement("alias", new XElement("name", theMessage.CommandArgs[1]), new XElement("beschreibung", String.Join(" ", theMessage.CommandArgs.ToArray(), 2, theMessage.CommandArgs.Count - 2))));
                        theMessage.Answer("Der Alias wurde erfolgreich hinzugefügt");
                        return true;
                    }
                    theMessage.Answer("Diesen Alias gibt es bereits");
                    return false;
                case "edit":
                    XElement alias = theMessage.TheUser.GetModulUserStorage("alias").Storage.Elements("alias").FirstOrDefault(x => x.Element("name").Value == theMessage.CommandArgs[1]);
                    if (alias != null)
                    {
                        alias.Element("beschreibung").Value = String.Join(" ", theMessage.CommandArgs.Skip(2).ToArray());
                        theMessage.Answer("Der Alias wurde erfolgreich bearbeitet");
                    }
                    else
                    {
                        theMessage.Answer("Du scheinst keinen solchen Alias definiert zu haben");
                    }
                    return true;
                case "remove":
                    XElement ralias = theMessage.TheUser.GetModulUserStorage("alias").Storage.Elements("alias").FirstOrDefault(x => x.Element("name").Value == theMessage.CommandArgs[1]);
                    if (ralias != null)
                    {
                        ralias.Remove();
                        theMessage.Answer("Alias wurde gelöscht");
                    }
                    else if (toolbox.IsOp(theMessage.Nickname))
                    {
                        ralias = UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage("alias").Storage.Elements("alias")).FirstOrDefault(x => x.Element("name").Value == theMessage.CommandArgs[1]);
                        if (ralias != null)
                        {
                            ralias.Remove();
                            theMessage.Answer("Alias wurde gelöscht");
                            return true;
                        }
                        theMessage.Answer("Alias wurde nicht gefunden");
                    }
                    else
                    {
                        theMessage.Answer("Du scheinst keinen solchen Alias definiert zu haben");
                    }
                    return true;
                default:
                    string theAlias = GetAlias(theMessage);
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
}