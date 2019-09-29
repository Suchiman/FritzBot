using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private CancellationTokenSource? newsthread;

        public void Start()
        {
            newsthread = new CancellationTokenSource();
            Task.Run(() => NewsThread(newsthread.Token), newsthread.Token);
        }

        public void Stop()
        {
            newsthread?.Cancel();
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
                    Log.Warning("Es waren keine referenz News vorhanden, verwende News aus diesem Poll als Referenz f�r den n�chsten Poll");
                    news = updatedNews;
                    continue;
                }

                var maxOld = DateTime.Today.AddDays(-1);
                List<NewsEntry> newNews = updatedNews.Where(x => x.Datum > maxOld).Except(news.Where(x => x.Datum > maxOld)).ToList();
                if (newNews.Count > 0)
                {
                    if (newNews.Count == news.Count)
                    {
                        Log.Warning("Anzahl neuer News entspricht anzahl News insgesamt. �berspringe Meldung auf verdacht");
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
            IDocument? document = null;
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
                    Debug.Fail("Meta-Info struktur ge�ndert");
                    Log.Warning("Meta-Info struktur ge�ndert, Count {Count}", metaInfos.Count);
                    return null;
                }

                entry.Version = metaInfos.SkipWhile(m => m.TextContent != "Version:").ElementAtOrDefault(1)?.TextContent.Trim();
                string? rawDateString = metaInfos.SkipWhile(m => m.TextContent != "Datum:").ElementAtOrDefault(1)?.TextContent.Trim();
                if (!DateTime.TryParseExact(rawDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    parsed = DateTime.MinValue;
                    Log.Warning("Fehler beim Parsen der DateTime {DateTime}", rawDateString);
                }
                entry.Datum = parsed;

                return entry;
            }).NotNull().ToList();
        }
    }

    class NewsEntry : IEquatable<NewsEntry>
    {
        public string? Titel { get; set; }
        public string? Version { get; set; }
        public DateTime Datum { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as NewsEntry);
        }

        public bool Equals(NewsEntry? other)
        {
            return other is object && Titel == other.Titel && Datum == other.Datum && Version == other.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Titel, Version, Datum);
        }

        public override string? ToString()
        {
            return Titel;
        }

        public static bool operator ==(NewsEntry lhs, NewsEntry rhs)
        {
            return Object.ReferenceEquals(lhs, rhs) || lhs?.Equals(rhs) == true;
        }

        public static bool operator !=(NewsEntry lhs, NewsEntry rhs)
        {
            return !Object.ReferenceEquals(lhs, rhs) && lhs?.Equals(rhs) == false;
        }
    }
}