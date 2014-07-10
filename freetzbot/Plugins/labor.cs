using CsQuery;
using CsQuery.Implementation;
using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Text.RegularExpressions;
using System.Threading;

namespace FritzBot.Plugins
{
    [Name("labor")]
    [Help("Gibt Informationen zu den aktuellen Labor Firmwares aus: !labor <boxnummer>")]
    [Subscribeable]
    class labor : PluginBase, ICommand, IBackgroundTask
    {
        public const string BaseUrl = "http://avm.de/fritz-labor/";
        private DataCache<List<Labordaten>> LaborDaten = null;
        private Thread laborthread;

        public labor()
        {
            LaborDaten = new DataCache<List<Labordaten>>(UpdateLaborCache, 60);
        }

        public void Start()
        {
            laborthread = toolbox.SafeThreadStart(PluginID, true, LaborCheck);
        }

        public void Stop()
        {
            laborthread.Abort();
        }

        protected override IQueryable<Subscription> GetSubscribers(BotContext context, string[] criteria)
        {
            if (criteria != null && criteria.Length > 0)
            {
                return base.GetSubscribers(context, criteria).Where(x => criteria.Any(c => x.Bedingungen.Any(a => a.Bedingung.Contains(c))));
            }
            return base.GetSubscribers(context, criteria);
        }

