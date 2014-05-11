using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("passwd")]
    [Help("Ändert dein Passwort. Denk dran dass du das im Query machen solltest. Nach der Eingabe von !passwd wirst du nach weiteren Details gefragt")]
    [Scope(Scope.User)]
    [ParameterRequired(false)]
    class passwd : PluginBase, ICommand
    {
        private string Requested;

        public void Run(ircMessage theMessage)
        {
            theMessage.Hidden = true;
            if (!theMessage.IsPrivate)
            {
                theMessage.SendPrivateMessage("Zu deiner eigenen Sicherheit solltest du das lieber mit mir im Query bereden");
                return;
            }
            Requested = theMessage.Nickname;
            User user;
            using (var context = new BotContext())
            {
                user = context.GetUser(Requested);
            }
            if (!String.IsNullOrEmpty(user.Password))
            {
                theMessage.SendPrivateMessage("Bitte gib zuerst dein altes Passwort ein:");
                ServerConnetion.OnPreProcessingMessage += CheckOldPW;
            }
            else
            {
                theMessage.SendPrivateMessage("Okay bitte gib nun dein Passwort ein");
                ServerConnetion.OnPreProcessingMessage += SetNewPW;
            }
        }

        private void CheckOldPW(object sender, ircMessage theMessage)
        {
            if (!theMessage.IsPrivate || Requested != theMessage.Nickname)
            {
                return;
            }
            theMessage.Hidden = true;
            using (var context = new BotContext())
            {
                if (!context.GetUser(Requested).CheckPassword(theMessage.Message))
                {
                    theMessage.SendPrivateMessage("Passwort korrekt, gib nun dein neues Passwort ein:");
                    ServerConnetion.OnPreProcessingMessage += SetNewPW;
                }
                else
                {
                    theMessage.SendPrivateMessage("Passwort inkorrekt, abbruch!");
                    PluginManager.GetInstance().RecycleScoped(this);
                }
            }
            ServerConnetion.OnPreProcessingMessage -= CheckOldPW;
        }

        void SetNewPW(object sender, ircMessage theMessage)
        {
            if (!theMessage.IsPrivate || Requested != theMessage.Nickname)
            {
                return;
            }
            theMessage.Hidden = true;
            using (var context = new BotContext())
            {
                context.GetUser(Requested).SetPassword(theMessage.Message);
                context.SaveChanges();
            }
            theMessage.SendPrivateMessage("Passwort wurde geändert!");
            ServerConnetion.OnPreProcessingMessage -= SetNewPW;
            PluginManager.GetInstance().RecycleScoped(this);
        }
    }
}