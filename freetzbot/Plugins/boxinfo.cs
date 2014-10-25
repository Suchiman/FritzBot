using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("boxinfo", "box")]
    [Help("Zeigt die Box/en des angegebenen Benutzers an.")]
    class boxinfo : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            string output = "";
            string UserToUse = theMessage.CommandLine;
            if (!theMessage.HasArgs)
            {
                UserToUse = theMessage.Nickname;
            }
            using (var context = new BotContext())
            {
                User u = context.GetUser(UserToUse);
                if (u != null)
                {
                    BoxManager mgr = new BoxManager(u, context);
                    output += mgr.GetRawUserBoxen().Join(", ");
                }
                else
                {
                    theMessage.Answer("Der Benutzer ist mir nicht bekannt");
                    return;
                }
            }
            if (String.IsNullOrEmpty(output))
            {
                if (!theMessage.HasArgs)
                {
                    theMessage.Answer("Für dich existieren keine Einträge");
                }
                else
                {
                    theMessage.Answer("Meine Datenbank enthält keinen Eintrag über diesen Benutzer");
                }
                return;
            }
            if (!theMessage.HasArgs)
            {
                theMessage.Answer("Entsprechend der Datenbank wurden die folgenden Boxen registriert: " + output);
            }
            else
            {
                theMessage.Answer(UserToUse + " besitzt laut Datenbank " + output);
            }
        }
    }
}