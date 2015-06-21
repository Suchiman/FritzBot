using AngleSharp;
using AngleSharp.Dom;
using FritzBot.DataModel;
using Serilog;
using System;
using System.Text.RegularExpressions;

namespace FritzBot.Plugins
{
    [Name("trunk")]
    [Help("Dies zeigt den aktuellsten Changeset an.")]
    [ParameterRequired(false)]
    class trunk : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            try
            {
                IDocument document = BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync("http://freetz.org/changeset").Result;
                string changeset = document.QuerySelector("#title").TextContent.Trim().Split(' ')[1];
                string datum = Regex.Replace(document.QuerySelector("#overview dd.time").TextContent.Trim().Replace("\n", ""), "[ ]{2,}", " ");
                theMessage.Answer(String.Format("Der aktuellste Changeset ist {0} und wurde am {1} in den Trunk eingecheckt. Siehe: http://freetz.org/changeset", changeset, datum));
            }
            catch (Exception ex)
            {
                theMessage.Answer("Das parsen oder Zugreifen auf die Freetz Webseite schlug fehl. Möglicherweise wurde die Struktur geändert oder es trat ein anderer Fehler auf.");
                Log.Error(ex, "Zugriff auf die Freetz Webseite fehlgeschlagen");
            }
        }
    }
}