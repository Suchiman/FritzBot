using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentFTP;
using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using FritzBot.Functions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FritzBot.Plugins
{
    [Name("labor")]
    [Help("Gibt Informationen zu den aktuellen Labor Firmwares aus: !labor <boxnummer>")]
    [Subscribeable]
    class labor : PluginBase, ICommand//, IBackgroundTask
    {
        public const string BaseUrl = "http://avm.de/fritz-labor/";
        private readonly DataCache<List<Labordaten>> LaborDaten;
        private CancellationTokenSource? laborCancellation;

        public labor()
        {
            LaborDaten = new DataCache<List<Labordaten>>(UpdateLaborCache, TimeSpan.FromMinutes(60));
        }

        public void Start()
        {
            laborCancellation = new CancellationTokenSource();
            Task.Run(() => LaborCheck(laborCancellation.Token), laborCancellation.Token);
        }

        public void Stop()
        {
            laborCancellation?.Cancel();
            laborCancellation = null;
        }

        protected override IQueryable<Subscription> GetSubscribers(BotContext context, string[] criteria)
        {
            if (criteria.Length > 0)
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
                theMessage.Answer("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + LaborDaten.LastUpdateFail?.Message + ". Verwende Cache vom " + LaborDaten.Renewed.ToString());
            }

            if (String.IsNullOrEmpty(theMessage.CommandLine))
            {
                theMessage.Answer("Aktuelle Labor Daten: " + daten.Select(x => $"{x.Typ}: {x.Datum}").Join(", ") + " - Zum Labor: " + Toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
            }
            else
            {
                string BoxName = BoxDatabase.GetShortName(theMessage.CommandLine);
                if (daten.FirstOrDefault(x => x.Typ == BoxName) is { } first)
                {
                    theMessage.Answer($"Die neueste {first.Typ} labor Version ist am {first.Datum} erschienen mit der Versionsnummer: {first.Version} - Laborseite: {first.Url}");
                }
                else
                {
                    theMessage.Answer("Eine solche Labor Firmware ist mir nicht bekannt");
                }
            }
        }

        private async Task LaborCheck(CancellationToken token)
        {
            List<Labordaten>? alte;
            while (!TryGetNewestLabors(out alte))
            {
                await Task.Delay(1000, token);
            }
            while (true)
            {
                if (ConfigHelper.GetBoolean("LaborCheckEnabled", true))
                {
                    List<Labordaten>? neue;
                    while (!TryGetNewestLabors(out neue))
                    {
                        await Task.Delay(1000, token);
                    }
                    List<Labordaten> unEquals = GetDifferentLabors(alte, neue);
                    if (unEquals.Count > 0)
                    {
                        string labors = "Neue Labor Versionen gesichtet! - " + unEquals.Select(x => $"{x.Typ} ({x.Version})").Join(", ") + " - Zum Labor: " + Toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
                        ServerManager.AnnounceGlobal(labors);
                        NotifySubscribers(labors);
                        alte = neue;
                    }
                    await Task.Delay(ConfigHelper.GetInt("LaborCheckIntervall", 300000), token);
                }
                else
                {
                    await Task.Delay(30000, token);
                }
            }
        }

        private List<Labordaten> GetFTPBetas()
        {
            var ftpBetas = new List<(Labordaten daten, string target)>();

            var betaCache = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "betaCache");
            if (!Directory.Exists(betaCache))
                Directory.CreateDirectory(betaCache);

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

                    string target = Path.Combine(betaCache, file.Name);
                    if (!File.Exists(target))
                    {
                        using Stream f = ftp.OpenRead(file.Name);
                        using FileStream fi = File.Create(target);
                        f.CopyTo(fi);
                    }

                    ftpBetas.Add((daten, target));
                }
            }

            foreach ((Labordaten daten, string target) in ftpBetas)
            {
                try
                {
                    using (Stream file = File.OpenRead(target))
                    using (ZipArchive archive = new ZipArchive(file, ZipArchiveMode.Read))
                    {
                        ZipArchiveEntry firmware = archive.Entries.FirstOrDefault(x => x.Name.Contains("_Labor.") || x.Name.Contains(".Labor.") || x.Name.Contains("_LabBETA.") || x.Name.Contains(".LabBETA."));
                        if (firmware == null)
                        {
                            Log.Error("Firmware {Firmware} hat keine erkannte Labor Firmware", target);
                            continue;
                        }

                        string RawName = firmware.Name;

                        daten.Version = Regex.Match(RawName, @"((\d{2,3}\.)?\d\d\.\d\d(-\d{1,6})?).image$").Groups[1].Value;
                        if (!BoxDatabase.TryGetShortName(RawName, out string? tmp))
                        {
                            if (RawName.LastIndexOf(' ') != -1)
                            {
                                daten.Typ = RawName.AsSpan(RawName.LastIndexOf(' ')).Trim().ToString();
                            }
                            else
                            {
                                daten.Typ = RawName.Trim();
                            }
                        }
                        else
                        {
                            daten.Typ = tmp;
                        }
                    }
                }
                catch (InvalidDataException ex) //'System.IO.InvalidDataException' in System.IO.Compression.dll("Das Ende des Datensatzes im zentralen Verzeichnis wurde nicht gefunden.")
                {
                    var betaCachePath = Path.GetDirectoryName(target)!;
                    var name = Path.GetFileNameWithoutExtension(target);
                    var extension = Path.GetExtension(target);
                    string corruptedFilePath = Path.Combine(betaCachePath, $"{name}_corrupted_{DateTime.Now:dd_MM_yyyy_hh_mm_ss}{extension}");
                    File.Move(target, corruptedFilePath);
                    Log.Error(ex, "Korruptes Zip {Filename} gefunden. Zip umbenannt zu {NewFilename}", target, corruptedFilePath);
                }
            }

            return ftpBetas.Select(x => x.Item1).ToList();
        }

        private bool TryGetNewestLabors([MaybeNullWhen(false)]out List<Labordaten> daten)
        {
            daten = null!;
            try
            {
                LaborDaten.ForceRefresh(false);
                daten = LaborDaten.GetItem(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<Labordaten> GetDifferentLabors(List<Labordaten> alte, List<Labordaten> neue)
        {
            return neue.Where(neu => !alte.Any(alt => alt == neu)).ToList();
        }

        private const string KeywordFritzOS = "FRITZ!OS ";
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
                string? fallbackDate = null;
                var lastUpdate = link.ParentElement.PreviousElementSibling;
                if (lastUpdate.TextContent.StartsWith(KeywordLetztesUpdate))
                {
                    fallbackDate = lastUpdate.TextContent.Substring(KeywordLetztesUpdate.Length);
                }
                IDocument detailPage = link.NavigateAsync().Result;
                IHtmlParagraphElement downloadInformations = detailPage.QuerySelector<IHtmlParagraphElement>("p:contains('Downloadinformationen:')") ?? detailPage.QuerySelector<IHtmlParagraphElement>("p:contains('Informationen zum Download:')");

                if (downloadInformations == null)
                    return null!;

                Labordaten daten = new Labordaten();
                daten.Typ = detailPage.Title.EndsWith(TitleAVM) ? detailPage.Title.Remove(detailPage.Title.Length - TitleAVM.Length) : detailPage.Title;
                daten.Version = downloadInformations.ChildNodes.Where(x => x.TextContent.StartsWith(KeywordVersion)).Select(x => x.TextContent.Substring(KeywordVersion.Length)).FirstOrDefault();
                daten.Datum = downloadInformations.ChildNodes.Where(x => x.TextContent.StartsWith(KeywordDatum)).Select(x => x.TextContent.Substring(KeywordDatum.Length)).FirstOrDefault() ?? fallbackDate;
                daten.Url = link.Href;
                return daten;
            }).Where(x => x != null!).ToList();

            try
            {
                NeueLaborDaten.AddRange(GetFTPBetas());
            }
            catch (FtpCommandException ex)
            {
                if (ex.Message == "There are too many connections from your internet address.")
                {
                    Log.Warning("Abrufen von FTP Labors wegen zu vielen Connections fehlgeschlagen");
                }
                else
                {
                    Log.Error(ex, "Abrufen von FTP Labors fehlgeschlagen");
                }
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
        public string? Typ { get; set; }
        public string? Datum { get; set; }
        public string? Version { get; set; }
        public string? Url { get; set; }

        public override int GetHashCode()
        {
            return Datum?.GetHashCode() ?? 0 ^ Version?.GetHashCode() ?? 0;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Labordaten);
        }

        public bool Equals(Labordaten? other)
        {
            return other is object && Version == other.Version && Datum == other.Datum && Url == other.Url && Typ == other.Typ;
        }

        public static bool operator ==(Labordaten left, Labordaten right)
        {
            if ((object)left != null)
            {
                return left.Equals(right);
            }
            if ((object)right != null)
            {
                return false;
            }
            return true;
        }

        public static bool operator !=(Labordaten left, Labordaten right)
        {
            if ((object)left != null)
            {
                return !left.Equals(right);
            }
            if ((object)right != null)
            {
                return true;
            }
            return false;
        }
    }
}