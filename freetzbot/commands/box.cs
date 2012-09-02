using System;

namespace FritzBot.commands
{
    [Module.Name("boxadd")]
    [Module.Help("Dies trägt deine Boxdaten ein, Beispiel: \"!boxadd 7270\", bitte jede Box einzeln angeben.")]
    [Module.ParameterRequired]
    class box : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.TheUser.AddBox(theMessage.CommandLine))
            {
                theMessage.Answer("Okay danke, ich werde mir deine \"" + theMessage.CommandLine + "\" notieren.");
                theMessage.Connection.Sendmsg("Neue Box wurde registriert: User: " + theMessage.Nick + ", Box: " + theMessage.CommandLine, "hippie2000");
            }
            else
            {
                theMessage.Answer("Wups, danke aber du hast mir deine \"" + theMessage.CommandLine + "\" bereits mitgeteilt ;-).");
            }
        }
    }
}