using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace FritzBot.commands
{
    [Module.Name("labor")]
    [Module.Help("Gibt Informationen zu den aktuellen Labor Firmwares aus: !labor <boxnummer>")]
    class labor : ICommand, IBackgroundTask
    {
        public void Start()
        {
            laborthread = new Thread(new ThreadStart(this.LaborCheck))
            {
                Name = "LaborThread",
                IsBackground = true
            };
            laborthread.Start();
        }

        public void Stop()
        {
            laborthread.Abort();
        }

        private List<Labordaten> LaborDaten = new List<Labordaten>();
        private DateTime LaborDatenUpdate = DateTime.MinValue;
        Thread laborthread;

        private void UpdateLaborCache()
        {
            LaborDaten = new List<Labordaten>();
            String page = ReadyHtmlForDocument(toolbox.GetWeb("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
            if (String.IsNullOrEmpty(page))
            {
                throw new InvalidOperationException("Verbindungsfehler");
            }
            XmlDocument dies = new XmlDocument();
            dies.LoadXml(page);
            XmlNodeList huh = dies.SelectNodes("//h2");
            foreach (XmlNode item in huh)
            {
                XmlNode aktuellerSibling = item.NextSibling;
                while (aktuellerSibling.NextSibling != null && !String.IsNullOrEmpty(aktuellerSibling.NextSibling.InnerText))
                {
                    aktuellerSibling = aktuellerSibling.NextSibling;
                    if (aktuellerSibling is XmlComment) continue;

                    Labordaten daten = new Labordaten();
                    daten.url = aktuellerSibling.SelectSingleNode(".//a/@href").Value.Trim();
                    String newPage = ReadyHtmlForDocument(toolbox.GetWeb("http://www.avm.de/de/Service/Service-Portale/Labor/" + daten.url));
                    XmlDocument ver = new XmlDocument();
                    while (true)
                    {
                        try
                        {
                            ver.LoadXml(newPage);
                            break;
                        }
                        catch (XmlException ex)
                        {
                            newPage = newPage.Remove(ex.LinePosition - 3, newPage.Substring(ex.LinePosition - 3).IndexOf('>') + 1);
                            continue;
                        }
                    }
                    XmlNodeList table = ver.SelectNodes("//table[@style=\"text-align:left; width:350px; float:left;\"]/tr[2]/td/text()");
                    daten.typ = ver.SelectSingleNode("//h3[contains(@id, 'H')]/text()").Value.Trim();
                    daten.typ = daten.typ.Substring(daten.typ.LastIndexOf(' ')).Trim();
                    daten.version = table[0].Value.Trim();
                    daten.datum = table[1].Value.Trim();
                    LaborDaten.Add(daten);
                }
            }
            LaborDatenUpdate = DateTime.Now;
        }

        private String ReadyHtmlForDocument(String html)
        {
            html = html.Replace("&ndash;", "–").Replace("&nbsp;", "").Replace("&uuml;", "ü").Replace("&auml;", "ä").Replace("&copy;", "©").Replace("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"de\">", "<html>").Replace("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "<!DOCTYPE html>");
            while (html.Contains("<br>") || html.Contains("\r") || html.Contains("\n") || html.Contains("\t") || html.Contains("  "))
            {
                html = html.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ", " ").Replace("<br>", "");
            }
            while (html.Contains("<script") && html.Contains("</script"))
            {
                html = html.Remove(html.IndexOf("<script"), html.Substring(html.IndexOf("<script")).IndexOf("</script") + 8);
            }
            while (html.Contains("<img"))
            {
                html = html.Remove(html.IndexOf("<img"), html.Substring(html.IndexOf("<img")).IndexOf(">") + 1);
            }
            return html;
        }

        public void Run(ircMessage theMessage)
        {
            String changeset = "";
            try
            {
                UpdateLaborCache();
            }
            catch (Exception ex)
            {
                if (LaborDatenUpdate == DateTime.MinValue)
                {
                    theMessage.Answer("Ich konnte leider keine Daten von der Laborwebseite abrufen und mein Cache ist leer");
                    return;
                }
                theMessage.Answer("Es war mir nicht möglich den Labor Cache zu erneuern. Grund: " + ex.Message + ". Verwende Cache vom " + LaborDatenUpdate.ToString());
            }
            if (String.IsNullOrEmpty(theMessage.CommandLine.ToLower()))
            {
                foreach (Labordaten item in LaborDaten)
                {
                    changeset += ", " + item.typ + ": " + item.datum;
                }
                changeset = "Aktuelle Labor Daten: " + changeset.Remove(0, 2) + " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
            }
            else
            {
                foreach (Labordaten item in LaborDaten)
                {
                    if (item.typ.ToLower().Contains(theMessage.CommandArgs[0].ToLower()) || theMessage.CommandArgs[0].ToLower().Contains(item.typ.ToLower()))
                    {
                        changeset += "Die neueste " + item.typ + " labor Version ist am " + item.datum + " erschienen mit der Versionsnummer: " + item.version + ". Laborseite: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/" + item.url);
                    }
                }
            }
            theMessage.Answer(changeset);
        }

        private void LaborCheck()
        {
            List<Labordaten> alte = null;
            String output = "";
            while (true)
            {
                if (Properties.Settings.Default.LaborCheck)
                {
                    do
                    {
                        try
                        {
                            UpdateLaborCache();
                            alte = LaborDaten;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                            if (ex is IndexOutOfRangeException)
                            {
                                Program.TheServers.AnnounceGlobal("Mir ist bei der Verarbeitung der AVM Labor Webseite ein Fehler unterlaufen der typisch dafür ist, dass AVM etwas hinzugefügt oder entfernt hat. Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php"));
                                return;
                            }
                        }
                    } while (true);
                    List<Labordaten> neue = LaborDaten;
                    String labors = "";
                    List<Labordaten> unEquals = new List<Labordaten>();
                    foreach (Labordaten neueDaten in neue)
                    {
                        bool equals = false;
                        foreach (Labordaten alteDaten in alte)
                        {
                            if(alteDaten.Equals(neueDaten))
                            {
                                equals = true;
                                break;
                            }
                        }
                        if (!equals)
                        {
                            unEquals.Add(neueDaten);
                        }
                    }
                    foreach (Labordaten item in unEquals)
                    {
                        labors += ", " + item.typ;
                    }
                    if (!String.IsNullOrEmpty(labors))
                    {
                        if (!String.IsNullOrEmpty(output))
                        {
                            output += ", Neue Labor Versionen gesichtet! -" + labors.Remove(0, 1);
                        }
                        else
                        {
                            output += "Neue Labor Versionen gesichtet! -" + labors.Remove(0, 1);
                        }
                    }
                    if (!String.IsNullOrEmpty(output))
                    {
                        output += " - Zum Labor: " + toolbox.ShortUrl("http://www.avm.de/de/Service/Service-Portale/Labor/index.php");
                        alte = neue;
                        Program.TheServers.AnnounceGlobal(output);
                    }
                    output = "";
                    Thread.Sleep(Properties.Settings.Default.LaborCheckIntervall);
                }
                else
                {
                    Thread.Sleep(30000);
                }
            }
        }
    }
}
namespace FritzBot
{
    struct Labordaten : IEquatable<Labordaten>
    {
        public String typ;
        public String datum;
        public String version;
        public String url;

        public override int GetHashCode()
        {
            return datum.GetHashCode() ^ version.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Labordaten))
                return false;

            return Equals((Labordaten)obj);
        }

        public bool Equals(Labordaten other)
        {
            return version == other.version && datum == other.datum && url == other.url && typ == other.typ;
        }

        public static bool operator ==(Labordaten labordaten1, Labordaten labordaten2)
        {
            return labordaten1.Equals(labordaten2);
        }

        public static bool operator !=(Labordaten labordaten1, Labordaten labordaten2)
        {
            return !labordaten1.Equals(labordaten2);
        }
    }
}