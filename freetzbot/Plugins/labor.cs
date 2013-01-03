using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FritzBot.Plugins
{
    [Module.Name("labor")]
    [Module.Help("Gibt Informationen zu den aktuellen Labor Firmwares aus: !labor <boxnummer>")]
    [Module.Subscribeable]
    class labor : PluginBase, ICommand, IBackgroundTask
    {
        public const String BaseUrl = "http://www.avm.de/de/Service/Service-Portale/Labor/";
        private DataCache<List<Labordaten>> LaborDaten = null;
        private Thread laborthread;

        public labor()
        {
            LaborDaten = new DataCache<List<Labordaten>>(UpdateLaborCache, 60);
        }

        public void Start()
        {
            laborthread = new Thread(new ThreadStart(this.LaborCheck))
            {
                Name = "LaborThread",
                IsBackground = true
            };
            laborthread.Start();
        }

        public void Stop()
        {
            laborthread.Abort();
        }

        protected override IEnumerable<User> GetSubscribers(String[] criteria)
        {
            if (criteria != null && criteria.Length > 0)
            {
                return base.GetSubscribers(criteria).Where(x => x.GetSubscriptions().Where(a => a.Value == PluginID).Any(y => y.AttributeValueOrEmpty("Bedingung").Split(';').Any(z => criteria.Contains(z))));
            }
            return base.GetSubscribers(criteria);
        }

        public void Run(ircMessage theMessage)
        {
            List<Labordaten> daten = LaborDaten.GetItem(true);

            if (LaborDaten.Renewed == DateTime.MinValue)
            {
                theMessage.Answer("Ich konnte leider keine Daten von der Laborwebseite abrufen und mein Cache ist leer");
                return;
            }
            else if (!LaborDaten.IsUpToDate)
            {
                theMessage.Answer("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + LaborDaten.LastUpdateFail.Message + ". Verwende Cache vom " + LaborDaten.Renewed.ToString());
            }

            if (String.IsNullOrEmpty(theMessage.CommandLine))
            {
                theMessage.Answer("Aktuelle Labor Daten: " + String.Join(", ", daten.Select(x => String.Format("{0}: {1}", x.typ, x.datum)).ToArray()) + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
            }
            else
            {
                String BoxName = BoxDatabase.GetInstance().GetShortName(theMessage.CommandLine);
                Labordaten first = daten.FirstOrDefault(x => x.typ == BoxName);
                if (first != null)
                {
                    theMessage.Answer(String.Format("Die neueste {0} labor Version ist am {1} erschienen mit der Versionsnummer: {2} - Laborseite: {3}", first.typ, first.datum, first.version, toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/" + first.url)));
                }
                else
                {
                    theMessage.Answer("Eine solche Labor Firmware ist mir nicht bekannt");
                }
            }
        }

        private void LaborCheck()
        {
            List<Labordaten> alte = null;
            while (!TryGetNewestLabors(out alte))
            {
                Thread.Sleep(1000);
            }
            while (true)
            {
                if (PluginStorage.GetVariable("CheckEnabled", "true") == "true")
                {
                    List<Labordaten> neue = null;
                    while (!TryGetNewestLabors(out neue))
                    {
                        Thread.Sleep(1000);
                    }
                    List<Labordaten> unEquals = GetDifferentLabors(alte, neue);
                    if (unEquals.Count > 0)
                    {
                        String labors = "Neue Labor Versionen gesichtet! - " + String.Join(", ", unEquals.Select(x => x.typ).ToArray()) + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
                        ServerManager.GetInstance().AnnounceGlobal(labors);
                        NotifySubscribers(labors);
                        alte = neue;
                    }
                    Thread.Sleep(Convert.ToInt32(PluginStorage.GetVariable("Intervall", "300000")));
                }
                else
                {
                    Thread.Sleep(30000);
                }
            }
        }

        private bool TryGetNewestLabors(out List<Labordaten> Daten)
        {
            Daten = null;
            try
            {
                LaborDaten.ForceRefresh(false);
                Daten = LaborDaten.GetItem(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<Labordaten> GetDifferentLabors(List<Labordaten> alte, List<Labordaten> neue)
        {
            List<Labordaten> veränderte = new List<Labordaten>();
            foreach (Labordaten neu in neue)
            {
                bool matching = false;
                foreach (Labordaten alt in alte)
                {
                    if (alt == neu)
                    {
                        matching = true;
                    }
                }
                if (!matching)
                {
                    veränderte.Add(neu);
                }
            }
            return veränderte;
        }

        private List<Labordaten> UpdateLaborCache(List<Labordaten> AlteLaborDaten)
        {
            HtmlNode LaborStartSeite = new HtmlDocument().LoadUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php").DocumentNode.StripComments();

            List<Labordaten> NeueLaborDaten = LaborStartSeite.Descendants("h2").First().Siblings().SelectMany(x => x.Descendants("a")).Where(x => x.ChildAttributes("href").Count() == 1).Where(x => x.ChildAttributes("style").Count() == 0 && !x.ChildAttributes("href").First().Value.StartsWith("..")).SelectMany(x => Labordaten.GetDaten(x.ChildAttributes("href").First().Value.Trim())).ToList();

            if (NeueLaborDaten.Count > 0)
            {
                return NeueLaborDaten;
            }
            else
            {
                return AlteLaborDaten;
            }
        }
    }

    class Labordaten : IEquatable<Labordaten>
    {
        public String typ;
        public String datum;
        public String version;
        public String url;

        public Labordaten() { }

        public static IEnumerable<Labordaten> GetDaten(String url)
        {
            HtmlNode LaborSeite = new HtmlDocument().LoadUrl(labor.BaseUrl + url).DocumentNode.StripComments();

            IEnumerable<IGrouping<String, HtmlNode>> node = LaborSeite.SelectSingleNode("//div[@id='effect']").Descendants().Where(x => x.GetAttributeValue("id", "").Length > 2).GroupBy(x => x.GetAttributeValue("id", "").Substring(2));
            foreach (IGrouping<String, HtmlNode> single in node)
            {
                HtmlNodeCollection table = single.First(x => x.Name == "div").SelectNodes("./table[@style=\"text-align:left; width:350px; float:left;\"]/tr[2]/td/text()");
                String RawTyp = single.First(x => x.Name == "h3").InnerText.Trim();
                Labordaten daten = new Labordaten();
                if (!BoxDatabase.GetInstance().TryGetShortName(RawTyp, out daten.typ))
                {
                    if (RawTyp.LastIndexOf(' ') != -1)
                    {
                        daten.typ = RawTyp.Substring(RawTyp.LastIndexOf(' ')).Trim();
                    }
                    else
                    {
                        daten.typ = RawTyp.Trim();
                    }
                }
                daten.version = table[0].InnerText.Trim();
                daten.datum = table[2].InnerText.Trim();
                daten.url = url;
                yield return daten;
            }
        }

        public override int GetHashCode()
        {
            return datum.GetHashCode() ^ version.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Labordaten))
                return false;

            return Equals((Labordaten)obj);
        }

        public bool Equals(Labordaten other)
        {
            return (object)other != null && version == other.version && datum == other.datum && url == other.url && typ == other.typ;
        }

        public static bool operator ==(Labordaten labordaten1, Labordaten labordaten2)
        {
            return labordaten1.Equals(labordaten2);
        }

        public static bool operator !=(Labordaten labordaten1, Labordaten labordaten2)
        {
            return !labordaten1.Equals(labordaten2);
        }
    }
}