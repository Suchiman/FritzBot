using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FritzBot.commands
{
    class passwd : ICommand
    {
        public String[] Name { get { return new String[] { "passwd" }; } }
        public String HelpText { get { return "Ändert dein Passwort. Denk dran dass du das im Query machen solltest. Nach der Eingabe von !passwd wirst du nach weiteren Details gefragt"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (sender != receiver)
            {
                connection.Sendmsg("Zu deiner eigenen Sicherheit solltest du das lieber mit mir im Query bereden", sender);
                return;
            }
            else
            {
                if (!String.IsNullOrEmpty(FritzBot.Program.TheUsers[sender].password))
                {
                    connection.Sendmsg("Bitte gib zuerst dein altes Passwort ein:", sender);
                    FritzBot.Program.await_response = true;
                    FritzBot.Program.awaited_nick = sender;
                    while (FritzBot.Program.await_response)
                    {
                        Thread.Sleep(50);
                    }
                    if (FritzBot.Program.TheUsers[sender].CheckPassword(FritzBot.Program.awaited_response))
                    {
                        connection.Sendmsg("Passwort korrekt, gib nun dein neues Passwort ein:", sender);
                    }
                    else
                    {
                        connection.Sendmsg("Passwort inkorrekt, abbruch!", sender);
                        return;
                    }
                    FritzBot.Program.await_response = true;
                    FritzBot.Program.awaited_nick = sender;
                    while (FritzBot.Program.await_response)
                    {
                        Thread.Sleep(50);
                    }
                    FritzBot.Program.TheUsers[sender].SetPassword(FritzBot.Program.awaited_response);
                    connection.Sendmsg("Passwort wurde geändert!", sender);
                }
                else
                {
                    connection.Sendmsg("Okay bitte gib nun dein Passwort ein", sender);
                    FritzBot.Program.await_response = true;
                    FritzBot.Program.awaited_nick = sender;
                    while (FritzBot.Program.await_response)
                    {
                        Thread.Sleep(50);
                    }
                    FritzBot.Program.TheUsers[sender].SetPassword(FritzBot.Program.awaited_response);
                    connection.Sendmsg("Passwort wurde festgelegt!", sender);
                }
            }
        }
    }
}
