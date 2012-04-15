using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace FritzBot
{
    static class toolbox
    {
        private static Thread LoggingThread = new Thread(new ThreadStart(LogThread));
        private static List<string> LoggingList = new List<string>();
        private static Mutex LoggingSafe = new Mutex();

        private static void LogThread()
        {
            while (true)
            {
                try
                {
                    while (!(LoggingList.Count > 0))
                    {
                        Thread.Sleep(500);
                    }
                    FileInfo loginfo = new FileInfo("log.txt");
                    if (loginfo.Exists)
                    {
                        if (loginfo.Length >= 1048576)
                        {
                            if (!Directory.Exists("oldlogs"))
                            {
                                Directory.CreateDirectory("oldlogs");
                            }
                            if (!File.Exists("oldlogs/log" + DateTime.Now.Day + "." + DateTime.Now.Month + ".txt"))
                            {
                                loginfo.MoveTo("oldlogs/log" + DateTime.Now.Day + "." + DateTime.Now.Month + ".txt");
                            }
                        }
                    }
                    File.AppendAllText("log.txt", LoggingList[0] + "\r\n", Encoding.GetEncoding("iso-8859-1"));
                    Console.WriteLine(LoggingList[0]);
                    LoggingList.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Zugriff auf den Serverlog: " + ex.Message);
                    return;
                }
            }
        }

        public static void Logging(String toLog)
        {
            LoggingSafe.WaitOne();
            try
            {
                if (!LoggingThread.IsAlive)
                {
                    LoggingThread = new Thread(new ThreadStart(LogThread));
                    LoggingThread.Name = "LoggingThread";
                    LoggingThread.IsBackground = true;
                    LoggingThread.Start();
                }
                LoggingList.Add(DateTime.Now.ToString("dd.MM HH:mm:ss ") + toLog);
            }
            catch (Exception ex)
            {
                Logging("Exception beim logging aufgetreten: " + ex.Message);
            }
            LoggingSafe.ReleaseMutex();
        }

        public static String Crypt(String toCrypt)
        {
            byte[] hash = null;
            byte[] tocode = Encoding.UTF8.GetBytes(toCrypt.ToCharArray());
            using (System.Security.Cryptography.SHA512 theCrypter = new System.Security.Cryptography.SHA512Managed())
            {
                hash = theCrypter.ComputeHash(tocode);
                theCrypter.Clear();
            }
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public static void InstantiateConnection(String server, int Port, String Nick, String Quit_Message, String InitialChannel)
        {
            Irc connection = new Irc(server, Port, Nick);
            connection.QuitMessage = Quit_Message;
            connection.Received += new Irc.ReceivedEventHandler(FritzBot.Program.process_incomming);
            connection.AutoReconnect = true;
            connection.Connect();
            Thread.Sleep(1000);
            if (InitialChannel.Contains(":"))
            {
                String[] channels = InitialChannel.Split(':');
                foreach (String channel in channels)
                {
                    connection.JoinChannel(channel);
                }
            }
            else
            {
                connection.JoinChannel(InitialChannel);
            }
            FritzBot.Program.irc_connections.Add(connection);
        }

        public static String GetWeb(String Url, Dictionary<String, String> POSTParams = null)
        {
            String POSTData = "";
            if (POSTParams != null)
            {
                foreach (String key in POSTParams.Keys)
                {
                    POSTData += HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(POSTParams[key]) + "&";
                }
            }
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(Url);
                request.Timeout = 10000;
                if (POSTParams != null)
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    byte[] data = Encoding.UTF8.GetBytes(POSTData);
                    request.ContentLength = data.Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                }
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Logging("Exception beim Webseiten Aufruf aufgetreten: " + ex.Message);
                return "";
            }
            StreamReader resStream = new StreamReader(response.GetResponseStream(), Encoding.Default);
            String thepage = resStream.ReadToEnd();
            resStream.Close();
            response.Close();

            return thepage;
        }

        public static String ShortUrl(String Url)
        {
            try
            {
                return GetWeb("http://tinyurl.com/api-create.php?url=" + Url);
            }
            catch
            {
                return Url;
            }
        }

        public static db getDatabaseByName(String Name)
        {
            foreach (db database in FritzBot.Program.databases)
            {
                if (database.datenbank_name == Name)
                {
                    return database;
                }
            }
            db datenbank = new db(Name);
            FritzBot.Program.databases.Add(datenbank);
            return datenbank;
        }

        public static ICommand getCommandByName(String Name)
        {
            foreach (ICommand theCommand in FritzBot.Program.Commands)
            {
                foreach (String theName in theCommand.Name)
                {
                    if (theName == Name)
                    {
                        return theCommand;
                    }
                }
            }
            throw new ArgumentException("Command not found");
        }

        public static void Announce(String message)
        {
            foreach (Irc connection in FritzBot.Program.irc_connections)
            {
                foreach (String room in connection.rooms)
                {
                    connection.Sendmsg(message, room);
                }
            }
        }

        public static Boolean RunningCheck()
        {
            for (int i = 0; i < FritzBot.Program.irc_connections.Count; i++)
            {
                if (FritzBot.Program.irc_connections[i].Running())
                {
                    return true;
                }
            }
            return false;
        }

        public static Boolean OpCheck(String Nickname)
        {
            if (FritzBot.Program.TheUsers[Nickname].is_op && FritzBot.Program.TheUsers[Nickname].authenticated)
            {
                return true;
            }
            return false;
        }

        public static Boolean IgnoreCheck(String Nickname)
        {
            return FritzBot.Program.TheUsers[Nickname].ignored;
        }
    }
}