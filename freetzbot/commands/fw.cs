using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace FritzBot.commands
{
    [Module.Name("fw")]
    [Module.Help("Sucht auf dem AVM FTP nach der Version des angegbenen Modells, z.b. \"!fw 7390\", \"!fw 7270_v1\", \"!fw 7390 source\", \"!fw 7390 recovery\" \"!fw 7390 all\"")]
    [Module.ParameterRequired]
    class fw : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Boolean recovery = false;
            Boolean source = false;
            Boolean firmware = false;
            String fwToFind = theMessage.CommandArgs[0];
            if (theMessage.CommandArgs.Count > 1)
            {
                if (theMessage.CommandArgs[0].ToLower() == "add")
                {
                    File.AppendAllText("fwdb.db", theMessage.CommandArgs[1]);
                    theMessage.Answer("Der FW Alias wurde hinzugefügt");
                    return;
                }
                switch (theMessage.CommandArgs[1].ToLower())
                {
                    case "all":
                        firmware = true;
                        recovery = true;
                        source = true;
                        fwToFind = theMessage.CommandArgs[0];
                        break;
                    case "source":
                        source = true;
                        fwToFind = theMessage.CommandArgs[0];
                        break;
                    case "recovery":
                        recovery = true;
                        fwToFind = theMessage.CommandArgs[0];
                        break;
                    default:
                        firmware = true;
                        fwToFind = theMessage.CommandArgs[0];
                        break;
                }
            }
            else
            {
                firmware = true;
            }

            String ftp = "ftp://ftp.avm.de/fritz.box/";
            String output = "";

            //Ordner der Box bestimmen: wenn möglich Alias verwenden, ansonsten versuchen zu finden
            String fw = "";
            if (File.Exists("fwdb.db"))
            {
                String[] fws = File.ReadAllLines("fwdb.db");
                foreach (String onefw in fws)
                {
                    if (onefw.Contains(fwToFind))
                    {
                        if (onefw.Split(new String[] { "=" }, 2, StringSplitOptions.None)[0].ToLower() == fwToFind.ToLower())
                        {
                            fw = onefw;
                            break;
                        }
                    }
                }
            }
            if (fw != "")
            {
                ftp += fw.Split(new String[] { "=" }, 2, StringSplitOptions.None)[1] + "/";
            }
            else
            {
                List<String> DirectoryNames = GetListingNames(FtpDirectory(ftp));
                foreach (String Directory in DirectoryNames)
                {
                    if (Directory.ToLower().Contains(fwToFind.ToLower()))
                    {
                        ftp += Directory + "/";
                        break;
                    }
                }
            }
            if (ftp == "ftp://ftp.avm.de/fritz.box/")
            {
                theMessage.Answer("Ich habe zu deiner Angabe leider nichts gefunden");
                return;
            }
            output = ftp;
            //Box Ordner ist nun gefunden, Firmware Image muss gefunden werden, vorsicht könnte bereits hier sein oder erst in einem weiteren Unterordner
            List<String> ftp_recur = FtpRecursiv(ftp);
            String recoveries = "";
            String sources = "";
            String firmwares = "";
            foreach (String datei in ftp_recur)
            {
                String[] slashsplit = datei.Split(new String[] { "/" }, 2, StringSplitOptions.None);
                String final = slashsplit[0] + "/";
                if (slashsplit[1].Contains(".image"))
                {
                    firmwares += ", " + final + ExtractVersion(slashsplit[1], ".image");
                }
                if (slashsplit[1].Contains(".recover-image.exe"))
                {
                    recoveries += ", " + final + ExtractVersion(slashsplit[1], ".recover-image.exe");
                }
                if (slashsplit[1].Contains(".tar.gz"))//fritzbox7170-source-files-04.87.tar.gz
                {
                    sources += ", " + ExtractVersion(slashsplit[1], ".tar.gz");
                }
            }
            if (String.IsNullOrEmpty(firmwares))
            {
                theMessage.Answer("Ich habe zu deiner Angabe leider nichts gefunden");
            }
            else
            {
                if (firmware && !String.IsNullOrEmpty(firmwares))
                {
                    output += " - Firmwares: " + firmwares.Remove(0, 2);
                }
                if (recovery && !String.IsNullOrEmpty(recoveries))
                {
                    output += " - Recoveries: " + recoveries.Remove(0, 2);
                }
                if (source && !String.IsNullOrEmpty(sources))
                {
                    output += " - Sources: " + sources.Remove(0, 2);
                }
                theMessage.Answer(output);
            }
        }
        /// <summary>
        /// Extrahiert von einem Dateinamen die Versionsnummer
        /// </summary>
        /// <param name="toExtract">Der Dateiname</param>
        /// <param name="extension">Die Erweiterung (.tar.gz, .image...)</param>
        /// <returns>Die Versionsnummer</returns>
        public static String ExtractVersion(String toExtract, String extension)
        {
            String temp = toExtract.Replace(extension, "");
            temp = Regex.Replace(temp, "[A-Za-z-_ ]", "#");
            temp = temp.Remove(0, temp.LastIndexOf('#') + 1);
            while (temp.Split('.').Length > 2)
            {
                temp = temp.Remove(0, temp.IndexOf('.') + 1);
            }
            return temp;
        }
        /// <summary>
        /// Extrahiert aus der FTP Verzeichnisauflistung die Namen der Dateien und Ordner
        /// </summary>
        /// <param name="Listing">Die FTP Verzeichnisauflistung</param>
        /// <returns>Eine Liste die alle Namen beinhaltet</returns>
        public static List<String> GetListingNames(String Listing)
        {
            List<String> DirectoryNames = new List<String>();
            String[] Entries = Listing.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String Entry in Entries)
            {
                DirectoryNames.Add(Entry.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8]);
            }
            return DirectoryNames;
        }
        /// <summary>
        /// Durchsucht die FTP Adresse Rekursiv nach Dateien mit der Dateierweiterung .image, .recover-image.exe, .tar.gz und gibt diese mit ihrem relativen Pfad zurück
        /// </summary>
        /// <param name="ftp">Die FTP Adresse</param>
        /// <returns>Eine Liste mit den gefundenen Dateinamen inklusive relativen Pfad angaben</returns>
        public static List<String> FtpRecursiv(String ftp)
        {
            String BoxdirectoryContent = FtpDirectory(ftp);
            String[] boxes = BoxdirectoryContent.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<String> found = new List<String>();
            foreach (String daten in boxes)
            {
                if (daten.ToCharArray()[0] == 'd')
                {
                    String pfad = daten.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    found.AddRange(FtpRecursiv(ftp + pfad + "/"));
                }
                else if (daten.ToCharArray()[0] == '-' && (daten.Contains(".image") || daten.Contains(".recover-image.exe") || daten.Contains(".tar.gz")))
                {
                    String file = daten.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    String[] FtpSplitted = ftp.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    found.Add(FtpSplitted[FtpSplitted.Length - 1] + "/" + file);
                }
            }
            return found;
        }
        /// <summary>
        /// Ruft von der angegeben FTP Adresse eine Verzeichnissauflistung ab
        /// </summary>
        /// <param name="ftp">Die FTP Adresse des Servers</param>
        /// <returns>Verzeichnissauflistung (String)</returns>
        public static String FtpDirectory(String ftp)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse directory = (FtpWebResponse)request.GetResponse();
            StreamReader DirectoryList = new StreamReader(directory.GetResponseStream());
            return DirectoryList.ReadToEnd();
        }
    }
}