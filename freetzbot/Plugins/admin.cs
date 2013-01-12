using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
                    IEnumerable<IGrouping<XElement, XElement>> boxen = XMLStorageEngine.GetManager().GetElement("Users").Descendants("box").GroupBy(x => x.Parent);
                    BoxDatabase db = BoxDatabase.GetInstance();
                    foreach (IGrouping<XElement, XElement> boxstorage in boxen)
                    {
                        foreach (XElement box in boxstorage)
                        {
                            IEnumerable<Box> found = db.FindBoxes(box.Value);
                            if (found.Count() == 1)
                            {
                                box.Value = found.First().ShortName;
                            }
                        }
                        boxstorage.Key.RemoveNodes();
                        boxstorage.Key.Add(boxstorage.Distinct(x => x.Value).OrderBy(x => x.Value));
                    }
                    theMessage.Answer("Done");
                    return;
                case "boxdb":
                    BoxDB(theMessage);
                    return;
                case "storage":
                    Storage(theMessage);
                    return;
                case "flush":
                    XMLStorageEngine.GetManager().Save();
                    theMessage.Answer("Erledigt");
                    return;
                default:
                    theMessage.Answer("Das habe ich nicht verstanden: Unterbefehle: box-recheck, boxdb (add, remove, regex, list)");
                    return;
            }
        }

        private static void Storage(ircMessage theMessage)
        {
            Match m = Regex.Match(theMessage.CommandLine, "storage (?<mode>[^ ]*) (?<plugin>[^ ]*) (?<key>[^ ]*)( \"(?<value>[^\"]*)\")?");
            if (m.Success)
            {
                ModulDataStorage storage = null;
                if (m.Groups["mode"].Value == "global")
                {
                    storage = XMLStorageEngine.GetManager().GetGlobalSettingsStorage(m.Groups["plugin"].Value);
                }
                else if (UserManager.GetInstance().Exists(m.Groups["mode"].Value))
                {
                    storage = UserManager.GetInstance()[m.Groups["mode"].Value].GetModulUserStorage(m.Groups["plugin"].Value);
                }
                else
                {
                    theMessage.Answer("Falscher Modus: global oder username");
                    return;
                }
                if (m.Groups["value"].Success)
                {
                    storage.SetVariable(m.Groups["key"].Value, m.Groups["value"].Value);
                    theMessage.Answer("Wert erfolgreich gesetzt");
                }
                else
                {
                    theMessage.Answer(storage.GetVariable(m.Groups["key"].Value, "Eintrag nicht vorhanden"));
                }
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
                if (box != null)
                {
                    box.Remove();
                    theMessage.Answer("Box entfernt");
                }
                else
                {
                    theMessage.Answer("So eine Box habe ich nicht gefunden");
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