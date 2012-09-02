using System;

namespace FritzBot.commands
{
    [Module.Name("boxinfo", "box")]
    [Module.Help("Zeigt die Box/en des angegebenen Benutzers an.")]
    class boxinfo : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            String output = "";
            String UserToUse = theMessage.CommandLine;
            if (!theMessage.HasArgs)
            {
                UserToUse = theMessage.Nick;
            }
            if (theMessage.TheUsers.Exists(UserToUse))
            {
                foreach (String box in theMessage.TheUsers[UserToUse].boxes)
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
                if (!theMessage.HasArgs)
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
            if (!theMessage.HasArgs)
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