using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
using System;
using System.Net;
using System.Net.Http;

namespace FritzBot.Plugins
{
    [Name("Hippie")]
    class HippiePlugin : PluginBase, IBackgroundTask
    {
        private static HttpClient Client = new HttpClient();

        public void Start()
        {
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Stop()
        {
            ServerConnection.OnPostProcessingMessage += Server_OnPostProcessingMessage;
        }

        public void Server_OnPostProcessingMessage(object sender, IrcMessage theMessage)
        {
            try
            {
                if (theMessage.Message.StartsWith("$", StringComparison.Ordinal) && theMessage.Message.Length > 1)
                {
                    string requestUri = $"{ConfigHelper.GetString("HippieUrl")}?user={WebUtility.UrlEncode(theMessage.Nickname)}&from={(theMessage.IsPrivate ? "pm" : "chan")}&date={WebUtility.UrlEncode(DateTime.Now.ToString("s"))}&query={theMessage.Message.Substring(1)}";
                    using (var response = Client.GetAsync(requestUri).GetAwaiter().GetResult())
                    {
                        string answer = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (!String.IsNullOrWhiteSpace(answer))
                        {
                            theMessage.Answer(answer.TrimEnd());
                            theMessage.HandledByEvent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Fehler beim Ausführen des Hippie Query");
            }
        }
    }
}