        public void Run(ircMessage theMessage)
        {
            List<Labordaten> daten = LaborDaten.GetItem(true);

            if (LaborDaten.Renewed == DateTime.MinValue)
            {
                theMessage.Answer("Ich konnte leider keine Daten von der Laborwebseite abrufen und mein Cache ist leer");
                return;
            }
            if (!LaborDaten.IsUpToDate)
            {
                theMessage.Answer("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + LaborDaten.LastUpdateFail.Message + ". Verwende Cache vom " + LaborDaten.Renewed.ToString());
            }

            if (String.IsNullOrEmpty(theMessage.CommandLine))
            {
                theMessage.Answer("Aktuelle Labor Daten: " + daten.Select(x => String.Format("{0}: {1}", x.typ, x.datum)).Join(", ") + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
            }
            else
            {
                string BoxName = BoxDatabase.GetShortName(theMessage.CommandLine);
                Labordaten first = daten.FirstOrDefault(x => x.typ == BoxName);
                if (first != null)
                {
                    theMessage.Answer(String.Format("Die neueste {0} labor Version ist am {1} erschienen mit der Versionsnummer: {2} - Laborseite: {3}", first.typ, first.datum, first.version, first.url));
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
                if (ConfigHelper.GetBoolean("LaborCheckEnabled", true))
                {
                    List<Labordaten> neue = null;
                    while (!TryGetNewestLabors(out neue))
                    {
                        Thread.Sleep(1000);
                    }
                    List<Labordaten> unEquals = GetDifferentLabors(alte, neue);
                    if (unEquals.Count > 0)
                    {
                        string labors = "Neue Labor Versionen gesichtet! - " + unEquals.Select(x => String.Format("{0} ({1})", x.typ, x.version)).Join(", ") + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
                        ServerManager.AnnounceGlobal(labors);
                        NotifySubscribers(labors);
                        alte = neue;
                    }
                    Thread.Sleep(ConfigHelper.GetInt("LaborCheckIntervall", 300000));
                }
                else
                {
                    Thread.Sleep(30000);
                }
            }
        }

        private List<Labordaten> GetFTPBetas()
        {
            List<Tuple<Labordaten, string>> ftpBetas = new List<Tuple<Labordaten, string>>();

            if (!Directory.Exists("betaCache"))
                Directory.CreateDirectory("betaCache");

            using (FtpClient ftp = new FtpClient())
            {
                ftp.Host = "ftp.avm.de";
                ftp.Credentials = new NetworkCredential("anonymous", "");
                ftp.SetWorkingDirectory("/fritz.box/beta");

                List<FtpListItem> files = ftp.GetListing().Where(x => x.Type == FtpFileSystemObjectType.File).ToList();
                foreach (FtpListItem file in files)
                {
                    Labordaten daten = new Labordaten();
                    daten.datum = (file.Modified == DateTime.MinValue && ftp.HasFeature(FtpCapability.MDTM) ? ftp.GetModifiedTime(file.FullName) : file.Modified).ToString("dd.MM.yyyy HH:mm:ss");
                    daten.url = "ftp://ftp.avm.de" + file.FullName;

                    string target = Path.Combine(Environment.CurrentDirectory, "betaCache", file.Name);
                    if (!File.Exists(target))
                    {
                        using (Stream f = ftp.OpenRead(file.Name))
                        using (FileStream fi = File.Create(target))
                            f.CopyTo(fi);
                    }

                    ftpBetas.Add(new Tuple<Labordaten, string>(daten, target));
                }
            }

            foreach (Tuple<Labordaten, string> fw in ftpBetas)
            {
                using (Stream file = File.OpenRead(fw.Item2))
                using (ZipArchive archive = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry firmware = archive.Entries.FirstOrDefault(x => x.Name.Contains("_Labor."));
                    if (firmware == null)
                    {
                        toolbox.LogFormat("Firmware {0} hat keine erkannte Labor Firmware", fw.Item2);
                        continue;
                    }

                    string RawName = firmware.Name;

                    fw.Item1.version = Regex.Match(RawName, @"_Labor.((\d{2,3}\.)?\d\d\.\d\d(-\d{1,6})?).image$").Groups[1].Value;
                    if (!BoxDatabase.TryGetShortName(RawName, out fw.Item1.typ))
                    {
                        if (RawName.LastIndexOf(' ') != -1)
                        {
                            fw.Item1.typ = RawName.Substring(RawName.LastIndexOf(' ')).Trim();
                        }
                        else
                        {
                            fw.Item1.typ = RawName.Trim();
                        }
                    }
                }
            }

            return ftpBetas.Select(x => x.Item1).ToList();
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
            Contract.Requires(alte != null && neue != null);
            Contract.Ensures(Contract.Result<List<Labordaten>>() != null);

            return neue.Where(neu => !alte.Any(alt => alt == neu)).ToList();
        }

        private List<Labordaten> UpdateLaborCache(List<Labordaten> AlteLaborDaten)
        {
            CQ LaborStartSeite = CQ.CreateFromUrl(labor.BaseUrl);
            CQ Labors = LaborStartSeite.Select("#content-section").Find("div.csc-space-after-1").Find("div.csc-default");
            List<Labordaten> NeueLaborDaten = Labors.Find("a.button-link").Select(x => x.GetAttribute("href")).Where(x => x != null && x.StartsWith("fritz-labor/")).SelectMany(x => Labordaten.GetDaten(x)).ToList();
            NeueLaborDaten.AddRange(GetFTPBetas());

            if (NeueLaborDaten.Count > 0)
            {
                return NeueLaborDaten;
            }
            return AlteLaborDaten;
        }
    }

    class Labordaten : IEquatable<Labordaten>
    {
        public string typ;
        public string datum;
        public string version;
        public string url;

        const string KeywordVersion = "Version: ";
        const string KeywordDatum = "Datum: ";

        public static IEnumerable<Labordaten> GetDaten(string url)
        {
            CQ LaborSeite = CQ.CreateFromUrl("http://avm.de/" + url);
            List<string> details = LaborSeite.Select("p:contains('Downloadinformationen:')")[0].ChildNodes.OfType<DomText>().Select(x => x.NodeValue.Trim()).ToList();

            Labordaten daten = new Labordaten();
            string RawTyp = LaborSeite.Select("#pagetitle").Text();
            if (!BoxDatabase.TryGetShortName(RawTyp, out daten.typ))
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
            daten.version = details.Where(x => x.StartsWith(KeywordVersion)).Select(x => x.Substring(KeywordVersion.Length)).FirstOrDefault();
            daten.datum = details.Where(x => x.StartsWith(KeywordDatum)).Select(x => x.Substring(KeywordDatum.Length)).FirstOrDefault();
            daten.url = "http://avm.de/" + url;
            yield return daten;
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
            if ((object)labordaten1 != null)
            {
                return labordaten1.Equals(labordaten2);
            }
            if ((object)labordaten2 != null)
            {
                return false;
            }
            return true;
        }

        public static bool operator !=(Labordaten labordaten1, Labordaten labordaten2)
        {
            if ((object)labordaten1 != null)
            {
                return !labordaten1.Equals(labordaten2);
            }
            if ((object)labordaten2 != null)
            {
                return true;
            }
            return false;
        }
    }
}