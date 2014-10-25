using CsQuery;
using FritzBot.DataModel;
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
                CQ freetz = CQ.CreateFromUrl("http://freetz.org/changeset");
                string changeset = freetz.Select("#title").Text().Trim().Split(' ')[1];
                string datum = Regex.Replace(freetz.Select("#overview").Find("dd.time").Text().Trim().Replace("\n", ""), "[ ]{2,}", " ");
                theMessage.Answer(String.Format("Der aktuellste Changeset ist {0} und wurde am {1} in den Trunk eingecheckt. Siehe: http://freetz.org/changeset", changeset, datum));
            }
            catch (Exception ex)
            {
                theMessage.Answer("Leider war es mir nicht möglich auf die Freetz Webseite zuzugreifen");
                //theMessage.Answer("Das parsen der Freetz Webseite schlug fehl. Möglicherweise wurde die Struktur geändert oder es trat ein anderer Fehler auf.");
                toolbox.Logging(ex);
            }
        }
    }
}