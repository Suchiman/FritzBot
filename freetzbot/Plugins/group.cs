using FritzBot.DataModel;
using System;
using System.Collections.Generic;

namespace FritzBot.Plugins
{
    //[Module.Name("group")]
    [Module.Help("Gruppiert 2 Benutzernamen zu einem internen Benutzer. z.b. !group Suchiman Suchi")]
    [Module.ParameterRequired]
    class group : PluginBase, ICommand, IBackgroundTask
    {
        public void Stop()
        {
            Program.UserMessaged -= CheckPhase1;
            Program.UserMessaged -= CheckPhase2;
        }

        public void Start()
        {
            Program.UserMessaged += CheckPhase1;
            Program.UserMessaged += CheckPhase2;
        }

        private List<ircMessage> UserRequested = new List<ircMessage>();
        private List<ircMessage> Check1Done = new List<ircMessage>();

        private ircMessage GetInProgressMessage(ircMessage theRequester)
        {
            foreach (ircMessage oneMessage in UserRequested)
            {
                if (oneMessage.Nickname == theRequester.Nickname)
                {
                    return oneMessage;
                }
            }
            return null;
        }

        private void CheckPhase1(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && GetInProgressMessage(theMessage) != null && !String.IsNullOrEmpty(UserManager.GetInstance()[GetInProgressMessage(theMessage).CommandArgs[0]].password) && !Check1Done.Contains(GetInProgressMessage(theMessage))))
            {
                return;
            }
            if (UserManager.GetInstance()[GetInProgressMessage(theMessage).CommandArgs[0]].CheckPassword(theMessage.Message))
            {
                theMessage.Answer("Korrekt");
                Check1Done.Add(GetInProgressMessage(theMessage));
                if (!String.IsNullOrEmpty(UserManager.GetInstance()[GetInProgressMessage(theMessage).CommandArgs[1]].password))
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
            if (String.IsNullOrEmpty(UserManager.GetInstance()[GetInProgressMessage(theMessage).CommandArgs[1]].password) || UserManager.GetInstance()[GetInProgressMessage(theMessage).CommandArgs[1]].CheckPassword(theMessage.Message))
            {
                UserManager.GetInstance().GroupUser(GetInProgressMessage(theMessage).CommandArgs[0], GetInProgressMessage(theMessage).CommandArgs[1]);
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
            if (UserManager.GetInstance().Exists(theMessage.CommandArgs[0]) && UserManager.GetInstance().Exists(theMessage.CommandArgs[1]))
            {
                if (toolbox.IsOp(theMessage.Nickname) || (String.IsNullOrEmpty(UserManager.GetInstance()[theMessage.CommandArgs[0]].password) && String.IsNullOrEmpty(UserManager.GetInstance()[theMessage.CommandArgs[1]].password)))
                {
                    UserManager.GetInstance().GroupUser(theMessage.CommandArgs[0], theMessage.CommandArgs[1]);
                    theMessage.Answer("Okay");
                    return;
                }
                if (!String.IsNullOrEmpty(UserManager.GetInstance()[theMessage.CommandArgs[0]].password))
                {
                    theMessage.SendPrivateMessage("Benutzer " + theMessage.CommandArgs[0] + " erfordert ein Passwort!");
                    UserRequested.Add(theMessage);
                }
                else if (!String.IsNullOrEmpty(UserManager.GetInstance()[theMessage.CommandArgs[1]].password))
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