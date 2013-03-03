using FritzBot.Core;
using FritzBot.DataModel;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Module.Name("trunk")]
    [Module.Help("Dies zeigt den aktuellsten Changeset an.")]
    [Module.ParameterRequired(false)]
    class trunk : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string webseite = toolbox.GetWeb("http://freetz.org/changeset");
            if (!String.IsNullOrEmpty(webseite))
            {
                try
                {
                    HtmlNode content = HtmlDocumentExtensions.GetHtmlNode(webseite).Descendants("div").First(x => x.GetAttributeValue("id", "").Equals("content") && x.GetAttributeValue("class", "").Equals("changeset"));
                    string changeset = content.Elements("div").First(x => x.GetAttributeValue("id", "").Equals("title")).Element("h1").InnerText.Split(' ')[1];
                    string datum = content.Elements("dl").First(x => x.GetAttributeValue("id", "").Equals("overview")).Elements("dd").First(x => x.GetAttributeValue("class", "").Equals("time")).InnerText;
                    theMessage.Answer(String.Format("Der aktuellste Changeset ist {0} und wurde am {1} in den Trunk eingecheckt. Siehe: http://freetz.org/changeset", changeset, Regex.Replace(datum.Trim().Replace("\n", ""), "[ ]{2,}", " ")));
                }
                catch (Exception ex)
                {
                    theMessage.Answer("Das parsen der Freetz Webseite schlug fehl. Möglicherweise wurde die Struktur geändert oder es trat ein anderer Fehler auf.");
                    toolbox.Logging(ex);
                }
            }
            else
            {
                theMessage.Answer("Leider war es mir nicht möglich auf die Freetz Webseite zuzugreifen");
            }
        }
    }
}