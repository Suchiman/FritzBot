using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("userlist")]
    [Module.Help("Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben.")]
    [Module.ParameterRequired(false)]
    class userlist : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (DBProvider db = new DBProvider())
            {
                string output = String.Join(", ", db.Query<BoxEntry>(x => x.Count > 0).Select(x => x.Reference).NotNull().Select(x => x.LastUsedName).ToArray());
                if (!String.IsNullOrEmpty(output))
                {
                    theMessage.SendPrivateMessage("Diese Benutzer haben bei mir mindestens eine Box registriert: " + output);
                }
                else
                {
                    theMessage.Answer("Ich fürchte, mir ist ein Fehler unterlaufen. Ich kann keine registrierten Benutzer feststellen.");
                }
            }
        }
    }
}