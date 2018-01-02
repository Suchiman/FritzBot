using FritzBot.Core;
using FritzBot.Database;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace FritzBot
{
    public static class Toolbox
    {
        /// <summary>
        /// Hasht einen string mit dem SHA512 Algorithmus
        /// </summary>
        /// <param name="toCrypt">Der zu hashende String</param>
        /// <returns>Den Hashwert des Strings</returns>
        public static string Crypt(string toCrypt)
        {
            byte[] hash = null;
            byte[] tocode = Encoding.UTF8.GetBytes(toCrypt.ToCharArray());
            using (SHA512 theCrypter = new SHA512Managed())
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
        public static void InstantiateConnection(string server, int Port, string Nick, string Quit_Message, string Channel)
        {
            ServerConnection Connection = ServerManager.NewConnection(server, Port, Nick, Quit_Message, new List<string>() { Channel });
            Connection.Connect();
        }

        /// <summary>
        /// Sendet eine HTTP Anfrage um die gewünschte Webseite in form eines Strings zu erhalten
        /// </summary>
        /// <param name="Url">Die http WebAdresse</param>
        /// <param name="POSTParams">Optionale POST Parameter</param>
        /// <returns>Die Webseite als String</returns>
        public static string GetWeb(string Url, Dictionary<string, string> POSTParams = null)
        {
            string POSTData = "";
            if (POSTParams != null)
            {
                foreach (string key in POSTParams.Keys)
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
                Log.Error(ex, "Exception beim Webseiten Aufruf");
                return "";
            }
            string theEncoding = "UTF-8";
            if (!String.IsNullOrEmpty(response.CharacterSet))
            {
                theEncoding = response.CharacterSet;
            }
            StreamReader resStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(theEncoding, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback));
            string thepage = resStream.ReadToEnd();
            resStream.Close();
            response.Close();

            return thepage;
        }

        /// <summary>
        /// Kürzt eine URL bei TinyURL
        /// </summary>
        /// <param name="Url">Die zu kürzende URL</param>
        /// <returns>Die gekürzte URL</returns>
        public static string ShortUrl(string Url)
        {
            try
            {
                return GetWeb("http://tinyurl.com/api-create.php?url=" + Url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Konnte {Url} nicht kürzen", Url);
                return Url;
            }
        }

        /// <summary>
        /// Codiert den string entsprechend den Anforderungen einer URL
        /// </summary>
        /// <param name="url">Der zu Codierende String</param>
        /// <returns>Einen URL Encodierten String</returns>
        public static string UrlEncode(string url)
        {
            Contract.Requires(url != null);

            return HttpUtility.UrlEncode(url, Encoding.UTF8);
        }

        public static bool IsOp(User user)
        {
            Contract.Requires(user != null);

            if (user.Admin && (user.Authenticated || String.IsNullOrEmpty(user.Password)))
            {
                return true;
            }
            return false;
        }

        public static T GetAttribute<T>(object obj)
        {
            Contract.Requires(obj != null);

            return GetAttribute<T>(obj.GetType());
        }

        public static T GetAttribute<T>(Type type)
        {
            Contract.Requires(type != null);

            Attribute Attr = Attribute.GetCustomAttribute(type, typeof(T));
            if (Attr == null)
            {
                return default(T);
            }
            return (T)(Attr as object);
        }

        public static Thread SafeThreadStart(string name, bool restartOnException, Action method)
        {
            Contract.Requires(method != null);
            Contract.Ensures(Contract.Result<Thread>() != null);

            Thread t = new Thread(() =>
            {
                do
                {
                    try
                    {
                        method();
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (ex is ThreadAbortException)
                        {
                            return;
                        }
                        Log.Error(ex, "Fehler beim ausführen eines Threads");
                    }
                } while (restartOnException);
            })
            {
                IsBackground = true,
                Name = name ?? String.Empty
            };
            t.Start();
            return t;
        }
    }
}