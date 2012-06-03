using System;

namespace FritzBot.commands
{
    class box : ICommand
    {
        public String[] Name { get { return new String[] { "boxadd" }; } }
        public String HelpText { get { return "Dies trägt deine Boxdaten ein, Beispiel: \"!boxadd 7270\", bitte jede Box einzeln angeben."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.getUser.AddBox(theMessage.CommandLine))
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