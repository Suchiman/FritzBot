using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Linq;
using System.Text;

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

            Translation translation = GoogleTranslator.GetTranslation(word, target, "");

            if (translation == null || String.IsNullOrEmpty(translation.FullTranslation))
            {
                theMessage.Answer("Das hat nicht so geklappt wie erwartet");
                return;
            }
            else if (translation.dict != null && theMessage.CommandArgs.Where(x => x != "en").Count() == 1)
            {
                StringBuilder sb = new StringBuilder(translation.FullTranslation);
                foreach (Dic item in translation.dict)
                {
                    sb.AppendFormat(" - {0}: {1}", item.pos, String.Join(", ", item.terms.Take(5)));
                }
                theMessage.Answer(sb.ToString());
            }
            else if (!String.IsNullOrEmpty(translation.FullTranslation))
            {
                theMessage.Answer(translation.FullTranslation);
            }
        }
    }
}