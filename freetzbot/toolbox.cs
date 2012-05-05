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
        private static Queue<String> LoggingList = new Queue<String>();
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
                    File.AppendAllText("log.txt", LoggingList.Peek() + "\r\n", Encoding.GetEncoding("iso-8859-1"));
                    Console.WriteLine(LoggingList.Dequeue());
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
                LoggingList.Enqueue(DateTime.Now.ToString("dd.MM HH:mm:ss ") + toLog);
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
            connection.Received += new Irc.ReceivedEventHandler(Program.process_incomming);
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
            Program.irc_connections.Add(connection);
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
            String theEncoding = "UTF-8";
            if (!String.IsNullOrEmpty(response.ContentEncoding))
            {
                theEncoding = response.ContentEncoding;
            }
            StreamReader resStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(theEncoding, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback));
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
            foreach (db database in Program.databases)
            {
                if (database.datenbank_name == Name)
                {
                    return database;
                }
            }
            db datenbank = new db(Name);
            Program.databases.Add(datenbank);
            return datenbank;
        }

        public static ICommand getCommandByName(String Name)
        {
            foreach (ICommand theCommand in Program.commands)
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
            foreach (Irc connection in Program.irc_connections)
            {
                foreach (String room in connection.rooms)
                {
                    connection.Sendmsg(message, room);
                }
            }
        }

        public static String AwaitAnswer(String nick)
        {
            Program.await_response = true;
            Program.awaited_nick = nick;
            int i = 0;
            while (String.IsNullOrEmpty(Program.awaited_response) && i < 300)
            {
                Thread.Sleep(100);
                i++;
            }
            Program.await_response = false;
            Program.awaited_nick = "";
            String response = Program.awaited_response;
            Program.awaited_response = "";
            return response;
        }

        public static Boolean IsRunning()
        {
            for (int i = 0; i < Program.irc_connections.Count; i++)
            {
                if (Program.irc_connections[i].Running())
                {
                    return true;
                }
            }
            return false;
        }

        public static Boolean IsOp(String Nickname)
        {
            if (Program.TheUsers[Nickname].isOp && (Program.TheUsers[Nickname].authenticated || String.IsNullOrEmpty(Program.TheUsers[Nickname].password)))
            {
                return true;
            }
            return false;
        }

        public static Boolean IsIgnored(String Nickname)
        {
            return Program.TheUsers[Nickname].ignored;
        }
    }
}