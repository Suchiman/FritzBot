using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace FritzBot.webpages
{
    class rss : IWebInterface
    {
        public string Url { get { return "/rss"; } }
        const string RFC822Format = "ddd',' dd MMM yyyy HH':'mm':'ss 'GMT'";
        public HtmlResponse GenPage(HtmlRequest request)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            HtmlResponse resp = new HtmlResponse();
            resp.status_code = 200;
            resp.content_type = "application/rss+xml; charset=" + Encoding.UTF8.WebName;
            XDocument doc = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"),
                new XElement("rss", new XAttribute("version", "2.0"),
                    new XElement("channel",
                        new XElement("title", "FritzBot News Feed"),
                        new XElement("link", webinterface.Address),
                        new XElement("description", "Newsfeed vom FritzBot"),
                        new XElement("language", "de-de"),
                        new XElement("pubDate", DateTime.Now.ToString(RFC822Format))
                    )
                )
            );
            IEnumerable<XElement> notifications = XMLStorageEngine.GetManager().GetGlobalSettingsStorage("NotificationHistory").Storage.Elements("Notification");
            if (request.getdata.ContainsKey("user") && UserManager.GetInstance().Exists(request.getdata["user"]))
            {
                XElement Subscriptions = UserManager.GetInstance()[request.getdata["user"]].GetModulUserStorage("subscribe").GetElement("Subscriptions", false);
                if (Subscriptions != null)
                {
                    List<String> plugins = Subscriptions.Elements("Plugin").Where(x => x.Attribute("Provider") != null && x.Attribute("Provider").Value == "RSSSubscriptionProvider").Select(x => x.Value).ToList();
                    notifications = notifications.Where(x => x.Attribute("Plugin") != null && plugins.Contains(x.Attribute("Plugin").Value));
                }
            }
            foreach (XElement notification in notifications)
            {
                XElement channel = doc.Descendants("channel").First();
                channel.Add(
                    new XElement("item",
                        new XElement("title", notification.Attribute("Plugin").Value),
                        new XElement("description", notification.Value),
                        new XElement("author", "FritzBot"),
                        new XElement("pubDate", DateTime.Parse(notification.Attribute("Created").Value).ToLocalTime().ToString(RFC822Format))
                    )
                );
            }
            resp.page = doc.ToStringWithDeclaration();
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            return resp;
        }
    }
}