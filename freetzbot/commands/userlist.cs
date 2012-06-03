using System;

namespace FritzBot.commands
{
    class userlist : ICommand
    {
        public String[] Name { get { return new String[] { "userlist" }; } }
        public String HelpText { get { return "Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            String output = "";
            foreach (User oneuser in theMessage.theUsers)
            {
                if (oneuser.boxes.Count > 0)
                {
                    output += ", " + oneuser.names[0];
                }
            }
            output = output.Remove(0, 2);
            if (!String.IsNullOrEmpty(output))
            {
                theMessage.SendPrivateMessage("Diese Benutzer haben bei mir mindestens eine Box registriert: " + output);
            }
            else
            {
                theMessage.Answer("Ich fürchte, mir ist ein Fehler unterlaufen. Ich kann keine registrierten Benutzer feststellen.");
            }
        }
    }
}