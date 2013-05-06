using FritzBot.Core;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FritzBot.Plugins
{
    [Module.Name("alias", "a")]
    [Module.Help("Legt einen Alias für einen Begriff fest, Beispiele, \"!a add freetz Eine Modifikation für...\", \"!a edit freetz DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen: $1 ... $99 für Leerzeichen getrennte Argumente und $X für alle Argumente. encode(url) für URL Konforme Zeichencodierung")]
    [Module.ParameterRequired]
    class alias : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            Server.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            Server.OnPostProcessingMessage -= Server_OnPostProcessingMessage;
        }

        void Server_OnPostProcessingMessage(object sender, ircMessage theMessage)
        {
            if (theMessage.IsCommand && !theMessage.Answered && !theMessage.ProcessedByCommand)
            {
                string alias = GetAlias(theMessage);
                if (!String.IsNullOrEmpty(alias))
                {
                    theMessage.Answer(alias);
                }
            }
        }

        public void Run(ircMessage theMessage)
        {
            AliasCommand(theMessage);
        }

        public static string GetAlias(ircMessage theMessage)
        {
            List<string> SplitString = new List<string>(theMessage.Message.TrimStart('!', ' ').Split(' '));
            if (SplitString[0] == "alias" || SplitString[0] == "a")
            {
                SplitString.RemoveAt(0);
            }
            string thealias;
            using (DBProvider db = new DBProvider())
            {
                AliasEntry entry = db.Query<AliasEntry>(x => x.Key == SplitString[0]).FirstOrDefault();
                if (entry != null && !String.IsNullOrEmpty(entry.Text))
                {
                    thealias = entry.Text;
                }
                else
                {
                    return String.Empty;
                }
            }
            for (int i = 0; thealias.Contains("$"); i++)
            {
                if (SplitString.Count > 1)
                {
                    string commandline = String.Join(" ", SplitString.Skip(1));
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
            if (thealias[0] == '!')
            {
                StackTrace trace = new StackTrace();
                StackFrame[] frames = trace.GetFrames();
                int recursion = frames.Count(x => x.GetMethod() == frames[0].GetMethod());
                if (recursion > 4)
                {
                    theMessage.Answer("Einen moment mal... das scheint rekursiv zu sein. Ich beende das mal");
                    return String.Empty;
                }
                IrcMessageData data = new IrcMessageData(theMessage.Data.Irc, theMessage.Data.From, theMessage.Data.Nick, theMessage.Data.Ident, theMessage.Data.Host, theMessage.Data.Channel, thealias, null, theMessage.Data.Type, theMessage.Data.ReplyCode);
                ircMessage fake = new ircMessage(data, theMessage.Server, theMessage.TheUser);
                Program.HandleCommand(fake);
                return String.Empty;
            }
            return thealias;
        }

        public static void AliasCommand(ircMessage theMessage)
        {
            using (DBProvider db = new DBProvider())
            {
                switch (theMessage.CommandArgs[0].ToLower())
                {
                    case "add":
                        if (theMessage.CommandArgs.Count < 3)
                        {
                            theMessage.Answer("Unzureichend viele Argumente: add <key> <text ...>");
                            return;
                        }
                        AliasEntry add = db.Query<AliasEntry>(x => x.Key == theMessage.CommandArgs[1]).FirstOrDefault();
                        if (add != null)
                        {
                            theMessage.Answer("Diesen Alias gibt es bereits");
                            return;
                        }
                        add = new AliasEntry();
                        add.Key = theMessage.CommandArgs[1];
                        add.Text = String.Join(" ", theMessage.CommandArgs.Skip(2));
                        add.Creator = theMessage.TheUser;
                        add.Created = DateTime.Now;
                        db.SaveOrUpdate(add);
                        theMessage.Answer("Der Alias wurde erfolgreich hinzugefügt");
                        return;
                    case "edit":
                        if (theMessage.CommandArgs.Count < 3)
                        {
                            theMessage.Answer("Unzureichend viele Argumente: edit <key> <neuer text>");
                            return;
                        }
                        AliasEntry edit = db.Query<AliasEntry>(x => x.Key == theMessage.CommandArgs[1]).FirstOrDefault();
                        if (edit != null)
                        {
                            edit.Text = String.Join(" ", theMessage.CommandArgs.Skip(2));
                            edit.Updater = theMessage.TheUser;
                            edit.Updated = DateTime.Now;
                            db.SaveOrUpdate(edit);
                            theMessage.Answer("Der Alias wurde erfolgreich bearbeitet");
                            return;
                        }
                        theMessage.Answer("So ein Alias wurde noch nicht definiert");
                        return;
                    case "remove":
                        if (theMessage.CommandArgs.Count < 2)
                        {
                            theMessage.Answer("Unzureichend viele Argumente: remove <key>");
                            return;
                        }
                        AliasEntry remove = db.Query<AliasEntry>(x => x.Key == theMessage.CommandArgs[1]).FirstOrDefault();
                        if (remove != null)
                        {
                            db.Remove(remove);
                            theMessage.Answer("Alias wurde gelöscht");
                            return;
                        }
                        theMessage.Answer("Diesen Alias gibt es nicht");
                        return;
                    case "info":
                    case "details":
                        if (theMessage.CommandArgs.Count < 2)
                        {
                            theMessage.Answer("Unzureichend viele Argumente: info <key>");
                            return;
                        }
                        AliasEntry info = db.Query<AliasEntry>(x => x.Key == theMessage.CommandArgs[1]).FirstOrDefault();
                        if (info != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            if (info.Creator != null)
                            {
                                sb.Append("Erstellt von " + info.Creator.LastUsedName);
                                if (info.Created > DateTime.MinValue)
                                {
                                    sb.Append(" am " + info.Created.ToShortDateString() + " um " + info.Created.ToShortTimeString() + ". ");
                                }
                                else
                                {
                                    sb.Append(". ");
                                }
                            }
                            if (info.Updater != null)
                            {
                                sb.Append("Geändert von " + info.Updater.LastUsedName);
                                if (info.Updated > DateTime.MinValue)
                                {
                                    sb.Append(" am " + info.Updated.ToShortDateString() + " um " + info.Updated.ToShortTimeString() + ". ");
                                }
                                else
                                {
                                    sb.Append(". ");
                                }
                            }
                            sb.Append("Definition: " + info.Text);
                            theMessage.Answer(sb.ToString());
                            return;
                        }
                        else
                        {
                            theMessage.Answer("Wups, diesen Alias kenne ich nicht");
                            return;
                        }
                    case "find":
                    case "search":
                        {
                            if (theMessage.CommandArgs.Count < 2)
                            {
                                theMessage.Answer("Wonach soll ich denn Suchen wenn du nichts angibst ?: search <key>");
                                return;
                            }
                            SODAQuery<AliasEntry> query = db.SODAQuery<AliasEntry>();
                            query.Member(x => x.Key).Constrain(theMessage.CommandArgs[1]).Like();
                            List<AliasEntry> search = query.Execute().ToList();
                            if (search.Count == 0)
                            {
                                theMessage.Answer("Nichts gefunden :(");
                                return;
                            }
                            theMessage.Answer("Mögliche Aliase: " + String.Join(", ", search.Select(x => x.Key)));
                            return;
                        }
                    default:
                        string theAlias = GetAlias(theMessage);
                        if (!String.IsNullOrEmpty(theAlias))
                        {
                            theMessage.Answer(theAlias);
                            return;
                        }
                        theMessage.Answer("Wups, diesen Alias gibt es nicht");
                        return;
                }
            }
        }
    }
}