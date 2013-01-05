using FritzBot.DataModel;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
            string url = String.Format("http://translate.google.com/translate_a/t?client=t&text={0}&hl={1}&sl={2}&tl={1}&ie=UTF-8&oe=UTF-8&multires=1&otf=1&pc=1&trs=1&ssel=3&tsel=6&sc=1", toolbox.UrlEncode(word), target, ""); //"" Für Auto Erkennung der Ausgangssprache
            string response = toolbox.GetWeb(url);
            Match m = Regex.Match(response, "\\[\\[\\[\"(?<translation>[^\"]*)\",\"(?<input>[^\"]*)\",\"\",\"\"\\]\\],.*,\"(?<source>[^\"]*)\",.*,");
            if (m.Success)
            {
                theMessage.Answer(DecodeEncodedNonAsciiCharacters(m.Groups["translation"].Value));
            }
            else
            {
                theMessage.Answer("Die API hat mir eine unerwartete Antwort geliefert");
            }
        }

        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(value, @"\\u(?<Value>[a-zA-Z0-9]{4})", m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }
    }
}