using FritzBot.DataModel;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Module.Name("calc")]
    [Module.Help("Ich kann sogar Rechnen :-) !calc 42*13+1 !calc 42*(42-(24+24)+1*3)/2")]
    [Module.ParameterRequired]
    class calc : PluginBase, ICommand
    {
        char[] superscripts = { '⁰', '¹', '²', '³', '⁴', '⁵', '⁶', '⁷', '⁸', '⁹' };

        public void Run(ircMessage theMessage)
        {
            string result = toolbox.GetWeb(String.Format("http://www.google.com/ig/calculator?q={0}", toolbox.UrlEncode(theMessage.CommandLine)));
            result = OktalUnescape(result);

            CalculationResult cr = JsonConvert.DeserializeObject<CalculationResult>(result);
            if (cr == null)
            {
                theMessage.Answer("Fehler beim Aufruf der API");
            }
            else if (!String.IsNullOrEmpty(cr.error))
            {
                theMessage.Answer("Die Eingabe ist ungültig oder konnte nicht interpretiert werden: " + cr.error);
            }
            else
            {
                theMessage.Answer(String.Format("{0} ergibt {1}", FixOutput(cr.lhs), FixOutput(cr.rhs)));
            }
        }

        public string FixOutput(string input)
        {
            input = HtmlEntity.DeEntitize(input);
            input = Regex.Replace(input, @"\<sup\>(\d*)\<\/sup\>", m => new String(m.Groups[1].Value.ToCharArray().Select(x => superscripts[Convert.ToInt32(x.ToString())]).ToArray()));
            return input;
        }

        public string OktalUnescape(string input)
        {
            StringBuilder _buffer = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];

                if (currentChar == '\\' && input[i + 1] == 'x')
                {
                    currentChar = input[++i];
                }
                else
                {
                    _buffer.Append(currentChar);
                    continue;
                }

                char[] hexValues = new char[2];
                char hexChar;
                for (int h = 0; h < hexValues.Length; h++)
                {
                    if (i + 1 < input.Length && (currentChar = input[++i]) != '\0')
                        hexValues[h] = currentChar;
                }

                hexChar = Convert.ToChar(int.Parse(new string(hexValues), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
                _buffer.Append(hexChar);
            }
            return _buffer.ToString();
        }
    }

    class CalculationResult
    {
        public string lhs { get; set; }
        public string rhs { get; set; }
        public string error { get; set; }
        public bool icc { get; set; }
    }
}