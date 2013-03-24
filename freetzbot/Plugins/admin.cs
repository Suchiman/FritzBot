using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Module.Name("admin")]
    [Module.Help("Administrativer Befehl zur Verwaltung einiger Funktionen: box-recheck, boxdb (add, remove, regex, list)")]
    [Module.Authorize]
    [Module.ParameterRequired]
    class admin : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            switch (theMessage.CommandArgs[0])
            {
                case "box-recheck":
                    using (DBProvider db = new DBProvider())
                    {
                        db.Query<BoxEntry>().ForEach(x => { x.ReAssociateBoxes(); db.SaveOrUpdate(x); });
                    }
                    theMessage.Answer("Done");
                    return;
                case "boxdb":
                    BoxDB(theMessage);
                    return;
                default:
                    theMessage.Answer("Das habe ich nicht verstanden: Unterbefehle: box-recheck, boxdb (add, remove, regex, list)");
                    return;
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
                using (DBProvider db = new DBProvider())
                {
                    if (box != null)
                    {
                        db.Remove(box);
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
                    String[] regexes = Regex.Matches(m.Groups["regex"].Value, "\"(?<value>[^\"]*)\"").Cast<Match>().Select(x => x.Groups["value"].Value).ToArray();
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
                    theMessage.Answer(String.Format("ShortName: {0}, FullName: {1}, RegexPatterns: {2}", box.ShortName, box.FullName, String.Join(", ", box.RegexPattern.ToArray())));
                }
                else
                {
                    theMessage.Answer("So eine Box habe ich nicht gefunden");
                }
            }
        }
    }
}