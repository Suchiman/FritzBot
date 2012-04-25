using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace FritzBot.commands
{
    class fw : ICommand
    {
        public String[] Name { get { return new String[] { "fw" }; } }
        public String HelpText { get { return "Sucht auf dem AVM FTP nach der Version des angegbenen Modells, z.b. \"!fw 7390\", \"!fw 7270_v1\", \"!fw 7390 source\", \"!fw 7390 recovery\" \"!fw 7390 all\""; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            Boolean recovery = false;
            Boolean source = false;
            Boolean firmware = false;
            if (message.Contains(" "))
            {
                String[] splitted = message.Split(' ');
                switch (splitted[1].ToLower())
                {
                    case "add":
                        toolbox.getDatabaseByName("fwdb.db").Add(splitted[1]);
                        connection.Sendmsg("Der FW Alias wurde hinzugefügt", receiver);
                        return;
                    case "all":
                        firmware = true;
                        recovery = true;
                        source = true;
                        message = splitted[0];
                        break;
                    case "source":
                        source = true;
                        message = splitted[0];
                        break;
                    case "recovery":
                        recovery = true;
                        message = splitted[0];
                        break;
                    default:
                        firmware = true;
                        message = splitted[0];
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
            String[] fws = toolbox.getDatabaseByName("fwdb.db").GetContaining(message + "=");
            if (fws.Length > 0)
            {
                ftp += fws[0].Split(new String[] { "=" }, 2, StringSplitOptions.None)[1] + "/";
            }
            else
            {
                List<String> DirectoryNames = GetDirectoryNames(FtpDirectory(ftp));
                foreach (String Directory in DirectoryNames)
                {
                    if (Directory.ToLower().Contains(message.ToLower()))
                    {
                        ftp += Directory + "/";
                        break;
                    }
                }
            }
            if (ftp == "ftp://ftp.avm.de/fritz.box/")
            {
                connection.Sendmsg("Ich habe zu deiner Angabe leider nichts gefunden", receiver);
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
                connection.Sendmsg("Ich habe zu deiner Angabe leider nichts gefunden", receiver);
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
                connection.Sendmsg(output, receiver);
            }
        }

        private String ExtractVersion(String toExtract, String extension)
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

        private List<String> GetDirectoryNames(String Listing)
        {
            List<String> DirectoryNames = new List<String>();
            String[] Entries = Listing.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String Entry in Entries)
            {
                DirectoryNames.Add(Entry.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8]);
            }
            return DirectoryNames;
        }

        private List<String> FtpRecursiv(String ftp)
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

        private String FtpDirectory(String ftp)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse directory = (FtpWebResponse)request.GetResponse();
            StreamReader DirectoryList = new StreamReader(directory.GetResponseStream());
            return DirectoryList.ReadToEnd();
        }
    }
}