using System;
using System.Threading;
using FritzBot;

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
                if (!String.IsNullOrEmpty(Program.TheUsers[sender].password))
                {
                    connection.Sendmsg("Bitte gib zuerst dein altes Passwort ein:", sender);
                    String Password = toolbox.AwaitAnswer(sender);
                    if (Program.TheUsers[sender].CheckPassword(Password))
                    {
                        connection.Sendmsg("Passwort korrekt, gib nun dein neues Passwort ein:", sender);
                    }
                    else
                    {
                        connection.Sendmsg("Passwort inkorrekt, abbruch!", sender);
                        return;
                    }
                    Password = toolbox.AwaitAnswer(sender);
                    Program.TheUsers[sender].SetPassword(Password);
                    connection.Sendmsg("Passwort wurde geändert!", sender);
                }
                else
                {
                    connection.Sendmsg("Okay bitte gib nun dein Passwort ein", sender);
                    String Password = toolbox.AwaitAnswer(sender);
                    Program.TheUsers[sender].SetPassword(Password);
                    connection.Sendmsg("Passwort wurde festgelegt!", sender);
                }
            }
        }
    }
}
