using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using HtmlAgilityPack;
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

        private DataCache<Dictionary<string, string>> PackagesCache = new DataCache<Dictionary<string, string>>(GetPackages, 30);

        public void Run(ircMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                theMessage.Answer(PackagesPage);
                return;
            }

            int lowestDifference = 0;
            string input = theMessage.CommandLine.ToLower();
            string sharpSplit = String.Empty;
            if (input.Contains('#'))
            {
                string[] split = input.Split(new[] { '#' }, 2);
                Contract.Assume(split.Length == 2);
                sharpSplit = "#" + split[1];
                input = split[0];
            }
            int inputLength = input.Length;
            string PackageUrl = null, PackageName = input;

            Dictionary<string, string> packages = PackagesCache.GetItem(true);
            if (packages != null)
            {
                if (!packages.TryGetValue(input, out PackageUrl) || String.IsNullOrEmpty(PackageUrl))
                {
                    string likelyKey = packages.Keys.FirstOrDefault(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase));
                    if (likelyKey != null)
                    {
                        PackageUrl = packages[likelyKey];
                        PackageName = likelyKey;
                        lowestDifference = Math.Abs(likelyKey.Length - inputLength);
                    }
                    else
                    {
                        lowestDifference = 1000;
                        foreach (KeyValuePair<string, string> one in packages)
                        {
                            int result = StringSimilarity.Compare(input, one.Key, true);
                            if (result < lowestDifference)
                            {
                                PackageUrl = one.Value;
                                PackageName = one.Key;
                                lowestDifference = result;
                            }
                        }
                    }
                }
            }
            if (!String.IsNullOrEmpty(PackageUrl))
            {
                theMessage.Answer(String.Format("{0} {1}: http://freetz.org{2}{3}", lowestDifference > 1 ? "Meinten Sie?" : "Freetz Paket", PackageName, PackageUrl, sharpSplit));
            }
            else
            {
                theMessage.Answer("Da ist etwas fürchterlich schief gelaufen");
            }
        }

        private static Dictionary<string, string> GetPackages(Dictionary<string, string> Alte)
        {
            try
            {
                HtmlNode document = new HtmlDocument().LoadUrl(PackagesPage).DocumentNode;
                return document.SelectNodes("//table[@class='wiki']/tr/td/a[@class='wiki']").Distinct(x => x.InnerText).ToDictionary(k => k.InnerText, v => v.GetAttributeValue("href", ""), StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return Alte;
            }
        }
    }
}