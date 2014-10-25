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

        public void Run(IrcMessage theMessage)
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
                ServerConnection.OnPreProcessingMessage += CheckOldPW;
            }
            else
            {
                theMessage.SendPrivateMessage("Okay bitte gib nun dein Passwort ein");
                ServerConnection.OnPreProcessingMessage += SetNewPW;
            }
        }

        private void CheckOldPW(object sender, IrcMessage theMessage)
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
                    ServerConnection.OnPreProcessingMessage += SetNewPW;
                }
                else
                {
                    theMessage.SendPrivateMessage("Passwort inkorrekt, abbruch!");
                    PluginManager.RecycleScoped(this);
                }
            }
            ServerConnection.OnPreProcessingMessage -= CheckOldPW;
        }

        void SetNewPW(object sender, IrcMessage theMessage)
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
            ServerConnection.OnPreProcessingMessage -= SetNewPW;
            PluginManager.RecycleScoped(this);
        }
    }
}