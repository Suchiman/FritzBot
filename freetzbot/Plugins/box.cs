using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("boxadd")]
    [Help("Dies trägt deine Boxdaten ein, Beispiel: \"!boxadd 7270\", bitte jede Box einzeln angeben.")]
    [ParameterRequired]
    class boxadd : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (var context = new BotContext())
            {
                BoxManager manager = new BoxManager(context.GetUser(theMessage.Nickname), context);
                if (manager.HasBox(theMessage.CommandLine))
                {
                    theMessage.Answer("Wups, danke aber du hast mir deine \"" + theMessage.CommandLine + "\" bereits mitgeteilt ;-).");
                    return;
                }
                BoxEntry box = manager.AddBox(theMessage.CommandLine);
                theMessage.Answer(String.Format("Okay danke, ich werde mir deine \"{0}\"{1} notieren.", box.Text, box.Box != null ? " (" + box.Box.FullName + ")" : ""));
            }
        }
    }
}