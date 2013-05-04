using FritzBot.Core;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("auth")]
    [Module.Help("Authentifiziert dich wenn du ein Passwort festgelegt hast. z.b. !auth passwort")]
    [Module.ParameterRequired]
    class auth : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            Server.OnPart += LogoutUser;
            Server.OnQuit += LogoutUser;
        }

        public void Stop()
        {
            Server.OnPart -= LogoutUser;
            Server.OnQuit -= LogoutUser;
        }

        void LogoutUser(object sender, IrcEventArgs e)
        {
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(e.Data.Nick);
                if (u != null)
                {
                    u.Authentication = DateTime.MinValue;
                    db.SaveOrUpdate(u);
                }
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.TheUser.Authenticated)
            {
                theMessage.Answer("Du bist bereits authentifiziert");
                return;
            }
            if (!theMessage.IsPrivate)
            {
                theMessage.Answer("Ohje das solltest du besser im Query tuen");
                return;
            }
            if (theMessage.TheUser.CheckPassword(theMessage.CommandLine))
            {
                using (DBProvider db = new DBProvider())
                {
                    theMessage.TheUser.Authentication = DateTime.Now;
                    db.SaveOrUpdate(theMessage.TheUser);
                }
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