using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FritzBot.Plugins
{
    [Name("alias", "a")]
    [Help("Legt einen Alias für einen Begriff fest, Beispiele, \"!a add freetz Eine Modifikation für...\", \"!a edit freetz DIE Modifikation\", \"!a remove freetz\", \"!a freetz\", Variablen: $1 ... $99 für Leerzeichen getrennte Argumente und $X für alle Argumente. encode(url) für URL Konforme Zeichencodierung")]
    [ParameterRequired]
    class alias : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnPostProcessingMessage -= Server_OnPostProcessingMessage;
        }

        void Server_OnPostProcessingMessage(object sender, IrcMessage theMessage)
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

        public void Run(IrcMessage theMessage)
        {
            AliasCommand(theMessage);
        }

        public static string GetAlias(IrcMessage theMessage)
        {
            List<string> SplitString = new List<string>(theMessage.Message.TrimStart('!', ' ').Split(' '));
            if (SplitString[0] == "alias" || SplitString[0] == "a")
            {
                SplitString.RemoveAt(0);
            }
            string thealias;
            using (var context = new BotContext())
            {
                string key = SplitString[0];
                AliasEntry entry = context.AliasEntries.FirstOrDefault(x => x.Key == key);
                if (entry != null && !String.IsNullOrEmpty(entry.Text))
                {
                    thealias = entry.Text;
                }
                else
                {
                    return String.Empty;
                }
            }
            for (int i = 0; thealias.Contains("$") && i < 99; i++)
            {
                if (SplitString.Count > 1)
                {
                    string commandline = SplitString.Skip(1).Join(" ");
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
                second = Toolbox.UrlEncode(second);
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
                IrcMessageData data = new IrcMessageData(theMessage.Data.Irc, theMessage.Data.From, theMessage.Data.Nick, theMessage.Data.Ident, theMessage.Data.Host, theMessage.Data.Channel, thealias, thealias, theMessage.Data.Type, theMessage.Data.ReplyCode);
                IrcMessage fake = new IrcMessage(data, theMessage.ServerConnetion);
                Program.HandleCommand(fake);
                theMessage.Hidden = fake.Hidden;
                theMessage.ProcessedByCommand = fake.ProcessedByCommand;
                for (int i = 0; i < fake.UnloggedMessages.Count; i++)
                {
                    theMessage.UnloggedMessages.Enqueue(fake.UnloggedMessages.Dequeue());
                }
                return String.Empty;
            }
            return thealias;
        }

        public static void AliasCommand(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                string key = null;
                if (theMessage.CommandArgs.Count > 1)
                {
                    key = theMessage.CommandArgs[1];
                }
                switch (theMessage.CommandArgs[0].ToLower())
                {
                    case "add":
                        if (theMessage.CommandArgs.Count < 3)
                        {
                            theMessage.Answer("Unzureichend viele Argumente: add <key> <text ...>");
                            return;
                        }
                        if (context.AliasEntries.Any(x => x.Key == key))
                        {
                            theMessage.Answer("Diesen Alias gibt es bereits");
                            return;
                        }
                        var add = new AliasEntry();
                        add.Key = theMessage.CommandArgs[1];
                        add.Text = theMessage.CommandArgs.Skip(2).Join(" ");
                        add.Creator = context.GetUser(theMessage.Nickname);
                        add.Created = DateTime.Now;
                        context.AliasEntries.Add(add);
                        context.SaveChanges();
                        theMessage.Answer("Der Alias wurde erfolgreich hinzugefügt");
                        return;
                    case "edit":
                        if (theMessage.CommandArgs.Count < 3)
                        {
                            theMessage.Answer("Unzureichend viele Argumente: edit <key> <neuer text>");
                            return;
                        }
                        AliasEntry edit = context.AliasEntries.FirstOrDefault(x => x.Key == key);
                        if (edit != null)
                        {
                            edit.Text = theMessage.CommandArgs.Skip(2).Join(" ");
                            edit.Updater = context.GetUser(theMessage.Nickname);
                            edit.Updated = DateTime.Now;
                            context.SaveChanges();
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
                        AliasEntry remove = context.AliasEntries.FirstOrDefault(x => x.Key == key);
                        if (remove != null)
                        {
                            context.AliasEntries.Remove(remove);
                            context.SaveChanges();
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
                        AliasEntry info = context.AliasEntries.Include(x => x.Creator.LastUsedName).Include(x => x.Updater.LastUsedName).FirstOrDefault(x => x.Key == key);
                        if (info != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            if (info.Creator != null)
                            {
                                sb.Append("Erstellt von " + info.Creator.LastUsedName);
                                if (info.Created.HasValue)
                                {
                                    sb.Append(" am " + info.Created.Value.ToShortDateString() + " um " + info.Created.Value.ToShortTimeString() + ". ");
                                }
                                else
                                {
                                    sb.Append(". ");
                                }
                            }
                            if (info.Updater != null)
                            {
                                sb.Append("Geändert von " + info.Updater.LastUsedName);
                                if (info.Updated.HasValue)
                                {
                                    sb.Append(" am " + info.Updated.Value.ToShortDateString() + " um " + info.Updated.Value.ToShortTimeString() + ". ");
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
                        theMessage.Answer("Wups, diesen Alias kenne ich nicht");
                        return;
                    case "find":
                    case "search":
                        {
                            if (theMessage.CommandArgs.Count < 2)
                            {
                                theMessage.Answer("Wonach soll ich denn Suchen wenn du nichts angibst ?: search <key>");
                                return;
                            }
                            key = key.ToLower();
                            List<AliasEntry> search = context.AliasEntries.Where(x => x.Key.ToLower().Contains(key) || x.Text.ToLower().Contains(key)).ToList();
                            if (search.Count == 0)
                            {
                                theMessage.Answer("Nichts gefunden :(");
                                return;
                            }
                            if (search.Count == 1)
                            {
                                theMessage.Answer(search[0].Key + ": " + search[0].Text);
                            }
                            else
                            {
                                theMessage.Answer("Mögliche Aliase: " + search.Select(x => x.Key).Join(", "));
                            }
                            return;
                        }
                    default:
                        string theAlias = GetAlias(theMessage);
                        if (!String.IsNullOrEmpty(theAlias))
                        {
                            theMessage.Answer(theAlias);
                            return;
                        }
                        if (theMessage.UnloggedMessages.Count == 0)
                        {
                            theMessage.Answer("Wups, diesen Alias gibt es nicht");
                        }
                        return;
                }
            }
        }
    }
}