using FritzBot.Core;
using FritzBot.DataModel;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Module.Name("title")]
    [Module.Help("Ruft den Titel der Webseite ab")]
    [Module.ParameterRequired]
    class title : PluginBase, IBackgroundTask
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                string link = theMessage.CommandArgs.FirstOrDefault(x => x.StartsWith("http"));
                string webseite = toolbox.GetWeb(link);
                HtmlNode titleNode = HtmlDocumentExtensions.GetHtmlNode(webseite).SelectSingleNode("//head/title");
                string title = Regex.Replace(titleNode.InnerText.Trim().Replace("\n", "").Replace("\r", ""), "[ ]{2,}", " ");
                theMessage.Answer(title);
            }
            catch { }
        }

        public void Start()
        {
            Program.UserMessaged += Run;
        }

        public void Stop()
        {
            Program.UserMessaged -= Run;
        }
    }
}