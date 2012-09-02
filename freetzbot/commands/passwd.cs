using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("passwd")]
    [Module.Help("Ändert dein Passwort. Denk dran dass du das im Query machen solltest. Nach der Eingabe von !passwd wirst du nach weiteren Details gefragt")]
    [Module.ParameterRequired(false)]
    class passwd : ICommand, IBackgroundTask
    {
        private List<String> CheckUserInProgress = new List<String>();

        private void CheckOldPW(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && CheckUserInProgress.Contains(theMessage.Nick)))
            {
                return;
            }
            theMessage.Hidden = true;
            if (theMessage.TheUser.CheckPassword(theMessage.Message))
            {
                theMessage.SendPrivateMessage("Passwort korrekt, gib nun dein neues Passwort ein:");
                SetUserInProgress.Add(theMessage.Nick);
                CheckUserInProgress.Remove(theMessage.Nick);
            }
            else
            {
                theMessage.SendPrivateMessage("Passwort inkorrekt, abbruch!");
                CheckUserInProgress.Remove(theMessage.Nick);
            }
        }

        private List<String> SetUserInProgress = new List<String>();

        void SetNewPW(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && SetUserInProgress.Contains(theMessage.Nick)))
            {
                return;
            }
            theMessage.Hidden = true;
            theMessage.TheUser.SetPassword(theMessage.Message);
            theMessage.SendPrivateMessage("Passwort wurde geändert!");
            SetUserInProgress.Remove(theMessage.Nick);
        }

        public void Stop()
        {
            Program.UserMessaged -= new Program.MessageEventHandler(SetNewPW);
            Program.UserMessaged -= new Program.MessageEventHandler(CheckOldPW);
        }

        public void Start()
        {
            Program.UserMessaged += new Program.MessageEventHandler(SetNewPW);//Vorsicht: Reihenfolge wichtig
            Program.UserMessaged += new Program.MessageEventHandler(CheckOldPW);
        }

        public void Run(ircMessage theMessage)
        {
            theMessage.Hidden = true;
            if (!theMessage.IsPrivate)
            {
                theMessage.SendPrivateMessage("Zu deiner eigenen Sicherheit solltest du das lieber mit mir im Query bereden");
                return;
            }
            else
            {
                if (!String.IsNullOrEmpty(theMessage.TheUser.password))
                {
                    theMessage.SendPrivateMessage("Bitte gib zuerst dein altes Passwort ein:");
                    CheckUserInProgress.Add(theMessage.Nick);
                }
                else
                {
                    theMessage.SendPrivateMessage("Okay bitte gib nun dein Passwort ein");
                    SetUserInProgress.Add(theMessage.Nick);
                }
            }
        }
    }
}
