using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace FritzBot.Plugins
{
    [Module.Name("fw")]
    [Module.Help("Sucht auf dem AVM FTP nach der Version des angegbenen Modells, z.b. \"!fw 7390\", \"!fw 7270_v1\", \"!fw 7390 source\", \"!fw 7390 recovery\" \"!fw 7390 all\"")]
    [Module.ParameterRequired]
    [Module.Subscribeable]
    class fw : PluginBase, ICommand//, IBackgroundTask
    {
        const string BaseDirectory = "ftp://ftp.avm.de/fritz.box/";
        Thread worker;

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
            List<string> alte = FTPGrabber.Scan(BaseDirectory, 1);
            SimpleStorage storage = GetPluginStorage(new DBProvider());
            while (true)
            {
                if (storage.Get("CheckEnabled", true))
                {
                    List<string> neue = FTPGrabber.Scan(BaseDirectory, 1);
                    List<string> unEquals = neue.Where(x => !alte.Contains(x)).ToList();
                    if (unEquals.Count > 0)
                    {
                        string labors = "Änderungen auf dem FTP gesichtet! - " + String.Join(", ", unEquals.Select(x => GetReadableString(x) + ": " + x.Split(' ').Last()).ToArray()) + " - Zum FTP: " + BaseDirectory;
                        ServerManager.GetInstance().AnnounceGlobal(labors);
                        NotifySubscribers(labors, unEquals.Select(x => BoxDatabase.GetInstance().GetShortName(x)).ToArray());
                        alte = neue;
                    }
                    Thread.Sleep(storage.Get("Intervall", 600000));
                }
                else
                {
                    Thread.Sleep(30000);
                }
            }
        }

        public string GetReadableString(string input)
        {
            string output;
            if (!BoxDatabase.GetInstance().TryGetShortName(input, out output))
            {
                //ftp://ftp.avm.de/fritz.box/fritzbox.fon_wlan_7170_sl/firmware/deutsch/-rw-r--r--    1 ftp      ftp         13010 Feb 26  2010 info.txt
                string[] splits = input.Split('/');
                if (splits.Length > 4)
                {
                    output = splits[4];
                }
                else
                {
                    output = input;
                }
            }
            return output;
        }

        protected override IQueryable<Subscription> GetSubscribers(string[] criteria)
        {
            if (criteria != null && criteria.Length > 0)
            {
                return base.GetSubscribers(criteria).Where(x => criteria.Any(c => x.Bedingungen.Contains(c)));
            }
            return base.GetSubscribers(criteria);
        }

        public void Run(ircMessage theMessage)
        {
            bool recovery = false;
            bool source = false;
            bool firmware = false;
            Box box = BoxDatabase.GetInstance().FindBoxes(theMessage.CommandLine).FirstOrDefault();
            if (theMessage.CommandArgs.Count > 1)
            {
                switch (theMessage.CommandArgs.Last().ToLower())
                {
                    case "all":
                        firmware = true;
                        recovery = true;
                        source = true;
                        break;
                    case "source":
                        source = true;
                        break;
                    case "recovery":
                        recovery = true;
                        break;
                    default:
                        firmware = true;
                        break;
                }
            }
            else
            {
                firmware = true;
            }

            string ftp = BaseDirectory;
            string output = "";

            List<string> DirectoryNames = GetListingNames(FtpDirectory(ftp)).ToList();
            foreach (string Directory in DirectoryNames)
            {
                if (BoxDatabase.GetInstance().FindBoxes(Directory).Any(x => x == box))
                {
                    ftp += Directory + "/";
                    break;
                }
            }
            if (ftp == BaseDirectory)
            {
                foreach (string Directory in DirectoryNames)
                {
                    if (Directory.Contains(theMessage.CommandLine))
                    {
                        ftp += Directory + "/";
                        break;
                    }
                }
            }
            if (ftp == BaseDirectory)
            {
                theMessage.Answer("Ich habe zu deiner Angabe leider nichts gefunden");
                return;
            }
            output = ftp;
            //Box Ordner ist nun gefunden, Firmware Image muss gefunden werden, vorsicht könnte bereits hier sein oder erst in einem weiteren Unterordner
            List<string> recoveries = new List<string>();
            List<string> sources = new List<string>();
            List<string> firmwares = new List<string>();
            foreach (string datei in FtpRecursiv(ftp))
            {
                string[] slashsplit = datei.Split(new string[] { "/" }, 2, StringSplitOptions.None);
                string final = slashsplit[0] + "/";
                if (slashsplit[1].EndsWith(".image"))
                {
                    firmwares.Add(final + ExtractVersion(slashsplit[1]));
                }
                if (slashsplit[1].EndsWith(".recover-image.exe"))
                {
                    recoveries.Add(final + ExtractVersion(slashsplit[1]));
                }
                if (slashsplit[1].EndsWith(".tar.gz"))//fritzbox7170-source-files-04.87.tar.gz
                {
                    sources.Add(ExtractVersion(slashsplit[1]));
                }
            }
            if (firmware && firmwares.Count > 0)
            {
                output += " - Firmwares: " + String.Join(", ", firmwares.ToArray());
            }
            if (recovery && recoveries.Count > 0)
            {
                output += " - Recoveries: " + String.Join(", ", recoveries.ToArray());
            }
            if (source && sources.Count > 0)
            {
                output += " - Sources: " + String.Join(", ", sources.ToArray());
            }
            if (String.IsNullOrEmpty(output))
            {
                theMessage.Answer("Ich habe zu deiner Angabe leider nichts gefunden");
                return;
            }
            theMessage.Answer(output);
        }
        /// <summary>
        /// Extrahiert von einem Dateinamen die Versionsnummer
        /// </summary>
        /// <param name="toExtract">Der Dateiname</param>
        /// <returns>Die Versionsnummer</returns>
        /// <exception cref="ArgumentException">Tritt ein wenn kein korrektes Versionsformat übergeben wurde</exception>
        public static string ExtractVersion(string toExtract)
        {
            Match regex = Regex.Match(toExtract, @"((\d{2,3}\.)?\d\d\.\d\d-?\d?\d?)\.\D"); //@"\d{2,3}\.\d\d\.\d\d"
            if (regex.Success)
            {
                return regex.Groups[1].Value;
            }
            throw new ArgumentException("Der angegebene string enthält kein korrektes Versionsformat");
        }
        /// <summary>
        /// Extrahiert aus der FTP Verzeichnisauflistung die Namen der Dateien und Ordner
        /// </summary>
        /// <param name="Listing">Die FTP Verzeichnisauflistung</param>
        /// <returns>Eine Liste die alle Namen beinhaltet</returns>
        public static IEnumerable<string> GetListingNames(string Listing)
        {
            string[] Entries = Listing.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string Entry in Entries)
            {
                yield return Entry.Split(new string[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
            }
        }
        /// <summary>
        /// Durchsucht die FTP Adresse Rekursiv nach Dateien mit der Dateierweiterung .image, .recover-image.exe, .tar.gz und gibt diese mit ihrem relativen Pfad zurück
        /// </summary>
        /// <param name="ftp">Die FTP Adresse</param>
        /// <returns>Eine Liste mit den gefundenen Dateinamen inklusive relativen Pfad angaben</returns>
        public static IEnumerable<string> FtpRecursiv(string ftp)
        {
            string[] lines = FtpDirectory(ftp).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string daten in lines)
            {
                if (daten[0] == 'd')
                {
                    string pfad = daten.Split(new string[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    foreach (string recursiv in FtpRecursiv(ftp + pfad + "/"))
                    {
                        yield return recursiv;
                    }
                }
                else if (daten[0] == '-' && (daten.EndsWith(".image") || daten.EndsWith(".recover-image.exe") || daten.EndsWith(".tar.gz")))
                {
                    string file = daten.Split(new string[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    string[] FtpSplitted = ftp.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    yield return FtpSplitted.Last() + "/" + file;
                }
            }
        }
        /// <summary>
        /// Ruft von der angegeben FTP Adresse eine Verzeichnissauflistung ab
        /// </summary>
        /// <param name="ftp">Die FTP Adresse des Servers</param>
        /// <returns>Verzeichnissauflistung (String)</returns>
        public static string FtpDirectory(string ftp)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse directory = (FtpWebResponse)request.GetResponse();
            StreamReader DirectoryList = new StreamReader(directory.GetResponseStream());
            return DirectoryList.ReadToEnd();
        }
    }

    class FTPGrabber
    {
        public static List<string> Scan(string Adress, int Threads)
        {
            FTPGrabber grabber = new FTPGrabber(Adress, Threads);
            return grabber.Grab().OrderBy(x => x).ToList();
        }

        string basedirectory;
        int threadsCount;

        public FTPGrabber(string Adress, int Threads)
        {
            basedirectory = Adress;
            threadsCount = Threads;
        }

        private class PartlyScanner
        {
            string[] dirsToDo;
            string ftpbase;
            List<string> result = new List<string>();
            public Thread Worker;

            public static PartlyScanner BeginScan(string basedir, string[] subdirs)
            {
                PartlyScanner scanner = new PartlyScanner(basedir, subdirs);
                scanner.BeginScan();
                return scanner;
            }
            private PartlyScanner(string basedir, string[] subdirs)
            {
                dirsToDo = subdirs;
                ftpbase = basedir;
            }
            public void BeginScan()
            {
                Worker = new Thread(delegate()
                {
                    foreach (string dir in dirsToDo)
                    {
                        FtpRecursiv(ftpbase + dir);
                    }
                });
                Worker.Start();
            }
            public void FtpRecursiv(string ftp)
            {
                string[] lines = FtpDirectory(ftp).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                result.AddRange(lines.Where(x => x.StartsWith("-")).Select(x => ftp + x));
                lines.Where(x => x.StartsWith("d")).Select(x => x.Split(new string[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8]).ForEach(x => FtpRecursiv(ftp + "/" + x));
            }
            public static string FtpDirectory(string ftp)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                FtpWebResponse directory = (FtpWebResponse)request.GetResponse();
                StreamReader DirectoryList = new StreamReader(directory.GetResponseStream());
                return DirectoryList.ReadToEnd();
            }
            public List<string> GetResult()
            {
                return result;
            }
        }

        public List<string> Grab()
        {
            string[] lines = PartlyScanner.FtpDirectory(basedirectory).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] dirs = lines.Where(x => x.StartsWith("d")).Select(x => x.Split(new string[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8]).ToArray();

            int divider = dirs.Length / (threadsCount);
            IEnumerable<IGrouping<int, string>> parts = from index in Enumerable.Range(0, dirs.Length)
                                                        group dirs[index] by index / divider;

            PartlyScanner[] scanners = new PartlyScanner[parts.Count()];

            foreach (IGrouping<int, string> part in parts)
            {
                scanners[part.Key] = PartlyScanner.BeginScan(basedirectory, part.ToArray());
            }
            scanners.ForEach(x => x.Worker.Join());
            return scanners.SelectMany(x => x.GetResult()).ToList();
        }
    }
}