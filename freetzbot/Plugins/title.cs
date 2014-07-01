using CsQuery;
using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Name("title")]
    [Help("Ruft den Titel der Webseite ab")]
    [ParameterRequired]
    class title : PluginBase, IBackgroundTask
    {
        public void Start()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnPostProcessingMessage -= Server_OnPostProcessingMessage;
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
                CQ doc = CQ.Create(new MemoryStream(e.Result));
                CQ titleNode = doc.Select("title");
                if (!titleNode.Any())
                {
                    return;
                }
                string title = Regex.Replace(titleNode.Text().Trim().Replace("\n", "").Replace("\r", "").Replace("â€“", "–"), "[ ]{2,}", " ");
                (e.UserState as ircMessage).Answer("[url] " + title);
            }
            catch
            {
            }
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