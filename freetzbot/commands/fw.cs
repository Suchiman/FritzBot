using System;
using System.IO;
using System.Net;

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

            //Ordner der Box bestimmen, wenn möglich Alias verwenden, ansonsten versuchen zu finden
            String[] fws = toolbox.getDatabaseByName("fwdb.db").GetContaining(message + "=");
            if (fws.Length > 0)
            {
                ftp += fws[0].Split(new String[] { "=" }, 2, StringSplitOptions.None)[1] + "/";
            }
            else
            {
                String directory = ftp_directory(ftp);
                String[] directories = directory.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String data in directories)
                {
                    String pfad = data.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries)[8];
                    if (pfad.ToLower().Contains(message.ToLower()))
                    {
                        ftp += pfad + "/";
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
            String[] ftp_recur = ftp_recursiv(ftp).Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            String recoveries = "";
            String sources = "";
            String firmwares = "";
            foreach (String datei in ftp_recur)
            {
                String[] slashsplit = datei.Split(new String[] { "/" }, 2, StringSplitOptions.None);
                String[] file_splitted = slashsplit[1].Split('.'); //fritz.box_fon_5010.23.04.27.image
                String final = slashsplit[0] + "/";
                if (slashsplit[1].Contains(".recover-image.exe"))
                {
                    int result;
                    if (file_splitted[file_splitted.Length - 5].Contains("_"))
                    {
                        String[] recoversplit = file_splitted[file_splitted.Length - 5].Split('_');
                        final += recoversplit[recoversplit.Length - 1] + "." + file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3];
                    }
                    else if (!int.TryParse(file_splitted[file_splitted.Length - 5], out result))
                    {
                        final += file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3];
                    }
                    else
                    {
                        final += file_splitted[file_splitted.Length - 5] + "." + file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3];
                    }
                    if (!String.IsNullOrEmpty(recoveries))
                    {
                        recoveries += ", " + final;
                    }
                    else
                    {
                        recoveries += final;
                    }
                }
                if (slashsplit[1].Contains(".image"))
                {
                    final += file_splitted[file_splitted.Length - 4] + "." + file_splitted[file_splitted.Length - 3] + "." + file_splitted[file_splitted.Length - 2];
                    if (!String.IsNullOrEmpty(firmwares))
                    {
                        firmwares += ", " + final;
                    }
                    else
                    {
                        firmwares += final;
                    }
                }
                if (slashsplit[1].Contains(".tar.gz"))//fritzbox7170-source-files-04.87.tar.gz
                {
                    String[] dotsplit = slashsplit[1].Split('.');
                    final = dotsplit[dotsplit.Length - 4] + "." + dotsplit[dotsplit.Length - 3];
                    if (!String.IsNullOrEmpty(sources))
                    {
                        sources += ", " + final;
                    }
                    else
                    {
                        sources += final;
                    }
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
                    output += " - Firmwares: " + firmwares;
                }
                if (recovery && !String.IsNullOrEmpty(recoveries))
                {
                    output += " - Recoveries: " + recoveries;
                }
                if (source && !String.IsNullOrEmpty(sources))
                {
                    output += " - Sources: " + sources;
                }
                connection.Sendmsg(output, receiver);
            }
        }

        private static String ftp_recursiv(String ftp)
        {
            String boxdirectory_content = ftp_directory(ftp);
            String[] boxes = boxdirectory_content.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String found = "";
            foreach (String daten in boxes)
            {
                if (daten.ToCharArray()[0] == 'd')
                {
                    String pfad = daten.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    found += ftp_recursiv(ftp + pfad + "/") + ";";
                }
                else if (daten.ToCharArray()[0] == '-' && (daten.Contains(".image") || daten.Contains(".recover-image.exe") || daten.Contains(".tar.gz")))
                {
                    String file = daten.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries)[8];
                    String[] ftp_splitted = ftp.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    found += ";" + ftp_splitted[ftp_splitted.Length - 1] + "/" + file;
                }
            }
            return found;
        }

        private static String ftp_directory(String ftp)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse directory = (FtpWebResponse)request.GetResponse();
            StreamReader directory_list = new StreamReader(directory.GetResponseStream());
            return directory_list.ReadToEnd();
        }
    }
}