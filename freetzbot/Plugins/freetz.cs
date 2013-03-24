using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("freetz", "f", "fp")]
    [Module.Help("Das erzeugt einen Link zu einem Freetz Packet, Beispiel: !fp dnsmasq")]
    class freetz : PluginBase, ICommand
    {
        private DataCache<List<KeyValuePair<string, string>?>> PackagesCache = new DataCache<List<KeyValuePair<string, string>?>>(GetPackages, 30);

        public void Run(ircMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                theMessage.Answer("http://freetz.org/");
                return;
            }

            int lowestDifference = 0;
            string input = theMessage.CommandLine.ToLower();
            string sharpSplit = String.Empty;
            if (input.Contains('#'))
            {
                string[] split = input.Split('#');
                sharpSplit = "#" + split[1];
                input = split[0];
            }
            int inputLength = input.Length;
            List<KeyValuePair<string, string>?> packages = PackagesCache.GetItem(true).Where(x => x.HasValue).ToList(); //PackagesCache.GetItem(true).Where(x => x.HasValue && (x.Value.Key.Length < (inputLength + 2) && x.Value.Key.Length > (inputLength - 2))).ToList();
            KeyValuePair<string, string>? TheChoosenOne = packages.FirstOrDefault(x => x.Value.Key.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (TheChoosenOne == null)
            {
                TheChoosenOne = packages.Where(x => x.HasValue).FirstOrDefault(x => x.Value.Key.ToLower().StartsWith(input));
                if (TheChoosenOne != null)
                {
                    lowestDifference = Math.Abs(TheChoosenOne.Value.Key.Length - inputLength);
                }
            }
            if (TheChoosenOne == null)
            {
                lowestDifference = 1000;
                foreach (KeyValuePair<string, string> one in packages)
                {
                    int result = StringSimilarity.Compare(input, one.Key, true);
                    if (result < lowestDifference)
                    {
                        TheChoosenOne = one;
                        lowestDifference = result;
                    }
                }
            }
            if (TheChoosenOne.HasValue)
            {
                if (lowestDifference > 1)
                {
                    theMessage.Answer(String.Format("Meinten Sie? {0}: http://freetz.org{1}{2}", TheChoosenOne.Value.Key, TheChoosenOne.Value.Value, sharpSplit));
                }
                else
                {
                    theMessage.Answer(String.Format("Freetz Packet {0}: http://freetz.org{1}{2}", TheChoosenOne.Value.Key, TheChoosenOne.Value.Value, sharpSplit));
                }
            }
            else
            {
                theMessage.Answer("Da ist etwas fürchterlich schief gelaufen");
            }
        }

        private static List<KeyValuePair<string, string>?> GetPackages(List<KeyValuePair<string, string>?> Alte)
        {
            try
            {
                HtmlNode document = new HtmlDocument().LoadUrl("http://freetz.org/wiki/packages").DocumentNode;
                return document.SelectNodes("//table[@class='wiki']/tr/td/a[@class='wiki']").Select(x => new KeyValuePair<string, string>?(new KeyValuePair<string, string>(x.InnerText, x.GetAttributeValue("href", "")))).ToList();
            }
            catch
            {
                return Alte;
            }
        }
    }
}