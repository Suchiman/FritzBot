using System;
using FritzBot;

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

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (sender != receiver)
            {
                connection.Sendmsg("Ohje das solltest du besser im Query tuen", receiver);
                return;
            }
            if (Program.TheUsers[sender].CheckPassword(message))
            {
                Program.TheUsers[sender].authenticated = true;
                connection.Sendmsg("Du bist jetzt authentifiziert", sender);
            }
            else
            {
                connection.Sendmsg("Das Passwort war falsch", sender);
            }
        }
    }
}
