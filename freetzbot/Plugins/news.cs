using CsQuery;
using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
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
            const string baseurl = "http://webgw.avm.de/download/UpdateNews.jsp";
            string output = string.Empty;
            List<NewsEntry> NewsDE = GetNews(baseurl + "?lang=de");
            List<NewsEntry> NewsEN = GetNews(baseurl + "?lang=en");
            while (true)
            {
                Thread.Sleep(ConfigHelper.GetInt("NewsCheckIntervall", 300) * 1000);
                List<NewsEntry> NewsDENew = GetNews(baseurl + "?lang=de");
                List<NewsEntry> NewsENNew = GetNews(baseurl + "?lang=en");
                string[] DiffDE = NewsDENew.Where(x => !NewsDE.Contains(x)).Select(x => x.Titel).Distinct().ToArray();
                string[] DiffEN = NewsENNew.Where(x => !NewsEN.Contains(x)).Select(x => x.Titel).Distinct().ToArray();
                string DiffDEstring = String.Join(", ", DiffDE);
                string DiffENstring = String.Join(", ", DiffEN);
                if (DiffDE.Length > 0 && DiffDEstring == DiffENstring)
                {
                    output = "Neue News: " + DiffDEstring + String.Format(" Auf zu den News: {0}", baseurl);
                }
                else
                {
                    if (DiffDE.Length > 0)
                    {
                        output = "Neue Deutsche News: " + DiffDEstring + String.Format(" Auf zu den DE-News: {0}?lang=de", baseurl); ;
                    }
                    if (DiffEN.Length > 0)
                    {
                        if (output != String.Empty)
                        {
                            output += ", ";
                        }
                        output += "Neue Englische News: " + DiffENstring + String.Format(" Auf zu den EN-News: {0}?lang=en", baseurl); ; ;
                    }
                }
                if (output != String.Empty)
                {
                    ServerManager.AnnounceGlobal(output);
                    NotifySubscribers(output);
                }
                output = String.Empty;
                NewsDE = NewsDENew;
                NewsEN = NewsENNew;
            }
        }

        private List<NewsEntry> GetNews(string Url)
        {
            Contract.Requires(Url != null);
            Contract.Ensures(Contract.Result<List<NewsEntry>>() != null);

            CQ doc = null;
            for (int i = 1; true; i++)
            {
                try
                {
                    doc = CQ.CreateFromUrl(Url);
                    break;
                }
                catch (Exception ex)
                {
                    if (i == 1 || i % 100 == 0)
                    {
                        toolbox.LogFormat("Fehler beim Laden der News, Versuch {0}: {1}", i, ex.Message);
                    }
                    Thread.Sleep(5000);
                }
            }

            return doc.Select("table[bgcolor='F6F6F6']").Select(x => new NewsEntry(x.Cq())).ToList();
        }
    }

    class NewsEntry
    {
        public string Titel;
        public string Version;
        public DateTime Datum;

        public NewsEntry(CQ node)
        {
            Titel = node.Find("span.uberschriftblau").Text().Trim();
            string[] SuperInfos = node.Find("table[bgcolor='#FFFFFF']").Find("table").Find("tr").Skip(2).First().Cq().Find("td.newsfont").Select(x => x.Cq().Text().Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
            if (SuperInfos.Length < 2)
            {
                throw new Exception("Der News Beitrag konnte nicht geparsed werden");
            }
            Version = SuperInfos[1];
            Datum = DateTime.Parse(SuperInfos[3]);
        }

        public override bool Equals(object obj)
        {
            if (obj is NewsEntry)
            {
                return (Titel == (obj as NewsEntry).Titel) && (Datum == (obj as NewsEntry).Datum) && (Version == (obj as NewsEntry).Version);
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