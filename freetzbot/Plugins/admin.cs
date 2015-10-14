using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Name("admin")]
    [Help("Administrativer Befehl zur Verwaltung einiger Funktionen: box-recheck, boxdb (add, remove, regex, list)")]
    [Authorize]
    [ParameterRequired]
    class admin : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            switch (theMessage.CommandArgs[0])
            {
                case "box-recheck":
                    using (var context = new BotContext())
                    {
                        throw new NotImplementedException();
                        //db.Query<BoxEntry>().ForEach(x => { x.ReAssociateBoxes(); db.SaveOrUpdate(x); });
                    }
                    theMessage.Answer("Done");
                    return;
                case "boxdb":
                    BoxDB(theMessage);
                    return;
                case "config":
                    Config(theMessage);
                    return;
                default:
                    theMessage.Answer("Das habe ich nicht verstanden: Unterbefehle: box-recheck, boxdb (add, remove, regex, list), config");
                    return;
            }
        }

        private void Config(IrcMessage theMessage)
        {
            if (!theMessage.CommandArgs.Count.In(2, 3))
            {
                theMessage.Answer("Syntax: !admin config <key> <value>");
                return;
            }
            string key = theMessage.CommandArgs[1];
            if (theMessage.CommandArgs.Count == 2)
            {
                if (ConfigHelper.KeyExists(key))
                {
                    theMessage.Answer(ConfigHelper.GetString(key));
                }
                else
                {
                    theMessage.Answer($"Der Schlüssel {key} existiert nicht");
                }
            }
            else if (theMessage.CommandArgs.Count == 3)
            {
                if (key == "remove")
                {
                    ConfigHelper.Remove(theMessage.CommandArgs[2]);
                    theMessage.Answer("Wert gelöscht");
                }
                else
                {
                    ConfigHelper.SetValue(key, theMessage.CommandArgs[2]);
                    theMessage.Answer("Wert geändert");
                }
            }
        }

        private static void BoxDB(IrcMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.Answer("Zu wenig Parameter: add, remove, regex, list");
                return;
            }

            switch (theMessage.CommandArgs[1])
            {
                case "add":
                    BoxDbAdd(theMessage);
                    return;
                case "remove":
                    BoxDbRemove(theMessage);
                    return;
                case "regex":
                    BoxDbRegex(theMessage);
                    return;
                case "list":
                    BoxDbList(theMessage);
                    return;
                case "info":
                    BoxDbInfo(theMessage);
                    return;
            }
        }

        private static void BoxDbAdd(IrcMessage theMessage)
        {
            Match match = Regex.Match(theMessage.CommandLine, "boxdb add \"(?<short>[^\"]*)\" \"(?<full>[^\"]*)\" (?<regex>.*)");
            if (!match.Success)
            {
                theMessage.Answer("Zu wenig Parameter: boxdb add \"(?<short>[^\"]*)\" \"(?<full>[^\"]*)\" (?<regex>.*)");
                return;
            }

            Box box = BoxDatabase.GetBoxByShortName(match.Groups["short"].Value);
            string[] regexes = Regex.Matches(match.Groups["regex"].Value, "\"(?<value>[^\"]*)\"").Cast<Match>().Select(x => x.Groups["value"].Value).ToArray();
            if (box == null)
            {
                box = BoxDatabase.AddBox(match.Groups["short"].Value, match.Groups["full"].Value, regexes);
                theMessage.Answer("Box erfolgreich hinzugefügt");
            }
            else
            {
                box.FullName = match.Groups["full"].Value;
                box.AddRegex(regexes);
                theMessage.Answer("Box Infos geupdated");
            }
        }

        private static void BoxDbRemove(IrcMessage theMessage)
        {
            Box box = BoxDatabase.GetBoxByShortName(theMessage.CommandArgs.Skip(2).Join(" "));
            if (box == null)
            {
                theMessage.Answer("So eine Box habe ich nicht gefunden");
                return;
            }

            using (var context = new BotContext())
            {
                context.Boxes.Remove(box);
                theMessage.Answer("Box entfernt");
            }
        }

        private static void BoxDbRegex(IrcMessage theMessage)
        {
            Match match = Regex.Match(theMessage.CommandLine, "boxdb regex \"(?<short>[^\"]*)\" (?<regex>.*)");
            if (!match.Success)
            {
                theMessage.Answer("Zu wenig Parameter: boxdb regex \"(?<short>[^\"]*)\" (?<regex>.*)");
                return;
            }

            Box box = BoxDatabase.GetBoxByShortName(match.Groups["short"].Value);
            if (box == null)
            {
                theMessage.Answer("Für diesen ShortName konnte ich keine Box ermitteln");
                return;
            }

            string[] regexes = Regex.Matches(match.Groups["regex"].Value, "\"(?<value>[^\"]*)\"").Cast<Match>().Select(x => x.Groups["value"].Value).ToArray();
            box.AddRegex(regexes);
            theMessage.Answer("Regex(e) erfolgreich hinzugefügt");
        }

        private static void BoxDbList(IrcMessage theMessage)
        {
            string[] allBoxes = BoxDatabase.Boxen.Select(x => x.ShortName).OrderByDescending(x => x).ToArray();
            if (allBoxes.Length == 0)
            {
                theMessage.Answer("Oh... ich habe keine Einträge über Boxen");
                return;
            }

            theMessage.Answer($"Ich kenne folgende Boxen: {allBoxes.Join(", ")}");
        }

        private static void BoxDbInfo(IrcMessage theMessage)
        {
            Box box = BoxDatabase.GetBoxByShortName(theMessage.CommandArgs[2]);
            if (box == null)
            {
                theMessage.Answer("So eine Box habe ich nicht gefunden");
                return;
            }

            theMessage.Answer($"ShortName: {box.ShortName}, FullName: {box.FullName}, RegexPatterns: {box.RegexPattern.Select(x => x.Pattern).Join(", ")}");
        }
    }
}