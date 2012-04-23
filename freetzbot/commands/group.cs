using System;
using System.Threading;
using FritzBot;

namespace FritzBot.commands
{
    class group : ICommand
    {
        public String[] Name { get { return new String[] { "group" }; } }
        public String HelpText { get { return "Gruppiert 2 Benutzernamen zu einem internen Benutzer. z.b. !group Suchiman Suchi"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String[] split = message.Split(' ');
            if (Program.TheUsers.Exists(split[0]) && Program.TheUsers.Exists(split[1]))
            {
                if (toolbox.IsOp(sender))
                {
                    Program.TheUsers.GroupUser(split[0], split[1]);
                    connection.Sendmsg("Okay", receiver);
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    if (!String.IsNullOrEmpty(Program.TheUsers[split[i]].password))
                    {
                        connection.Sendmsg("Benutzer " + split[i] + " erfordert ein Passwort!", sender);
                        String Answer = toolbox.AwaitAnswer(sender);
                        if (Program.TheUsers[split[i]].CheckPassword(Answer))
                        {
                            connection.Sendmsg("Korrekt", sender);
                        }
                        else
                        {
                            connection.Sendmsg("Passwort falsch, abbruch!", sender);
                            return;
                        }
                    }
                }
                Program.TheUsers.GroupUser(split[0], split[1]);
                connection.Sendmsg("Okay", receiver);
            }
            else
            {
                connection.Sendmsg("Ich konnte mindestens einen der angegebenen Benutzer nicht finden", receiver);
            }
        }
    }
}
