using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FritzBot.Plugins
{
    [Name("news")]
    [Help("Checkt die AVM Firmware News Webseite und gibt beim Fund neuer Nachrichten eine entsprechende Meldung aus")]
    [Subscribeable]
    class news : PluginBase, IBackgroundTask
    {
        private CancellationTokenSource newsthread;

        public void Start()
        {
            newsthread = new CancellationTokenSource();
            Task.Run(() => NewsThread(newsthread.Token), newsthread.Token);
        }

        public void Stop()
        {
            newsthread.Cancel();
            newsthread = null;
        }

        private async Task NewsThread(CancellationToken token)
        {
            const string newsUrl = "http://avm.de/service/downloads/update-news/";

            List<NewsEntry> news = await GetNews(newsUrl, token);
            while (true)
            {
                await Task.Delay(ConfigHelper.GetInt("NewsCheckIntervall", 300) * 1000, token);

                List<NewsEntry> updatedNews = await GetNews(newsUrl, token);
                if (updatedNews.Count == 0)
                {
                    Log.Warning("Keine News gefunden");
                    continue;
                }

                if (news.Count == 0)
                {
                    Log.Warning("Es waren keine referenz News vorhanden, verwende News aus diesem Poll als Referenz für den nächsten Poll");
                    news = updatedNews;
                    continue;
                }

                List<NewsEntry> newNews = updatedNews.Except(news).ToList();
                if (newNews.Count > 0)
                {
                    if (newNews.Count == news.Count)
                    {
                        Log.Warning("Anzahl neuer News entspricht anzahl News insgesamt. Überspringe Meldung auf verdacht");
                        news = updatedNews;
                        continue;
                    }

                    string output = $"Neue News: {newNews.Select(x => $"{x.Titel} ({x.Version})").Distinct().Join(", ")} Auf zu den News: {newsUrl}";
                    ServerManager.AnnounceGlobal(output);
                    NotifySubscribers(output);
                }
                news = updatedNews;
            }
        }

        private static async Task<List<NewsEntry>> GetNews(string Url, CancellationToken token)
        {
            Contract.Requires(Url != null);
            Contract.Ensures(Contract.Result<List<NewsEntry>>() != null);

            IDocument document = null;
            for (int i = 1; ; i++)
            {
                try
                {
                    document = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(Url);
                    break;
                }
                catch (Exception ex)
                {
                    if (i == 1 || i % 100 == 0)
                    {
                        Log.Error(ex, "Fehler beim Laden der News, Versuch {Try}", i);
                    }
                    await Task.Delay(5000, token);
                }
            }

            return document.QuerySelectorAll<IHtmlDivElement>("div.entrylist > div.entry").Select(x =>
            {
                NewsEntry entry = new NewsEntry();

                entry.Titel = x.QuerySelector<IHtmlDivElement>("div.headline")?.Children.FirstOrDefault()?.TextContent?.Trim();
                List<IHtmlDivElement> metaInfos = x.QuerySelectorAll<IHtmlDivElement>("div.meta-infos > div.row > div.cell").ToList();
                if (metaInfos.Count % 2 != 0)
                {
                    Debug.Fail("Meta-Info struktur geändert");
                    Log.Warning("Meta-Info struktur geändert, Count {Count}", metaInfos.Count);
                    return null;
                }

                entry.Version = metaInfos.SkipWhile(m => m.TextContent != "Version:").ElementAtOrDefault(1)?.TextContent.Trim();
                string rawDateString = metaInfos.SkipWhile(m => m.TextContent != "Datum:").ElementAtOrDefault(1)?.TextContent.Trim();
                DateTime parsed;
                if (!DateTime.TryParseExact(rawDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    parsed = DateTime.MinValue;
                    Log.Warning("Fehler beim Parsen der DateTime {DateTime}", rawDateString);
                }
                entry.Datum = parsed;

                return entry;
            }).NotNull().ToList();
        }
    }

    class NewsEntry
    {
        public string Titel { get; set; }
        public string Version { get; set; }
        public DateTime Datum { get; set; }

        public override bool Equals(object obj)
        {
            var newsEntry = obj as NewsEntry;
            if (newsEntry is NewsEntry)
            {
                return (Titel == newsEntry.Titel) && (Datum == newsEntry.Datum) && (Version == newsEntry.Version);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Titel.GetHashCode() ^ Version.GetHashCode() ^ Datum.GetHashCode();
        }

        public override string ToString()
        {
            return Titel;
        }

        public static bool operator ==(NewsEntry daten1, NewsEntry daten2)
        {
            return daten1.Equals(daten2);
        }

        public static bool operator !=(NewsEntry daten1, NewsEntry daten2)
        {
            return (daten1.Titel != daten2.Titel) || (daten1.Datum != daten2.Datum) || (daten1.Version != daten2.Version);
        }
    }
}