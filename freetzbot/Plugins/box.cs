using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("boxadd")]
    [Help("Dies trägt deine Boxdaten ein, Beispiel: \"!boxadd 7270\", bitte jede Box einzeln angeben.")]
    [ParameterRequired]
    class boxadd : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                BoxManager manager = new BoxManager(context.GetUser(theMessage.Nickname), context);
                if (manager.HasBox(theMessage.CommandLine))
                {
                    theMessage.Answer($"Wups, danke aber du hast mir deine \"{theMessage.CommandLine}\" bereits mitgeteilt ;-).");
                    return;
                }
                BoxEntry box = manager.AddBox(theMessage.CommandLine);
                theMessage.Answer($"Okay danke, ich werde mir deine \"{box.Text}\"{(box.Box != null ? " (" + box.Box.FullName + ")" : "")} notieren.");
            }
        }
    }
}