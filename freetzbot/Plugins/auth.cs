using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;

namespace FritzBot.Plugins
{
    [Name("auth")]
    [Help("Authentifiziert dich wenn du ein Passwort festgelegt hast. z.b. !auth passwort")]
    [ParameterRequired]
    class auth : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            ServerConnection.OnPart += LogoutUser;
            ServerConnection.OnQuit += LogoutUser;
        }

        public void Stop()
        {
            ServerConnection.OnPart -= LogoutUser;
            ServerConnection.OnQuit -= LogoutUser;
        }

        void LogoutUser(object sender, IrcEventArgs e)
        {
            using (var context = new BotContext())
            {
                User u = context.GetUser(e.Data.Nick);
                if (u != null)
                {
                    u.Authentication = DateTime.MinValue;
                    context.SaveChanges();
                }
            }
        }

        public void Run(ircMessage theMessage)
        {
            using (var context = new BotContext())
            {
                User user = context.GetUser(theMessage.Nickname);
                if (user.Authenticated)
                {
                    theMessage.Answer("Du bist bereits authentifiziert");
                    return;
                }
                if (!theMessage.IsPrivate)
                {
                    theMessage.Answer("Ohje das solltest du besser im Query tuen");
                    return;
                }
                if (user.CheckPassword(theMessage.CommandLine))
                {
                    user.Authentication = DateTime.Now;
                    context.SaveChanges();
                    theMessage.SendPrivateMessage("Du bist jetzt authentifiziert");
                }
                else
                {
                    theMessage.SendPrivateMessage("Das Passwort war falsch");
                }
                theMessage.Hidden = true;
            }
        }
    }
}