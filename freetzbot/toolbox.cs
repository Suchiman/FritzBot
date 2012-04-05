using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace freetzbot
{
    class toolbox
    {
        private static Thread loggingthread = new Thread(new ThreadStart(log_thread));
        private static List<string> logging_list = new List<string>();
        private static Mutex logging_safe = new Mutex();

        private static void log_thread()
        {
            while (true)
            {
                try
                {
                    while (!(logging_list.Count > 0))
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
                    File.AppendAllText("log.txt", logging_list[0] + "\r\n", Encoding.GetEncoding("iso-8859-1"));
                    Console.WriteLine(logging_list[0]);
                    logging_list.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Zugriff auf den Serverlog: " + ex.Message);
                    return;
                }
            }
        }

        public static void logging(String to_log)
        {
            logging_safe.WaitOne();
            try
            {
                if (!loggingthread.IsAlive)
                {
                    loggingthread = new Thread(new ThreadStart(log_thread));
                    loggingthread.Name = "LoggingThread";
                    loggingthread.IsBackground = true;
                    loggingthread.Start();
                }
                logging_list.Add(DateTime.Now.ToString("dd.MM HH:mm:ss ") + to_log);
            }
            catch (Exception ex)
            {
                logging("Exception beim logging aufgetreten: " + ex.Message);
            }
            logging_safe.ReleaseMutex();
        }

        public static void instantiate_connection(String server, int port, String nick, String quit_message, String initial_channel)
        {
            irc connection = new irc(server, port, nick);
            connection.quit_message = quit_message;
            connection.Received += new irc.ReceivedEventHandler(freetzbot.Program.process_incomming);
            connection.AutoReconnect = true;
            connection.connect();
            Thread.Sleep(1000);
            if (initial_channel.Contains(":"))
            {
                String[] channels = initial_channel.Split(':');
                foreach (String channel in channels)
                {
                    connection.join(channel);
                }
            }
            else
            {
                connection.join(initial_channel);
            }
            freetzbot.Program.irc_connections.Add(connection);
        }

        public static String get_web(String url)
        {
            StringBuilder sb = new StringBuilder();
            Stream resStream = null;
            String tempString = null;
            Byte[] buf = new Byte[8192];
            int count = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                resStream = response.GetResponseStream();
            }
            catch (Exception ex)
            {
                logging("Exception beim Webseiten Aufruf aufgetreten: " + ex.Message);
                return "";
            }
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    tempString = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(tempString);
                }
            }
            while (count > 0);
            return sb.ToString();
        }

        public static String short_url(String url)
        {
            return get_web("http://tinyurl.com/api-create.php?url=" + url);
        }

        public static db getDatabaseByName(String name)
        {
            foreach (db database in freetzbot.Program.databases)
            {
                if (database.datenbank_name == name)
                {
                    return database;
                }
            }
            db datenbank = new db(name);
            freetzbot.Program.databases.Add(datenbank);
            return datenbank;
        }

        public static command getCommandByName(String name)
        {
            foreach (command thecommand in freetzbot.Program.commands)
            {
                foreach (String thename in thecommand.get_name())
                {
                    if (thename == name)
                    {
                        return thecommand;
                    }
                }
            }
            throw new Exception("Command not found");
        }

        public static void announce(String message)
        {
            foreach (irc connection in freetzbot.Program.irc_connections)
            {
                foreach (String room in connection.rooms)
                {
                    connection.sendmsg(message, room);
                }
            }
        }

        public static Boolean running_check()
        {
            for (int i = 0; i < freetzbot.Program.irc_connections.Count; i++)
            {
                if (freetzbot.Program.irc_connections[i].running())
                {
                    return true;
                }
            }
            return false;
        }

        public static Boolean op_check(String nickname)
        {
            if (nickname == "hippie2000" || nickname == "Suchiman")
            {
                return true;
            }
            return false;
        }

        public static Boolean ignore_check(String parameter = "")
        {
            if (toolbox.getDatabaseByName("ignore.db").Find(parameter) != -1)
            {
                return true;
            }
            return false;
        }
    }
}