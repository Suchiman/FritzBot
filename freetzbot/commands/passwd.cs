using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace freetzbot.commands
{
    class passwd : command
    {
        private String[] name = { "passwd" };
        private String helptext = "Ändert dein Passwort. Denk dran dass du das im Query machen solltest. Nach der Eingabe von !passwd wirst du nach weiteren Details gefragt";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
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
            if (sender != receiver)
            {
                connection.sendmsg("Zu deiner eigenen Sicherheit solltest du das lieber mit mir im Query bereden", sender);
                return;
            }
            else
            {
                if (freetzbot.Program.TheUsers[sender].password != "")
                {
                    connection.sendmsg("Bitte gib zuerst dein altes Passwort ein:", sender);
                    freetzbot.Program.await_response = true;
                    freetzbot.Program.awaited_nick = sender;
                    while (freetzbot.Program.await_response)
                    {
                        Thread.Sleep(50);
                    }
                    if (freetzbot.Program.TheUsers[sender].CheckPassword(freetzbot.Program.awaited_response))
                    {
                        connection.sendmsg("Passwort korrekt, gib nun dein neues Passwort ein:", sender);
                    }
                    else
                    {
                        connection.sendmsg("Passwort inkorrekt, abbruch!", sender);
                        return;
                    }
                    freetzbot.Program.await_response = true;
                    freetzbot.Program.awaited_nick = sender;
                    while (freetzbot.Program.await_response)
                    {
                        Thread.Sleep(50);
                    }
                    freetzbot.Program.TheUsers[sender].SetPassword(freetzbot.Program.awaited_response);
                    connection.sendmsg("Passwort wurde geändert!", sender);
                }
                else
                {
                    connection.sendmsg("Okay bitte gib nun dein Passwort ein", sender);
                    freetzbot.Program.await_response = true;
                    freetzbot.Program.awaited_nick = sender;
                    while (freetzbot.Program.await_response)
                    {
                        Thread.Sleep(50);
                    }
                    freetzbot.Program.TheUsers[sender].SetPassword(freetzbot.Program.awaited_response);
                    connection.sendmsg("Passwort wurde festgelegt!", sender);
                }
            }
        }
    }
}
