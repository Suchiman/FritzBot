using FritzBot.Core;
using FritzBot.DataModel;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Module.Name("title")]
    [Module.Help("Ruft den Titel der Webseite ab")]
    [Module.ParameterRequired]
    class title : PluginBase, IBackgroundTask
    {
        public void Start()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            Server.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            Server.OnPostProcessingMessage -= Server_OnPostProcessingMessage;
        }

        private void Server_OnPostProcessingMessage(object sender, ircMessage theMessage)
        {
            try
            {
                List<string> links = theMessage.CommandArgs.Where(x => x.StartsWith("http://") || x.StartsWith("https://")).Distinct().ToList();
                foreach (string link in links)
                {
                    WebClient dl = new WebClient();
                    dl.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36");
                    dl.DownloadProgressChanged += dl_DownloadProgressChanged;
                    dl.DownloadDataCompleted += dl_DownloadDataCompleted;
                    dl.DownloadDataAsync(new Uri(link), theMessage);
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging(ex);
            }
        }

        void dl_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null || e.Result == null)
            {
                return;
            }
            try
            {
                string html;
                if (FritzBot.Functions.UTF8Checker.IsUtf8(e.Result))
                {
                    html = Encoding.UTF8.GetString(e.Result);
                }
                else
                {
                    html = Encoding.GetEncoding("iso-8859-1").GetString(e.Result);
                }
                HtmlNode doc = HtmlDocumentExtensions.GetHtmlNode(html);
                HtmlNode titleNode = doc.SelectSingleNode("//head/title");
                if (titleNode == null)
                {
                    titleNode = doc.SelectSingleNode("//title");
                    if (titleNode == null)
                    {
                        return;
                    }
                }
                string title = Regex.Replace(titleNode.InnerText.Trim().Replace("\n", "").Replace("\r", "").Replace("â€“", "–"), "[ ]{2,}", " ");
                (e.UserState as ircMessage).Answer("[url] " + HtmlEntity.DeEntitize(title));
            }
            catch { }
        }

        private void dl_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (Math.Max(e.TotalBytesToReceive, e.BytesReceived) > 1048576)
            {
                ((WebClient)sender).CancelAsync();
            }
        }
    }
}