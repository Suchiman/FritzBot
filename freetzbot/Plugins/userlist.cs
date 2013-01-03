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
            string output = String.Join(", ", UserManager.GetInstance().Where(x => x.GetModulUserStorage("box").Storage.Elements("box").Count() > 0).Select(x => x.names.FirstOrDefault()).ToArray<String>());
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