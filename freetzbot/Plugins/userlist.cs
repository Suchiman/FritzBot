using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("userlist")]
    [Help("Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben.")]
    [ParameterRequired(false)]
    class userlist : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            using (var context = new BotContext())
            {
                string output = context.BoxEntries.Select(x => x.User.LastUsedName.Name).Distinct().Join(", ");
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