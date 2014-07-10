using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Linq;
using System.Text;

namespace FritzBot.Plugins
{
    [Name("translate", "t")]
    [Help("Übersetzt zwischen Sprachen ;-), Beispiel: !translate en Guten Morgen")]
    class translate : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string target = theMessage.CommandArgs.First().ToLower();
            string word = theMessage.CommandArgs.Skip(1).Join(" ");
            if (target.Length != 2)
            {
                target = "en";
                word = theMessage.CommandArgs.Join(" ");
            }

            Translation translation = GoogleTranslator.GetTranslation(word, target, "");

            if (translation == null || String.IsNullOrEmpty(translation.FullTranslation))
            {
                theMessage.Answer("Das hat nicht so geklappt wie erwartet");
                return;
            }
            if (translation.dict != null && theMessage.CommandArgs.Where(x => x != "en").Count() == 1)
            {
                StringBuilder sb = new StringBuilder(translation.FullTranslation);
                foreach (Dic item in translation.dict)
                {
                    sb.AppendFormat(" - {0}: {1}", item.pos, item.terms.Take(5).Join(", "));
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