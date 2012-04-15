using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
            if (FritzBot.Program.TheUsers.Exists(split[0]) && FritzBot.Program.TheUsers.Exists(split[1]))
            {
                if (toolbox.OpCheck(sender))
                {
                    FritzBot.Program.TheUsers.GroupUser(split[0], split[1]);
                    connection.Sendmsg("Okay", receiver);
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    if (!String.IsNullOrEmpty(FritzBot.Program.TheUsers[split[i]].password))
                    {
                        connection.Sendmsg("Benutzer " + split[i] + " erfordert ein Passwort!", sender);
                        FritzBot.Program.await_response = true;
                        FritzBot.Program.awaited_nick = sender;
                        while (FritzBot.Program.await_response)
                        {
                            Thread.Sleep(50);
                        }
                        if (FritzBot.Program.TheUsers[split[i]].CheckPassword(FritzBot.Program.awaited_response))
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
                FritzBot.Program.TheUsers.GroupUser(split[0], split[1]);
                connection.Sendmsg("Okay", receiver);
            }
            else
            {
                connection.Sendmsg("Ich konnte mindestens einen der angegebenen Benutzer nicht finden", receiver);
            }
        }
    }
}
