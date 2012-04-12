using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace freetzbot.commands
{
    class group : command
    {
        private String[] name = { "group" };
        private String helptext = "Gruppiert 2 Benutzernamen zu einem internen Benutzer. z.b. !group Suchiman Suchi";
        private Boolean op_needed = false;
        private Boolean parameter_needed = true;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            String[] split = message.Split(' ');
            if (freetzbot.Program.TheUsers.Exists(split[0]) && freetzbot.Program.TheUsers.Exists(split[1]))
            {
                if (toolbox.op_check(sender))
                {
                    freetzbot.Program.TheUsers.GroupUser(split[0], split[1]);
                    connection.sendmsg("Okay", receiver);
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    if (freetzbot.Program.TheUsers[split[i]].password != "")
                    {
                        connection.sendmsg("Benutzer " + split[i] + " erfordert ein Passwort!", sender);
                        freetzbot.Program.await_response = true;
                        freetzbot.Program.awaited_nick = sender;
                        while (freetzbot.Program.await_response)
                        {
                            Thread.Sleep(50);
                        }
                        if (freetzbot.Program.TheUsers[split[i]].CheckPassword(freetzbot.Program.awaited_response))
                        {
                            connection.sendmsg("Korrekt", sender);
                        }
                        else
                        {
                            connection.sendmsg("Passwort falsch, abbruch!", sender);
                            return;
                        }
                    }
                }
                freetzbot.Program.TheUsers.GroupUser(split[0], split[1]);
                connection.sendmsg("Okay", receiver);
            }
            else
            {
                connection.sendmsg("Ich konnte mindestens einen der angegebenen Benutzer nicht finden", receiver);
            }
        }
    }
}
