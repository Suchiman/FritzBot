using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Functions;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace FritzBot.Plugins
{
    [Name("fw")]
    [Help("Gibt infos zu aktuellen Firmwares")]
    class Fw2Plugin : PluginBase, ICommand
    {
        private static readonly DataCache<Dictionary<string, FirmwareEntry>> Cache = new DataCache<Dictionary<string, FirmwareEntry>>(RefreshCache, TimeSpan.FromHours(1));
        private static readonly HttpClient Client = new HttpClient();

        private static Dictionary<string, FirmwareEntry> RefreshCache(Dictionary<string, FirmwareEntry> old)
        {
            var data = new Dictionary<string, FirmwareEntry>(old?.Count ?? 128, StringComparer.OrdinalIgnoreCase);
            using (var links = Client.GetAsync(ConfigHelper.GetString("FirmwareUrl")).GetAwaiter().GetResult().Content.ReadAsStreamAsync().GetAwaiter().GetResult())
            using (var reader = new StreamReader(links))
            {
                while (reader.ReadLine() is string line)
                {
                    var splits = line.Split('\t');
                    if (splits.Length != 6)
                    {
                        Log.Warning("Zeile {Row} in Datensatz enthält {Length} anstatt 6 Elemente", line, splits.Length);
                        continue;
                    }

                    var entry = new FirmwareEntry
                    {
                        Nick = splits[0],
                        Model = splits[1],
                        Firmware = splits[2],
                        Recovery = splits[3],
                        Downgrade = splits[4],
                        Source = splits[5]
                    };
                    data.Add(entry.Nick, entry);
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

            bool firmware = false;
            bool recovery = false;
            bool downgrades = false;
            bool source = false;
            List<string> nameSegments = new List<string>();
            string rawName;
            if (theMessage.CommandArgs.Count > 1)
            {
                foreach (string argument in theMessage.CommandArgs)
                {
                    if (argument.Equals("all", StringComparison.OrdinalIgnoreCase) || argument.Equals("alles", StringComparison.OrdinalIgnoreCase))
                    {
                        firmware = true;
                        recovery = true;
                        downgrades = true;
                        source = true;
                        break;
                    }
                    else if (argument.Equals("firmware", StringComparison.OrdinalIgnoreCase) || argument.Equals("firmwares", StringComparison.OrdinalIgnoreCase))
                    {
                        firmware = true;
                    }
                    else if (argument.Equals("recovery", StringComparison.OrdinalIgnoreCase) || argument.Equals("recoveries", StringComparison.OrdinalIgnoreCase))
                    {
                        recovery = true;
                    }
                    else if (argument.Equals("downgrade", StringComparison.OrdinalIgnoreCase) || argument.Equals("downgrades", StringComparison.OrdinalIgnoreCase))
                    {
                        downgrades = true;
                    }
                    else if (argument.Equals("source", StringComparison.OrdinalIgnoreCase) || argument.Equals("sources", StringComparison.OrdinalIgnoreCase) || argument.Equals("src", StringComparison.OrdinalIgnoreCase))
                    {
                        source = true;
                    }
                    else
                    {
                        nameSegments.Add(argument);
                    }
                }
                rawName = nameSegments.Join(" ");
            }
            else
            {
                firmware = true;
                rawName = theMessage.CommandLine;
            }

            if (!data.TryGetValue(rawName, out var entry))
            {
                theMessage.Answer("Nichts gefunden");
                return;
            }

            string? output = FormatResult(entry, firmware, recovery, downgrades, source);
            if (!String.IsNullOrEmpty(output))
            {
                theMessage.Answer(output);
            }
        }

        private static string? FormatResult(FirmwareEntry entry, bool firmware, bool recovery, bool downgrades, bool source)
        {
            string? output = entry.Model;
            if (firmware && !String.IsNullOrWhiteSpace(entry.Firmware))
            {
                output += " - Firmwares: " + entry.Firmware;
            }
            if (recovery && !String.IsNullOrWhiteSpace(entry.Recovery))
            {
                output += " - Recoveries: " + entry.Recovery;
            }
            if (downgrades && !String.IsNullOrWhiteSpace(entry.Downgrade))
            {
                output += " - Downgrades: " + entry.Downgrade;
            }
            if (source && !String.IsNullOrWhiteSpace(entry.Source))
            {
                output += " - Sources: " + entry.Source;
            }

            return output;
        }
    }

    class FirmwareEntry
    {
        public string? Nick { get; set; }
        public string? Model { get; set; }
        public string? Firmware { get; set; }
        public string? Recovery { get; set; }
        public string? Downgrade { get; set; }
        public string? Source { get; set; }
    }
}