using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using FritzBot.Functions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Reflection;
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

        public void Run(IrcMessage theMessage)
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
                theMessage.Answer("Aktuelle Labor Daten: " + daten.Select(x => String.Format("{0}: {1}", x.Typ, x.Datum)).Join(", ") + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
            }
            else
            {
                string BoxName = BoxDatabase.GetShortName(theMessage.CommandLine);
                Labordaten first = daten.FirstOrDefault(x => x.Typ == BoxName);
                if (first != null)
                {
                    theMessage.Answer(String.Format("Die neueste {0} labor Version ist am {1} erschienen mit der Versionsnummer: {2} - Laborseite: {3}", first.Typ, first.Datum, first.Version, first.Url));
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
                        string labors = "Neue Labor Versionen gesichtet! - " + unEquals.Select(x => String.Format("{0} ({1})", x.Typ, x.Version)).Join(", ") + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
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
                    daten.Datum = (file.Modified == DateTime.MinValue && ftp.HasFeature(FtpCapability.MDTM) ? ftp.GetModifiedTime(file.FullName) : file.Modified).ToString("dd.MM.yyyy HH:mm:ss");
                    daten.Url = "ftp://ftp.avm.de" + file.FullName;

                    string target = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "betaCache", file.Name);
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
                try
                {
                    using (Stream file = File.OpenRead(fw.Item2))
                    using (ZipArchive archive = new ZipArchive(file, ZipArchiveMode.Read))
                    {
                        ZipArchiveEntry firmware = archive.Entries.FirstOrDefault(x => x.Name.Contains("_Labor.") || x.Name.Contains(".Labor.") || x.Name.Contains("_LabBETA.") || x.Name.Contains(".LabBETA."));
                        if (firmware == null)
                        {
                            Log.Error("Firmware {Firmware} hat keine erkannte Labor Firmware", fw.Item2);
                            continue;
                        }

                        string RawName = firmware.Name;

                        fw.Item1.Version = Regex.Match(RawName, @"((\d{2,3}\.)?\d\d\.\d\d(-\d{1,6})?).image$").Groups[1].Value;
                        string tmp;
                        if (!BoxDatabase.TryGetShortName(RawName, out tmp))
                        {
                            if (RawName.LastIndexOf(' ') != -1)
                            {
                                fw.Item1.Typ = RawName.Substring(RawName.LastIndexOf(' ')).Trim();
                            }
                            else
                            {
                                fw.Item1.Typ = RawName.Trim();
                            }
                        }
                        else
                        {
                            fw.Item1.Typ = tmp;
                        }
                    }
                }
                catch (InvalidDataException ex) //'System.IO.InvalidDataException' in System.IO.Compression.dll("Das Ende des Datensatzes im zentralen Verzeichnis wurde nicht gefunden.")
                {
                    var betaCachePath = Path.GetDirectoryName(fw.Item2);
                    var name = Path.GetFileNameWithoutExtension(fw.Item2);
                    var extension = Path.GetExtension(fw.Item2);
                    string corruptedFilePath = Path.Combine(betaCachePath, $"{name}_corrupted_{DateTime.Now:dd_MM_yyyy_hh_mm_ss}{extension}");
                    File.Move(fw.Item2, corruptedFilePath);
                    Log.Error(ex, "Korruptes Zip {Filename} gefunden. Zip umbenannt zu {NewFilename}", fw.Item2, corruptedFilePath);
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

        private const string KeywordVersion = "Version: ";
        private const string KeywordDatum = "Datum: ";
        private const string KeywordLetztesUpdate = "Letzte Aktualisierung: ";
        private const string TitleAVM = " | AVM Deutschland";
        private List<Labordaten> UpdateLaborCache(List<Labordaten> AlteLaborDaten)
        {
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            IDocument LaborStartSeite = context.OpenAsync(BaseUrl).Result;
            List<Labordaten> NeueLaborDaten = LaborStartSeite.QuerySelectorAll<IHtmlAnchorElement>("#content-section div.csc-space-after-1 div.csc-default a.button-link").Where(x => x.Href.Contains("/fritz-labor/")).Select(link =>
            {
                string fallbackDate = null;
                var lastUpdate = link.ParentElement.PreviousElementSibling;
                if (lastUpdate.TextContent.StartsWith(KeywordLetztesUpdate))
                {
                    fallbackDate = lastUpdate.TextContent.Substring(KeywordLetztesUpdate.Length);
                }
                IDocument detailPage = link.Navigate().Result;
                IHtmlParagraphElement downloadInformations = detailPage.QuerySelector<IHtmlParagraphElement>("p:contains('Downloadinformationen:')");

                Labordaten daten = new Labordaten();
                daten.Typ = detailPage.Title.EndsWith(TitleAVM) ? detailPage.Title.Remove(detailPage.Title.Length - TitleAVM.Length) : detailPage.Title;
                daten.Version = downloadInformations.ChildNodes.Where(x => x.TextContent.StartsWith(KeywordVersion)).Select(x => x.TextContent.Substring(KeywordVersion.Length)).FirstOrDefault();
                daten.Datum = downloadInformations.ChildNodes.Where(x => x.TextContent.StartsWith(KeywordDatum)).Select(x => x.TextContent.Substring(KeywordDatum.Length)).FirstOrDefault() ?? fallbackDate;
                daten.Url = link.Href;
                return daten;
            }).ToList();

            try
            {
                NeueLaborDaten.AddRange(GetFTPBetas());
            }
            catch (FtpCommandException ex)
            {
                Log.Error(ex, "Abrufen von FTP Labors fehlgeschlagen");
            }

            if (NeueLaborDaten.Count > 0)
            {
                return NeueLaborDaten;
            }
            return AlteLaborDaten;
        }
    }

    class Labordaten : IEquatable<Labordaten>
    {
        public string Typ { get; set; }
        public string Datum { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }

        public override int GetHashCode()
        {
            return Datum.GetHashCode() ^ Version.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var labordaten = obj as Labordaten;
            if (labordaten == null)
                return false;

            return Equals(labordaten);
        }

        public bool Equals(Labordaten other)
        {
            return (object)other != null && Version == other.Version && Datum == other.Datum && Url == other.Url && Typ == other.Typ;
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