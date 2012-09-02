using System;

namespace FritzBot.commands
{
    [Module.Name("userlist")]
    [Module.Help("Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben.")]
    [Module.ParameterRequired(false)]
    class userlist : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            String output = "";
            foreach (User oneuser in theMessage.TheUsers)
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