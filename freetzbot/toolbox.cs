using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.CSharp;

namespace FritzBot
{
    public static class toolbox
    {
        private static Thread LoggingThread = new Thread(new ThreadStart(LogThread));
        private static Queue<String> LoggingList = new Queue<String>();
        private static readonly object _LogThreadLocker = new object();
        private static readonly object _LoggingLocker = new object();

        private static void LogThread()
        {
            while (true)
            {
                try
                {
                    lock (_LogThreadLocker)
                    {
                        while (!(LoggingList.Count > 0))
                        {
                            Monitor.Wait(_LogThreadLocker);
                        }
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
            lock (_LogThreadLocker)
            {
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
                    Monitor.Pulse(_LogThreadLocker);
                }
                catch (Exception ex)
                {
                    Logging("Exception beim logging aufgetreten: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Hasht einen String mit dem SHA512 Algorithmus
        /// </summary>
        /// <param name="toCrypt">Der zu hashende String</param>
        /// <returns>Den Hashwert des Strings</returns>
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

        /// <summary>
        /// Baut unter Verwendung der angegebenen Daten eine neue Verbindung auf
        /// </summary>
        /// <param name="server">Der Hostname des Servers</param>
        /// <param name="Port">Der Port des Servers, standardmäßig 6667</param>
        /// <param name="Nick">Der Nickname des Bots</param>
        /// <param name="Quit_Message">Die Nachricht beim Verlassen</param>
        /// <param name="Channels">Eine Liste von Channelnamen die der Bot betreten soll</param>
        public static void InstantiateConnection(String server, int Port, String Nick, String Quit_Message, String Channel)
        {
            List<String> channels = new List<String>() { { Channel } };
            Program.TheServers.NewConnection(server, Port, Nick, Quit_Message, channels);
        }

        /// <summary>
        /// Kompiliert Quellcode im Arbeitsspeicher zu einem Assembly
        /// </summary>
        /// <param name="fileName">Ein Array das die Dateinamen enthält</param>
        /// <returns>Das aus den Quellcode erstellte Assembly</returns>
        public static Assembly LoadSource(String[] fileName)
        {
            CSharpCodeProvider compiler = new CSharpCodeProvider();
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.dll");
            compilerParams.ReferencedAssemblies.Add("System.Web.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().GetName().Name + ".exe");
            CompilerResults results = null;
            try
            {
                results = compiler.CompileAssemblyFromFile(compilerParams, fileName);
            }
            catch (Exception ex)
            {
                Logging(ex.Message);
            }
            if (results.Errors.Count > 0)
            {
                foreach (CompilerError theError in results.Errors)
                {
                    Logging("Compilerfehler: " + theError.ErrorText);
                }
                throw new Exception("Compilation failed");
            }
            return results.CompiledAssembly;
        }

        /// <summary>
        /// Sendet eine HTTP Anfrage um die gewünschte Webseite in form eines Strings zu erhalten
        /// </summary>
        /// <param name="Url">Die http WebAdresse</param>
        /// <param name="POSTParams">Optionale POST Parameter</param>
        /// <returns>Die Webseite als String</returns>
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

        /// <summary>
        /// Kürzt eine URL bei TinyURL
        /// </summary>
        /// <param name="Url">Die zu kürzende URL</param>
        /// <returns>Die gekürzte URL</returns>
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

        /// <summary>
        /// Codiert den String entsprechend den Anforderungen einer URL
        /// </summary>
        /// <param name="url">Der zu Codierende String</param>
        /// <returns>Einen URL Encodierten String</returns>
        public static String UrlEncode(String url)
        {
            return System.Web.HttpUtility.UrlEncode(url, Encoding.UTF8);
        }

        /// <summary>
        /// Sucht nach der Instanz des Kommandos, wirft eine ArgumentException wenn das Kommando nicht gefunden wird
        /// </summary>
        /// <param name="Name">Der name des Kommandos</param>
        /// <returns>Das gefundene Kommando Objekt</returns>
        public static ICommand getCommandByName(String Name)
        {
            foreach (ICommand theCommand in Program.Commands)
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

        public static Boolean IsOp(String Nickname)
        {
            if (Program.TheUsers.Exists(Nickname))
            {
                if (Program.TheUsers[Nickname].IsOp && (Program.TheUsers[Nickname].Authenticated || String.IsNullOrEmpty(Program.TheUsers[Nickname].password)))
                {
                    return true;
                }
            }
            return false;
        }

        public static Boolean IsIgnored(String Nickname)
        {
            return Program.TheUsers[Nickname].ignored;
        }
    }
}