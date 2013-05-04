using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;

namespace FritzBot.Plugins
{
    [Module.Name("passwd")]
    [Module.Help("Ändert dein Passwort. Denk dran dass du das im Query machen solltest. Nach der Eingabe von !passwd wirst du nach weiteren Details gefragt")]
    [Module.ParameterRequired(false)]
    class passwd : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            Server.OnPreProcessingMessage += Server_OnPreProcessingMessage;
        }

        public void Stop()
        {
            Server.OnPreProcessingMessage -= Server_OnPreProcessingMessage;
        }

        private void Server_OnPreProcessingMessage(object sender, ircMessage theMessage)
        {
            SetNewPW(theMessage);
            CheckOldPW(theMessage);
        }

        private List<string> CheckUserInProgress = new List<string>();

        private void CheckOldPW(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && CheckUserInProgress.Contains(theMessage.Nickname)))
            {
                return;
            }
            theMessage.Hidden = true;
            if (theMessage.TheUser.CheckPassword(theMessage.Message))
            {
                theMessage.SendPrivateMessage("Passwort korrekt, gib nun dein neues Passwort ein:");
                SetUserInProgress.Add(theMessage.Nickname);
                CheckUserInProgress.Remove(theMessage.Nickname);
            }
            else
            {
                theMessage.SendPrivateMessage("Passwort inkorrekt, abbruch!");
                CheckUserInProgress.Remove(theMessage.Nickname);
            }
        }

        private List<string> SetUserInProgress = new List<string>();

        void SetNewPW(ircMessage theMessage)
        {
            if (!(theMessage.IsPrivate && SetUserInProgress.Contains(theMessage.Nickname)))
            {
                return;
            }
            theMessage.Hidden = true;
            using (DBProvider db = new DBProvider())
            {
                theMessage.TheUser.SetPassword(theMessage.Message);
                db.SaveOrUpdate(theMessage.TheUser);
            }
            theMessage.SendPrivateMessage("Passwort wurde geändert!");
            SetUserInProgress.Remove(theMessage.Nickname);
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
                if (!String.IsNullOrEmpty(theMessage.TheUser.Password))
                {
                    theMessage.SendPrivateMessage("Bitte gib zuerst dein altes Passwort ein:");
                    CheckUserInProgress.Add(theMessage.Nickname);
                }
                else
                {
                    theMessage.SendPrivateMessage("Okay bitte gib nun dein Passwort ein");
                    SetUserInProgress.Add(theMessage.Nickname);
                }
            }
        }
    }
}