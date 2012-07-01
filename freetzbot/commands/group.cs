using System;
using System.Collections.Generic;

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
            Program.UserMessaged -= new Program.MessageEventHandler(CheckPhase1);
            Program.UserMessaged -= new Program.MessageEventHandler(CheckPhase2);
        }

        public group()
        {
            Program.UserMessaged += new Program.MessageEventHandler(CheckPhase1);
            Program.UserMessaged += new Program.MessageEventHandler(CheckPhase2);
        }

        private List<ircMessage> UserRequested = new List<ircMessage>();
        private List<ircMessage> Check1Done = new List<ircMessage>();

        private ircMessage GetInProgressMessage(ircMessage theRequester)
        {
            foreach (ircMessage oneMessage in UserRequested)
            {
                if (oneMessage.Nick == theRequester.Nick)
                {
                    return oneMessage;
                }
            }
            return null;
        }

        private void CheckPhase1(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && GetInProgressMessage(theMessage) != null && !String.IsNullOrEmpty(Program.TheUsers[GetInProgressMessage(theMessage).CommandArgs[0]].password) && !Check1Done.Contains(GetInProgressMessage(theMessage))))
            {
                return;
            }
            if (Program.TheUsers[GetInProgressMessage(theMessage).CommandArgs[0]].CheckPassword(theMessage.Message))
            {
                theMessage.Answer("Korrekt");
                Check1Done.Add(GetInProgressMessage(theMessage));
                if (!String.IsNullOrEmpty(Program.TheUsers[GetInProgressMessage(theMessage).CommandArgs[1]].password))
                {
                    theMessage.Answer(GetInProgressMessage(theMessage).CommandArgs[1] + " erfordert ein Passwort");
                }
                else
                {
                    CheckPhase2(theMessage);
                }
            }
            else
            {
                theMessage.Answer("Passwort falsch, abbruch!");
                UserRequested.Remove(GetInProgressMessage(theMessage));
            }
            theMessage.Handled = true;
            theMessage.Hidden = true;
        }

        private void CheckPhase2(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && GetInProgressMessage(theMessage) != null && !theMessage.Handled))
            {
                return;
            }
            if (String.IsNullOrEmpty(Program.TheUsers[GetInProgressMessage(theMessage).CommandArgs[1]].password) || Program.TheUsers[GetInProgressMessage(theMessage).CommandArgs[1]].CheckPassword(theMessage.Message))
            {
                Program.TheUsers.GroupUser(GetInProgressMessage(theMessage).CommandArgs[0], GetInProgressMessage(theMessage).CommandArgs[1]);
                theMessage.Answer("Benutzer erfolgreich verschmolzen!");
            }
            else
            {
                theMessage.Answer("Passwort falsch, abbruch!");
            }
            UserRequested.Remove(GetInProgressMessage(theMessage));
            theMessage.Handled = true;
            theMessage.Hidden = true;
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.TheUsers.Exists(theMessage.CommandArgs[0]) && theMessage.TheUsers.Exists(theMessage.CommandArgs[1]))
            {
                if (toolbox.IsOp(theMessage.Nick) || (String.IsNullOrEmpty(Program.TheUsers[theMessage.CommandArgs[0]].password) && String.IsNullOrEmpty(Program.TheUsers[theMessage.CommandArgs[1]].password)))
                {
                    theMessage.TheUsers.GroupUser(theMessage.CommandArgs[0], theMessage.CommandArgs[1]);
                    theMessage.Answer("Okay");
                    return;
                }
                if (!String.IsNullOrEmpty(Program.TheUsers[theMessage.CommandArgs[0]].password))
                {
                    theMessage.SendPrivateMessage("Benutzer " + theMessage.CommandArgs[0] + " erfordert ein Passwort!");
                    UserRequested.Add(theMessage);
                }
                else if (!String.IsNullOrEmpty(Program.TheUsers[theMessage.CommandArgs[1]].password))
                {
                    theMessage.SendPrivateMessage("Benutzer " + theMessage.CommandArgs[1] + " erfordert ein Passwort!");
                    UserRequested.Add(theMessage);
                }
            }
            else
            {
                theMessage.Answer("Ich konnte mindestens einen der angegebenen Benutzer nicht finden");
            }
        }
    }
}