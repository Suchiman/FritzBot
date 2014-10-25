using CsQuery;
using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace FritzBot.Plugins
{
    [Name("fm", "ff")]
    [Help("Das erzeugt einen Link zu einem FritzBox Freetz Informationsseite, Beispiel: !fm 7270 v1")]
    class fm : PluginBase, ICommand
    {
        private const string ModelPage = "http://freetz.org/wiki/models";

        private DataCache<Dictionary<string, TableBox>> PackagesCache = new DataCache<Dictionary<string, TableBox>>(GetPackages, 30);

        public void Run(IrcMessage theMessage)
        {
            if (!theMessage.HasArgs)
            {
                theMessage.Answer(ModelPage);
                return;
            }

            string input = theMessage.CommandLine.ToLower();

            bool wildcard = false;

            if (theMessage.CommandArgs.LastOrDefault() == "all")
            {
                input = theMessage.CommandArgs.Take(theMessage.CommandArgs.Count - 1).Join(" ");
                wildcard = true;
            }
            else if (input.EndsWith("*"))
            {
                input = input.TrimEnd('*');
                wildcard = true;
            }

            string sharpSplit = String.Empty;
            if (input.Contains('#'))
            {
                string[] split = input.Split(new[] { '#' }, 2);
                Contract.Assume(split.Length == 2);
                sharpSplit = "#" + split[1];
                input = split[0];
            }
            int inputLength = input.Length;

            TableBox Box = null;
            List<TableBox> Boxen = new List<TableBox>();

            Dictionary<string, TableBox> packages = PackagesCache.GetItem(true);
            if (packages != null)
            {
                if (wildcard || !packages.TryGetValue(input, out Box))
                {
                    List<TableBox> likelyKey = packages.Where(x => x.Key.StartsWith(input, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();
                    if (likelyKey.Count > 0)
                    {
                        Box = likelyKey.FirstOrDefault();
                        Boxen = likelyKey;
                    }
                    else
                    {
                        int lowestDifference = 1000;
                        foreach (KeyValuePair<string, TableBox> one in packages)
                        {
                            int result = StringSimilarity.Compare(input, one.Key, true);
                            if (result < lowestDifference)
                            {
                                Box = one.Value;
                                lowestDifference = result;
                            }
                        }
                    }
                }
                if (!Boxen.Contains(Box))
                {
                    Boxen.Add(Box);
                }
            }

            if (Box == null)
            {
                theMessage.Answer("Da ist etwas fürchterlich schief gelaufen");
                return;
            }

            StringBuilder sb = new StringBuilder("Freetz Modell ");

            if (theMessage.CommandName == "ff")
            {
                sb.Append(Box.Name);

                if (NotEmpty(Box.CPU))
                {
                    sb.Append(", CPU: ");
                    sb.Append(Box.CPU);
                }

                if (NotEmpty(Box.RAM))
                {
                    sb.Append(", RAM: ");
                    sb.Append(Box.RAM);
                }

                if (NotEmpty(Box.Flash))
                {
                    sb.Append(", Flash: ");
                    sb.Append(Box.Flash);
                }

                if (NotEmpty(Box.USBHost))
                {
                    sb.Append(", USB-Host: ");
                    sb.Append(Box.USBHost);
                }

                if (NotEmpty(Box.Annex))
                {
                    sb.Append(", Annex: ");
                    sb.Append(Box.Annex);
                }

                if (NotEmpty(Box.FreetzVersion))
                {
                    sb.Append(", Unterstütze Freetz Version: ");
                    sb.Append(Box.FreetzVersion);
                    if (NotEmpty(Box.AngepassteFirmware))
                    {
                        sb.Append(" (");
                        sb.Append(Box.AngepassteFirmware);
                        sb.Append(")");
                    }
                }

                if (NotEmpty(Box.Sprache))
                {
                    sb.Append(", Sprachen: ");
                    sb.Append(Box.Sprache);
                }

                if (NotEmpty(Box.Url))
                {
                    sb.Append(", Detailseite(n): ");
                    sb.Append(Boxen.Select(x => "http://freetz.org" + x.Url).Join(" , "));
                }
                else
                {
                    sb.Append(", Noch keine Detailseite vorhanden");
                }
            }
            else
            {
                if (Boxen.Count > 0)
                {
                    sb.Append(Boxen.Select(x => x.FreetzType + " http://freetz.org" + x.Url).Join(" , "));
                }
                else
                {
                    sb.Append(Box.FreetzType);
                    sb.Append(" http://freetz.org");
                    sb.Append(Box.Url);
                }
            }

            theMessage.Answer(sb.ToString());
        }

        private static Dictionary<string, TableBox> GetPackages(Dictionary<string, TableBox> Alte)
        {
            try
            {
                CQ document = CQ.CreateFromUrl(ModelPage);
                return document.Select("table.wiki").Find("tr").Where(x => !x.Cq().Find("td").First().Text().In("", " Modell ")).Select(x => new TableBox(x)).Distinct(x => x.FreetzType).ToDictionary(k => k.FreetzType, v => v, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return Alte;
            }
        }

        private static bool NotEmpty(string s)
        {
            return !String.IsNullOrEmpty(s) && s != "-";
        }
    }

    class TableBox
    {
        private static string LastName = null;

        public string Name { get; set; }

        public string Url { get; set; }

        public string FreetzType { get; set; }

        public string AngepassteFirmware { get; set; }

        public string FreetzVersion { get; set; }

        public string Annex { get; set; }

        public string Sprache { get; set; }

        public string CPU { get; set; }

        public string Flash { get; set; }

        public string RAM { get; set; }

        public string USBHost { get; set; }

        public TableBox(IDomObject x)
        {
            List<IDomObject> TableRows = x.Cq().Find("td").ToList();
            Name = TableRows[0].InnerText.Trim();

            if (Name == "+" && !String.IsNullOrEmpty(LastName))
            {
                Name = LastName;
            }

            LastName = Name;

            FreetzType = TableRows[1].Cq().Text().Trim();
            CQ link = TableRows[1].Cq().Find("a");
            if (link.Any())
            {
                Url = link.Attr("href");
            }

            AngepassteFirmware = TableRows[2].InnerText.Trim();

            FreetzVersion = TableRows[3].InnerText.Trim();

            Annex = TableRows[4].InnerText.Trim();

            Sprache = TableRows[5].InnerText.Trim();

            CPU = TableRows[6].InnerText.Trim();

            Flash = TableRows[7].InnerText.Trim();

            RAM = TableRows[8].InnerText.Trim();

            USBHost = TableRows[9].InnerText.Trim();
        }
    }
}