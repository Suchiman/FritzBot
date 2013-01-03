using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxadd")]
    [Module.Help("Dies trägt deine Boxdaten ein, Beispiel: \"!boxadd 7270\", bitte jede Box einzeln angeben.")]
    [Module.ParameterRequired]
    [Module.Subscribeable]
    class box : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            List<Box> MatchingBoxes = BoxDatabase.GetInstance().FindBoxes(theMessage.CommandLine).ToList();
            String BoxToAdd = "";
            if (MatchingBoxes.Count == 0)
            {
                BoxToAdd = theMessage.CommandLine;
            }
            else if (MatchingBoxes.Count == 1)
            {
                BoxToAdd = MatchingBoxes.First().ShortName;
            }
            else if (MatchingBoxes.Count > 1)
            {
                theMessage.Answer("Multiple Treffer, bitte entscheide dich für eine: " + String.Join(", ", MatchingBoxes.Select(x => x.ShortName).ToArray()));
                return;
            }
            ModulDataStorage mds = theMessage.TheUser.GetModulUserStorage("box");
            XElement box = mds.Storage.Elements("box").FirstOrDefault(x => x.Value == BoxToAdd);
            if (box == null)
            {
                mds.Storage.Add(new XElement("box", BoxToAdd));
                theMessage.Answer("Okay danke, ich werde mir deine \"" + BoxToAdd + "\" notieren.");
                NotifySubscribers(String.Format("Benutzer {0} hat eine neue Box registriert: {1}", theMessage.Nickname, BoxToAdd));
            }
            else
            {
                theMessage.Answer("Wups, danke aber du hast mir deine \"" + BoxToAdd + "\" bereits mitgeteilt ;-).");
            }
        }
    }
}