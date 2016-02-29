using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("fp")]
    [Help("Das erzeugt einen Link zu einem Freetz Paket, Beispiel: !fp dnsmasq")]
    class freetz : PluginBase, ICommand
    {
        private const string PackagesPage = "http://freetz.org/wiki/packages";

        private DataCache<List<FreetzPackage>> PackagesCache = new DataCache<List<FreetzPackage>>(GetPackages, TimeSpan.FromMinutes(30));

        public void Run(IrcMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                theMessage.Answer(PackagesPage);
                return;
            }

            string input = theMessage.CommandLine;
            string anchor = String.Empty;
            if (input.Contains('#'))
            {
                string[] split = input.Split(new[] { '#' }, 2);
                Contract.Assume(split.Length == 2);
                anchor = "#" + split[1];
                input = split[0];
            }

            int lowestFuzzyDifference = 0, lowestStartsWithDifference = 0;
            FreetzPackage exactMatch = null, fuzzyMatch = null, startsWithMatch = null;

            List<FreetzPackage> packages = PackagesCache.GetItem(true);
            if (packages != null && (exactMatch = packages.FirstOrDefault(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase))) == null)
            {
                FreetzPackage likelyKey = packages.FirstOrDefault(x => x.Name.StartsWith(input, StringComparison.OrdinalIgnoreCase));
                if (likelyKey != null)
                {
                    lowestStartsWithDifference = Math.Abs(likelyKey.Name.Length - input.Length);
                    startsWithMatch = likelyKey;
                }
                else
                {
                    lowestFuzzyDifference = 1000;
                    foreach (FreetzPackage one in packages)
                    {
                        int result = StringSimilarity.Compare(input, one.Name, true);
                        if (result < lowestFuzzyDifference)
                        {
                            fuzzyMatch = one;
                            lowestFuzzyDifference = result;
                        }
                    }
                }
            }

            if (exactMatch != null)
            {
                Answer(theMessage, exactMatch, false, anchor);
            }
            else if (startsWithMatch != null && lowestStartsWithDifference < 5)
            {
                Answer(theMessage, startsWithMatch, lowestStartsWithDifference > 1, anchor);
            }
            else if (fuzzyMatch != null && lowestFuzzyDifference < 4)
            {
                Answer(theMessage, fuzzyMatch, true, anchor);
            }
            else if (startsWithMatch != null)
            {
                Answer(theMessage, startsWithMatch, true, anchor);
            }
            else
            {
                theMessage.Answer("Ich habe kein solches Paket gefunden");
            }
        }

        private static void Answer(IrcMessage theMessage, FreetzPackage package, bool isAmbiguous, string anchor)
        {
            if (String.IsNullOrWhiteSpace(package.Url))
            {
                theMessage.Answer($"Das Paket {package.Name} existiert, hat jedoch keine Detailseite");
                return;
            }

            string answer = isAmbiguous ? "Meintest du " : "Freetz Paket ";
            answer += package.Name;
            if (isAmbiguous)
            {
                answer += " ?";
            }
            answer += ": " + package.Url + anchor;

            theMessage.Answer(answer);

        }

        private static List<FreetzPackage> GetPackages(List<FreetzPackage> Alte)
        {
            try
            {
                IDocument document = BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(PackagesPage).Result;
                return document.QuerySelectorAll<IHtmlAnchorElement>("table.wiki a.wiki").Select(x => new FreetzPackage
                {
                    Name = x.Text,
                    Url = x.HrefOrNull()
                }).ToList();
            }
            catch
            {
                return Alte;
            }
        }

        class FreetzPackage
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }
    }
}