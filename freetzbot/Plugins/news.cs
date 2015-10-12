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
using System.Linq;
using System.Threading;

namespace FritzBot.Plugins
{
    [Name("news")]
    [Help("Checkt die AVM Firmware News Webseite und gibt beim Fund neuer Nachrichten eine entsprechende Meldung aus")]
    [Subscribeable]
    class news : PluginBase, IBackgroundTask
    {
        private Thread newsthread;

        public void Start()
        {
            newsthread = toolbox.SafeThreadStart("NewsThread", true, NewsThread);
        }

        public void Stop()
        {
            newsthread.Abort();
        }

        private void NewsThread()
        {
            const string newsUrl = "http://avm.de/service/downloads/update-news/";

            List<NewsEntry> news = GetNews(newsUrl);
            while (true)
            {
                Thread.Sleep(ConfigHelper.GetInt("NewsCheckIntervall", 300) * 1000);

                List<NewsEntry> updatedNews = GetNews(newsUrl);
                if (updatedNews.Count == 0)
                {
                    Log.Warning("Keine News gefunden");
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

        private static List<NewsEntry> GetNews(string Url)
        {
            Contract.Requires(Url != null);
            Contract.Ensures(Contract.Result<List<NewsEntry>>() != null);

            IDocument document = null;
            for (int i = 1; ; i++)
            {
                try
                {
                    document = BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(Url).Result;
                    break;
                }
                catch (Exception ex)
                {
                    if (i == 1 || i % 100 == 0)
                    {
                        Log.Error(ex, "Fehler beim Laden der News, Versuch {Try}", i);
                    }
                    Thread.Sleep(5000);
                }
            }

            return document.QuerySelectorAll<IHtmlDivElement>("div.entrylist > div.entry").Select(x =>
            {
                NewsEntry entry = new NewsEntry();

                entry.Titel = x.QuerySelector<IHtmlDivElement>("div.headline")?.Children.FirstOrDefault()?.TextContent?.Trim();
                List<IHtmlDivElement> metaInfos = x.QuerySelectorAll<IHtmlDivElement>("div.meta-infos > div.row > div.cell").ToList();
                if (metaInfos.Count != 6)
                {
                    Debug.Fail("Meta-Info struktur geändert");
                    Log.Warning("Meta-Info struktur geändert, Count {Count}", metaInfos.Count);
                    return null;
                }
                entry.Version = metaInfos[3].TextContent.Trim();
                entry.Datum = DateTime.Parse(metaInfos[5].TextContent.Trim());

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