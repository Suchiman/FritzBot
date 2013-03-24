using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("boxinfo", "box")]
    [Module.Help("Zeigt die Box/en des angegebenen Benutzers an.")]
    class boxinfo : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string output = "";
            string UserToUse = theMessage.CommandLine;
            if (!theMessage.HasArgs)
            {
                UserToUse = theMessage.Nickname;
            }
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(UserToUse);
                if (u != null)
                {
                    BoxEntry boxen = db.QueryLinkedData<BoxEntry, User>(u).FirstOrDefault();
                    if (boxen != null)
                    {
                        output += String.Join(", ", boxen.GetRawUserBoxen().ToArray());
                    }
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