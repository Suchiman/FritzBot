using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("passwd")]
    [Module.Help("Ändert dein Passwort. Denk dran dass du das im Query machen solltest. Nach der Eingabe von !passwd wirst du nach weiteren Details gefragt")]
    [Module.Scope(Scope.User)]
    [Module.ParameterRequired(false)]
    class passwd : PluginBase, ICommand
    {
        private User Requested;

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
                Requested = theMessage.TheUser;
                if (!String.IsNullOrEmpty(theMessage.TheUser.Password))
                {
                    theMessage.SendPrivateMessage("Bitte gib zuerst dein altes Passwort ein:");
                    Server.OnPreProcessingMessage += CheckOldPW;
                }
                else
                {
                    theMessage.SendPrivateMessage("Okay bitte gib nun dein Passwort ein");
                    Server.OnPreProcessingMessage += SetNewPW;
                }
            }
        }

        private void CheckOldPW(object sender, ircMessage theMessage)
        {
            if (!theMessage.IsPrivate || Requested != theMessage.TheUser)
            {
                return;
            }
            theMessage.Hidden = true;
            if (theMessage.TheUser.CheckPassword(theMessage.Message))
            {
                theMessage.SendPrivateMessage("Passwort korrekt, gib nun dein neues Passwort ein:");
                Server.OnPreProcessingMessage += SetNewPW;
            }
            else
            {
                theMessage.SendPrivateMessage("Passwort inkorrekt, abbruch!");
                PluginManager.GetInstance().RecycleScoped(this);
            }
            Server.OnPreProcessingMessage -= CheckOldPW;
        }

        void SetNewPW(object sender, ircMessage theMessage)
        {
            if (!theMessage.IsPrivate || Requested != theMessage.TheUser)
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
            Server.OnPreProcessingMessage -= SetNewPW;
            PluginManager.GetInstance().RecycleScoped(this);
        }
    }
}