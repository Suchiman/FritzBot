using AngleSharp;
using AngleSharp.Dom;
using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
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

        private void Server_OnPostProcessingMessage(object sender, IrcMessage theMessage)
        {
            try
            {
                List<string> links = theMessage.CommandArgs.Where(x => x.StartsWith("http://") || x.StartsWith("https://")).Distinct().ToList();
                foreach (string link in links)
                {
                    WebClient dl = new WebClient();
                    dl.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36");
                    dl.Headers.Add("Accept-Language", "de-de, de, en;q=0.5");
                    dl.DownloadProgressChanged += dl_DownloadProgressChanged;
                    dl.DownloadDataCompleted += dl_DownloadDataCompleted;
                    dl.DownloadDataAsync(new Uri(link), theMessage);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fehler beim Downloaden der Webseite");
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
                IDocument doc = BrowsingContext.New().OpenAsync(x => x.Content(new MemoryStream(e.Result), true)).Result;

                string title = Regex.Replace(doc.Title.Replace("\n", "").Replace("\r", "").Replace("â€“", "–"), "[ ]{2,}", " ");
                if (!String.IsNullOrWhiteSpace(title))
                {
                    (e.UserState as IrcMessage).Answer("[url] " + title);
                }
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