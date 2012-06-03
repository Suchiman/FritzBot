using System;

namespace FritzBot.commands
{
    class auth : ICommand
    {
        public String[] Name { get { return new String[] { "auth" }; } }
        public String HelpText { get { return "Authentifiziert dich wenn du ein Passwort festgelegt hast. z.b. !auth passwort"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            if (!theMessage.isPrivate)
            {
                theMessage.Answer("Ohje das solltest du besser im Query tuen");
                return;
            }
            if (theMessage.getUser.CheckPassword(theMessage.CommandLine))
            {
                theMessage.getUser.authenticated = true;
                theMessage.SendPrivateMessage("Du bist jetzt authentifiziert");
            }
            else
            {
                theMessage.SendPrivateMessage("Das Passwort war falsch");
            }
            theMessage.Hidden = true;
        }
    }
}
