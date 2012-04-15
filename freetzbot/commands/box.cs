using System;

namespace FritzBot.commands
{
    class box : ICommand
    {
        public String[] Name { get { return new String[] { "box" }; } }
        public String HelpText { get { return "Dies trägt deine Boxdaten ein, Beispiel: \"!box 7270\", bitte jede Box einzeln angeben."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (FritzBot.Program.TheUsers[sender].AddBox(message))
            {
                connection.Sendmsg("Okay danke, ich werde mir deine \"" + message + "\" notieren.", receiver);
                connection.Sendmsg("Neue Box wurde registriert: User: " + sender + ", Box: " + message, "hippie2000");
            }
            else
            {
                connection.Sendmsg("Wups, danke aber du hast mir deine \"" + message + "\" bereits mitgeteilt ;-).", receiver);
            }
        }
    }
}