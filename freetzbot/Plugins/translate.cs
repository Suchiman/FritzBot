using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("translate", "t")]
    [Module.Help("Übersetzt zwischen Sprachen ;-), Beispiel: !translate en Guten Morgen")]
    class translate : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string target = theMessage.CommandArgs.First().ToLower();
            string word = String.Join(" ", theMessage.CommandArgs.Skip(1).ToArray());
            if (target.Length != 2)
            {
                target = "en";
                word = String.Join(" ", theMessage.CommandArgs.ToArray());
            }

            string translation = GoogleTranslator.TranslateTextSimple(word, target);

            if (!String.IsNullOrEmpty(translation))
            {
                theMessage.Answer(translation);
            }
            else
            {
                theMessage.Answer("Das hat nicht so geklappt wie erwartet");
            }
        }
    }
}