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
        public void Run(ircMessage theMessage)
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

        private void Config(ircMessage theMessage)
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
                    theMessage.Answer("Der Schlüssel " + key + " existiert nicht");
                }
            }
            if (theMessage.CommandArgs.Count == 3)
            {
                ConfigHelper.SetValue(key, theMessage.CommandArgs[2]);
                theMessage.Answer("Okay");
            }
        }

        private static void BoxDB(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.Answer("Zu wenig Parameter: add, remove, regex, list");
                return;
            }
            if (theMessage.CommandArgs[1] == "add")
            {
                Match m = Regex.Match(theMessage.CommandLine, "boxdb add \"(?<short>[^\"]*)\" \"(?<full>[^\"]*)\" (?<regex>.*)");
                if (m.Success)
                {
                    Box box = BoxDatabase.GetInstance().GetBoxByShortName(m.Groups["short"].Value);
                    string[] regexes = Regex.Matches(m.Groups["regex"].Value, "\"(?<value>[^\"]*)\"").Cast<Match>().Select(x => x.Groups["value"].Value).ToArray();
                    if (box == null)
                    {
                        box = BoxDatabase.GetInstance().AddBox(m.Groups["short"].Value, m.Groups["full"].Value, regexes);
                        theMessage.Answer("Box erfolgreich hinzugefügt");
                    }
                    else
                    {
                        box.FullName = m.Groups["full"].Value;
                        box.AddRegex(regexes);
                        theMessage.Answer("Box Infos geupdated");
                    }
                }
                else
                {
                    theMessage.Answer("Zu wenig Parameter: boxdb add \"(?<short>[^\"]*)\" \"(?<full>[^\"]*)\" (?<regex>.*)");
                }
            }
            if (theMessage.CommandArgs[1] == "remove")
            {
                Box box = BoxDatabase.GetInstance().GetBoxByShortName(String.Join(" ", theMessage.CommandArgs.Skip(2).ToArray()));
                using (var context = new BotContext())
                {
                    if (box != null)
                    {
                        context.Boxes.Remove(box);
                        theMessage.Answer("Box entfernt");
                    }
                    else
                    {
                        theMessage.Answer("So eine Box habe ich nicht gefunden");
                    }
                }
            }
            if (theMessage.CommandArgs[1] == "regex")
            {
                Match m = Regex.Match(theMessage.CommandLine, "boxdb regex \"(?<short>[^\"]*)\" (?<regex>.*)");
                if (m.Success)
                {
                    Box box = BoxDatabase.GetInstance().GetBoxByShortName(m.Groups["short"].Value);
                    string[] regexes = Regex.Matches(m.Groups["regex"].Value, "\"(?<value>[^\"]*)\"").Cast<Match>().Select(x => x.Groups["value"].Value).ToArray();
                    if (box == null)
                    {
                        theMessage.Answer("Für diesen ShortName konnte ich keine Box ermitteln");
                    }
                    else
                    {
                        box.AddRegex(regexes);
                        theMessage.Answer("Regex(e) erfolgreich hinzugefügt");
                    }
                }
                else
                {
                    theMessage.Answer("Zu wenig Parameter: boxdb regex \"(?<short>[^\"]*)\" (?<regex>.*)");
                }
            }
            if (theMessage.CommandArgs[1] == "list")
            {
                string[] AlleBoxen = BoxDatabase.GetInstance().GetBoxen().Select(x => x.ShortName).OrderByDescending(x => x).ToArray();
                if (AlleBoxen.Length == 0)
                {
                    theMessage.Answer("Oh... ich habe keine Einträge über Boxen");
                    return;
                }
                theMessage.Answer("Ich kenne folgende Boxen: " + String.Join(", ", AlleBoxen));
            }
            if (theMessage.CommandArgs[1] == "info")
            {
                Box box = BoxDatabase.GetInstance().GetBoxByShortName(theMessage.CommandArgs[2]);
                if (box != null)
                {
                    theMessage.Answer(String.Format("ShortName: {0}, FullName: {1}, RegexPatterns: {2}", box.ShortName, box.FullName, String.Join(", ", box.RegexPattern.Select(x => x.Pattern))));
                }
                else
                {
                    theMessage.Answer("So eine Box habe ich nicht gefunden");
                }
            }
        }
    }
}