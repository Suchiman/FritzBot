using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace FritzBot.Plugins
{
    [Name("fw")]
    [Help("Sucht auf dem AVM FTP nach der Version des angegbenen Modells, z.b. \"!fw 7390\", \"!fw 7270_v1\", \"!fw 7390 source\", \"!fw 7390 recovery\" \"!fw 7390 all\"")]
    [ParameterRequired]
    [Subscribeable]
    class fw : PluginBase, ICommand, IBackgroundTask
    {
        private FtpDirectory LastScan;
        private Thread worker;
        private string host;

        protected bool FWCheckEnabled { get { return ConfigHelper.GetBoolean("FWCheckEnabled", true); } }
        protected int FWCheckIntervall { get { return ConfigHelper.GetInt("FWCheckIntervall", 6000); } }

        public void Start()
        {
            worker = toolbox.SafeThreadStart(PluginID, true, WorkerThread);
        }

        public void Stop()
        {
            try
            {
                if (worker != null && worker.IsAlive)
                {
                    worker.Abort();
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging(ex);
            }
        }

        public void WorkerThread()
        {
            while (true)
            {
                if (FWCheckEnabled)
                {
                    FtpDirectory CurrentScan;
                    using (FtpClient ftp = GetClient())
                    {
                        try
                        {
                            CurrentScan = RecurseFTP(ftp, "/fritz.box");
                        }
                        catch
                        {
                            Thread.Sleep(FWCheckIntervall);
                            continue;
                        }
                    }
                    if (LastScan == null)
                    {
                        LastScan = CurrentScan;
                        Thread.Sleep(FWCheckIntervall);
                        continue;
                    }

                    List<string> neu = new List<string>();
                    List<string> gelöscht = new List<string>();
                    List<string> geändert = new List<string>();
                    var joinedDirectories = LastScan.Folders.Flatten(x => x.Folders).FullOuterJoin(CurrentScan.Folders.Flatten(x => x.Folders), x => x.FullName, x => x.FullName, (o, c, _) => new { Original = o, Current = c });
                    foreach (var directory in joinedDirectories)
                    {
                        if (directory.Original == null) // Neuer Ordner
                        {
                            neu.Add("[" + directory.Current.FullName + (directory.Current.Files.Any() ? "(" + directory.Current.Files.Select(x => x.Name).Join(", ") + ")]" : "]"));
                        }
                        else if (directory.Current == null) // Ordner gelöscht
                        {
                            gelöscht.Add("[" + directory.Original.FullName + (directory.Original.Files.Any() ? "(" + directory.Original.Files.Select(x => x.Name).Join(", ") + ")]" : "]"));
                        }
                        else
                        {
                            var joinedFiles = directory.Original.Files.FullOuterJoin(directory.Current.Files, x => x.Name, x => x.Name, (o, c, _) => new { Original = o, Current = c });
                            List<string> fileChanges = new List<string>();
                            foreach (var file in joinedFiles)
                            {
                                if (file.Original == null) // Neue Datei
                                {
                                    fileChanges.Add(file.Current.Name + "(neu)");
                                }
                                else if (file.Current == null) // Datei gelöscht
                                {
                                    fileChanges.Add(file.Original.Name + "(gelöscht)");
                                }
                                else if (file.Original.Modified != file.Current.Modified) // Datei geändert
                                {
                                    fileChanges.Add(file.Original.Name + "(" + file.Original.Modified.ToString() + "/" + file.Current.Modified.ToString() + ")");
                                }
                            }
                            if (fileChanges.Count > 0)
                            {
                                geändert.Add("[" + directory.Current.FullName + "(" + fileChanges.Join(", ") + ")]");
                            }
                        }
                    }
                    if (neu.Count + gelöscht.Count + geändert.Count > 0)
                    {
                        string labors = "Änderungen auf dem FTP gesichtet! - ";
                        if (neu.Count > 0)
                        {
                            labors += "Neue { " + neu.Join(" ") + " } ";
                        }
                        if (gelöscht.Count > 0)
                        {
                            labors += "Gelöscht { " + gelöscht.Join(" ") + " } ";
                        }
                        if (geändert.Count > 0)
                        {
                            labors += "Geändert { " + geändert.Join(" ") + " } ";
                        }
                        labors = labors.TrimEnd() + " - Zum FTP: ftp://ftp.avm.de/fritz.box/";
                        ServerManager.AnnounceGlobal(labors);
                        NotifySubscribers(labors, neu.Concat(geändert).Select(x => BoxDatabase.GetShortName(x)).ToArray());
                    }
                    LastScan = CurrentScan;
                    Thread.Sleep(FWCheckIntervall);
                }
                else
                {
                    Thread.Sleep(30000);
                }
            }
        }

        private FtpClient GetClient()
        {
            //AVM hat mehrere FTP Server mit unterschiedlichem Inhalt, hier konsistent einen Server auswählen
            if (host == null)
            {
                IPAddress[] addresses = Dns.GetHostAddresses("ftp.avm.de");
                host = (addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetworkV6 && x.ToString().EndsWith(".7")) ?? addresses.FirstOrDefault()).ToString();
            }
            return new FtpClient
            {
                Host = host,
                Credentials = new NetworkCredential("anonymous", "")
            };
        }

        private FtpDirectory RecurseFTP(FtpClient ftp, string path)
        {
            FtpListItem[] filesAndFolders = ftp.GetListing(path, FtpListOption.Modify);

            FtpDirectory current = new FtpDirectory();
            current.FullName = path;
            current.Folders = filesAndFolders.Where(x => x.Type == FtpFileSystemObjectType.Directory).Select(x => RecurseFTP(ftp, x.FullName)).ToList();
            current.Files = filesAndFolders.Where(x => x.Type == FtpFileSystemObjectType.File).Select(x => new FtpFile { Name = x.Name, Modified = x.Modified }).ToList();

            return current;
        }

        protected override IQueryable<Subscription> GetSubscribers(BotContext context, string[] criteria)
        {
            if (criteria != null && criteria.Length > 0)
            {
                return base.GetSubscribers(context, criteria).Where(x => criteria.Any(c => x.Bedingungen.Any(a => a.Bedingung.Contains(c))));
            }
            return base.GetSubscribers(context, criteria);
        }

        public void Run(IrcMessage theMessage)
        {
            bool recovery = false;
            bool source = false;
            bool firmware = false;
            Box box = BoxDatabase.FindBoxes(theMessage.CommandLine).FirstOrDefault();
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
                        source = true;
                        break;
                    }
                    else if (argument.Equals("source", StringComparison.OrdinalIgnoreCase) || argument.Equals("sources", StringComparison.OrdinalIgnoreCase) || argument.Equals("src", StringComparison.OrdinalIgnoreCase))
                    {
                        source = true;
                    }
                    else if (argument.Equals("recovery", StringComparison.OrdinalIgnoreCase) || argument.Equals("recoveries", StringComparison.OrdinalIgnoreCase))
                    {
                        recovery = true;
                    }
                    else if (argument.Equals("firmware", StringComparison.OrdinalIgnoreCase) || argument.Equals("firmwares", StringComparison.OrdinalIgnoreCase))
                    {
                        firmware = true;
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

            FtpDirectory Scan = LastScan;
            FtpDirectory match = null;
            List<FtpDirectory> matches = null;
            bool shouldRefresh = false;
            if (Scan != null)
            {
                matches = FindDirectory(Scan.Folders, box, rawName);
                if (matches.Count > 1)
                {
                    theMessage.Answer("Bitte genauer spezifizieren: " + OnlyReadableNames(matches.Select(x => x.Name)).Join(", "));
                    return;
                }
                match = matches.FirstOrDefault();
                shouldRefresh = true;
            }
            if (match == null)
            {
                List<FtpDirectory> FreshScanned;
                using (FtpClient client = GetClient())
                {
                    FreshScanned = client.GetListing("/fritz.box").Where(x => x.Type == FtpFileSystemObjectType.Directory).Select(x => new FtpDirectory { FullName = x.FullName }).ToList();
                    matches = FindDirectory(FreshScanned, box, rawName);
                    if (matches.Count > 1)
                    {
                        theMessage.Answer("Bitte genauer spezifizieren: " + OnlyReadableNames(matches.Select(x => x.Name)).Join(", "));
                        return;
                    }
                    match = matches.FirstOrDefault();
                    if (match != null)
                    {
                        match = RecurseFTP(client, match.FullName);
                        shouldRefresh = false;
                    }
                }
            }

            if (match == null)
            {
                theMessage.Answer("Ich habe zu deiner Suche leider kein Verzeichnis gefunden");
                return;
            }

            string output = FormatResult(match, recovery, source, firmware);
            if (!String.IsNullOrEmpty(output))
            {
                theMessage.Answer(output);
            }

            if (shouldRefresh)
            {
                using (FtpClient client = GetClient())
                {
                    match = RecurseFTP(client, match.FullName);
                    string refreshedOutput = FormatResult(match, recovery, source, firmware);
                    if (String.IsNullOrEmpty(output) && !String.IsNullOrEmpty(refreshedOutput))
                    {
                        theMessage.Answer(refreshedOutput);
                        return;
                    }
                    if (output != refreshedOutput)
                    {
                        theMessage.Answer("Wups, meine Angabe war nicht mehr Up-to-Date, hier kommen die aktuellen Ergebnisse:");
                        theMessage.Answer(refreshedOutput);
                    }
                }
            }
        }

        private static IEnumerable<string> OnlyReadableNames(IEnumerable<string> files)
        {
            foreach (string name in files)
            {
                string shortName;
                if (BoxDatabase.TryGetShortName(name, out shortName))
                {
                    yield return shortName;
                }
            }
        }

        private static List<FtpDirectory> FindDirectory(List<FtpDirectory> directories, Box box, string name)
        {
            FtpDirectory idMatch = null;
            FtpDirectory directMatch = null;
            List<FtpDirectory> roughMatches = new List<FtpDirectory>();
            foreach (FtpDirectory directory in directories)
            {
                if (box != null && BoxDatabase.FindBoxes(directory.Name).Any(x => x == box))
                {
                    idMatch = directory;
                    break;
                }
                else if (directory.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    directMatch = directory;
                }
                else if (directory.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    roughMatches.Add(directory);
                }
            }
            return idMatch == null && directMatch == null ? roughMatches : new List<FtpDirectory> { idMatch ?? directMatch };
        }

        private static string FormatResult(FtpDirectory scan, bool recovery, bool source, bool firmware)
        {
            List<FtpDirectory> directories = scan.Folders.Flatten(x => x.Folders).Concat(new[] { scan }).ToList();

            List<string> recoveries = new List<string>();
            List<string> sources = new List<string>();
            List<string> firmwares = new List<string>();

            foreach (FtpDirectory directory in directories.Where(x => x.Files.Count > 0))
            {
                string folderName = Path.GetFileName(directory.FullName) + "/";
                foreach (FtpFile file in directory.Files)
                {
                    if (file.Name.EndsWith(".image", StringComparison.OrdinalIgnoreCase))
                    {
                        firmwares.Add(folderName + TryExtractVersion(file.Name));
                    }
                    if (file.Name.EndsWith(".recover-image.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        recoveries.Add(folderName + TryExtractVersion(file.Name));
                    }
                    if (file.Name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))//fritzbox7170-source-files-04.87.tar.gz
                    {
                        sources.Add(TryExtractVersion(file.Name));
                    }
                }
            }

            string output = "";
            if (firmware && firmwares.Count > 0)
            {
                output += " - Firmwares: " + firmwares.Join(", ");
            }
            if (recovery && recoveries.Count > 0)
            {
                output += " - Recoveries: " + recoveries.Join(", ");
            }
            if (source && sources.Count > 0)
            {
                output += " - Sources: " + sources.Join(", ");
            }

            return output == "" ? "" : "ftp://ftp.avm.de" + scan.FullName + output;
        }

        /// <summary>
        /// Extrahiert von einem Dateinamen die Versionsnummer
        /// </summary>
        /// <param name="toExtract">Der Dateiname</param>
        /// <returns>Die Versionsnummer oder der eingabestring wenn keine Versionsnummer extrahiert wurden konnte</returns>
        public static string TryExtractVersion(string toExtract)
        {
            Contract.Requires(toExtract != null);
            Contract.Ensures(Contract.Result<string>() != null);

            Match regex = Regex.Match(toExtract, @"((\b\d{2,3}\.)?\d\d\.\d\d-?\d?\d?)\.\D"); //@"\d{2,3}\.\d\d\.\d\d"
            if (!regex.Success)
            {
                regex = Regex.Match(toExtract, @"((\b\d{2,3}\.)?\d\d\.\d\d-\d{5})\.\D"); // Mit 5 stelliger Revisionsnummer
            }
            return regex.Success ? regex.Groups[1].Value : toExtract;
        }
    }

    class FtpFile
    {
        public string Name { get; set; }
        public DateTime Modified { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    class FtpDirectory
    {
        public string Name { get { return Path.GetFileName(FullName); } }
        public string FullName { get; set; }
        public List<FtpFile> Files { get; set; }
        public List<FtpDirectory> Folders { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}