using FritzBot.Core;
using FritzBot.DataModel;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FritzBot.Plugins
{
    [Module.Name("news")]
    [Module.Help("Checkt die AVM Firmware News Webseite und gibt beim Fund neuer Nachrichten eine entsprechende Meldung aus")]
    [Module.Subscribeable]
    class news : PluginBase, IBackgroundTask
    {
        private Thread newsthread;

        public void Start()
        {
            newsthread = new Thread(new ThreadStart(NewsThread));
            newsthread.Name = "NewsThread";
            newsthread.IsBackground = true;
            newsthread.Start();
        }

        public void Stop()
        {
            newsthread.Abort();
        }

        private void NewsThread()
        {
            const String baseurl = "http://webgw.avm.de/download/UpdateNews.jsp";
            String output = string.Empty;
            List<NewsEntry> NewsDE = GetNews(baseurl + "?lang=de").ToList();
            List<NewsEntry> NewsEN = GetNews(baseurl + "?lang=en").ToList();
            while (true)
            {
                Thread.Sleep(Convert.ToInt32(PluginStorage.GetVariable("Intervall", "300")) * 1000);
                List<NewsEntry> NewsDENew = GetNews(baseurl + "?lang=de").ToList();
                List<NewsEntry> NewsENNew = GetNews(baseurl + "?lang=en").ToList();
                String[] DiffDE = NewsDENew.Where(x => !NewsDE.Contains(x)).Select(x => x.Titel).ToArray();
                String[] DiffEN = NewsENNew.Where(x => !NewsEN.Contains(x)).Select(x => x.Titel).ToArray();
                if (DiffDE.Length > 0)
                {
                    output = "Neue Deutsche News: " + String.Join(", ", DiffDE);
                }
                if (DiffEN.Length > 0)
                {
                    if (output != String.Empty)
                    {
                        output += ", ";
                    }
                    output += "Neue Englische News: " + String.Join(", ", DiffEN);
                }
                if (output != String.Empty)
                {
                    ServerManager.GetInstance().AnnounceGlobal(output);
                    NotifySubscribers(output);
                }
                output = String.Empty;
            }
        }

        private IEnumerable<NewsEntry> GetNews(String Url)
        {
            return new HtmlDocument().LoadUrl(Url).DocumentNode.StripComments().SelectNodes("//table[@width='100%'][@border='0'][@cellpadding='0'][@cellspacing='0'][@bgcolor='F6F6F6']").Select(x => new NewsEntry(x));
        }
    }

    class NewsEntry
    {
        public String Titel;
        public String Version;
        public DateTime Datum;

        public NewsEntry(HtmlNode node)
        {
            Titel = HtmlEntity.DeEntitize(node.SelectSingleNode(".//span[@class='uberschriftblau']").InnerText).Trim();
            String[] SuperInfos = node.SelectNodes(".//table[@width='100%'][@cellpadding='10'][@cellspacing='0'][@bgcolor='#FFFFFF']//table//tr[3]//td[@nowrap=''][@class='newsfont']").Select(x => x.InnerText.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
            Version = SuperInfos[0];
            Datum = DateTime.Parse(SuperInfos[1]);
        }

        public override bool Equals(object obj)
        {
            if (obj is NewsEntry)
            {
                return (this.Titel == (obj as NewsEntry).Titel) && (this.Datum == (obj as NewsEntry).Datum) && (this.Version == (obj as NewsEntry).Version);
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