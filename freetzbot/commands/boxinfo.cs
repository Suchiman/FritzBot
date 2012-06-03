using System;

namespace FritzBot.commands
{
    class boxinfo : ICommand
    {
        public String[] Name { get { return new String[] { "boxinfo", "box" }; } }
        public String HelpText { get { return "Zeigt die Box/en des angegebenen Benutzers an."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            String output = "";
            String UserToUse = theMessage.CommandLine;
            if (!theMessage.hasArgs)
            {
                UserToUse = theMessage.Nick;
            }
            if (theMessage.theUsers.Exists(UserToUse))
            {
                foreach (String box in theMessage.theUsers[UserToUse].boxes)
                {
                    output += ", " + box;
                }
            }
            else
            {
                theMessage.Answer("Der Benutzer ist mir nicht bekannt");
                return;
            }
            if (String.IsNullOrEmpty(output))
            {
                if (!theMessage.hasArgs)
                {
                    theMessage.Answer("Für dich existieren keine Einträge");
                }
                else
                {
                    theMessage.Answer("Meine Datenbank enthält keinen Eintrag über diesen Benutzer");
                }
                return;
            }
            output = output.Remove(0, 2);
            if (!theMessage.hasArgs)
            {
                theMessage.Answer("Entsprechend der Datenbank wurden die folgenden Boxen registriert: " + output);
            }
            else
            {
                theMessage.Answer(UserToUse + " besitzt laut Datenbank " + output);
            }
        }
    }
}