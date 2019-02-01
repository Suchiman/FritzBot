using AngleSharp;
using AngleSharp.Dom;
using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FritzBot.Plugins
{
    [Name("title")]
    [Help("Ruft den Titel der Webseite ab")]
    [ParameterRequired]
    class title : PluginBase, IBackgroundTask
    {
        private static readonly HttpClient Client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 6.3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36" },
                { "Accept-Language", "de-de, de, en;q=0.5" }
            },
            MaxResponseContentBufferSize = 1 * 1024 * 1024
        };

        public void Start()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnPostProcessingMessage -= Server_OnPostProcessingMessage;
        }

        private async void Server_OnPostProcessingMessage(object sender, IrcMessage theMessage)
        {
            if (theMessage.IsIgnored)
            {
                return;
            }

            try
            {
                List<string> links = theMessage.CommandArgs.Where(x => x.StartsWith("http://") || x.StartsWith("https://")).Distinct().ToList();
                if (links.Count == 0)
                {
                    return;
                }

                var tasks = new List<Task<HttpResponseMessage>>(links.Count);
                foreach (string link in links)
                {
                    Uri address = new Uri(link);
                    if (IPAddress.TryParse(address.DnsSafeHost, out var ip) && IsInternal(ip))
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {
                            var addresses = Dns.GetHostAddresses(address.DnsSafeHost);
                            if (addresses.Any(IsInternal))
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    tasks.Add(Client.GetAsync(address, HttpCompletionOption.ResponseContentRead));
                }

                if (tasks.Count == 0)
                {
                    return;
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch
                {
                }

                foreach (var task in tasks)
                {
                    try
                    {
                        var response = await task;
                        var stream = await response.Content.ReadAsStreamAsync();
                        IDocument doc = await BrowsingContext.New().OpenAsync(x => x.Content(stream, true));

                        string title = Regex.Replace(doc.Title.Replace("\n", "").Replace("\r", "").Replace("â€“", "–"), "[ ]{2,}", " ");
                        if (!String.IsNullOrWhiteSpace(title))
                        {
                            theMessage.Answer("[url] " + title);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fehler beim Downloaden der Webseite");
            }
        }

        private static bool IsInternal(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv6SiteLocal || address.IsIPv6Multicast || address.IsIPv6LinkLocal)
                {
                    return true;
                }

                var addressAsString = address.ToString();
                var firstWord = addressAsString.AsSpan(0, addressAsString.IndexOf(':'));

                return firstWord.Length >= 4 && (firstWord.StartsWith("fc") || firstWord.StartsWith("fd")) || firstWord == "100";
            }

            byte[] ip = address.GetAddressBytes();
            switch (ip[0])
            {
                case 10:
                case 127:
                    return true;
                case 172:
                    return ip[1] >= 16 && ip[1] < 32;
                case 192:
                    return ip[1] == 168;
                default:
                    return false;
            }
        }
    }
}