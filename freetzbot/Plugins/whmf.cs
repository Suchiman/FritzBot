using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace FritzBot.Plugins
{
    [Name("whmf", "w")]
    [Help("Das erzeugt einen Link zu wehavemorefun mit dem angegebenen Suchkriterium, Beispiele: !whmf 7270, !whmf CAPI Treiber")]
    class whmf : PluginBase, ICommand
    {
        private static readonly DataCache<Dictionary<string, string>> Cache = new DataCache<Dictionary<string, string>>(RefreshCache, TimeSpan.FromHours(12));
        private static HttpClient Client = new HttpClient();

        private static Dictionary<string, string> RefreshCache(Dictionary<string, string> old)
        {
            var data = new Dictionary<string, string>(old?.Count ?? 128, StringComparer.OrdinalIgnoreCase);
            using (var links = Client.GetAsync(ConfigHelper.GetString("WikiUrl")).GetAwaiter().GetResult().Content.ReadAsStreamAsync().GetAwaiter().GetResult())
            using (var reader = new StreamReader(links))
            {
                while (reader.ReadLine() is string line)
                {
                    var splits = line.Split('\t');
                    data.Add(splits[0], splits[2]);
                }
            }
            return data;
        }

        public void Run(IrcMessage theMessage)
        {
            var data = Cache.GetItem(false);
            if (data == null)
            {
                theMessage.Answer("Konnte Datenbank nicht abrufen");
                return;
            }

            if (!data.TryGetValue(theMessage.CommandLine, out string url))
            {
                theMessage.Answer("Nichts gefunden");
                return;
            }

            theMessage.Answer(url);
        }
    }
}