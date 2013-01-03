using System;
using FritzBot;
using FritzBot.Core;

namespace webpages
{
    class settings : IWebInterface
    {
        public String Url { get { return "/settings"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            String logincheck = login.CheckLogin(request);
            if (!String.IsNullOrEmpty(logincheck))
            {
                if (UserManager.GetInstance()[logincheck].IsOp)
                {
                    if (request.postdata.Count > 0)
                    {
                        try
                        {
                            DateTime FlushIntervallS = DateTime.MinValue.AddMinutes(Convert.ToDouble(request.postdata["UserFlushIntervallM"]));
                            FlushIntervallS = FlushIntervallS.AddSeconds(Convert.ToDouble(request.postdata["UserFlushIntervallS"]));
                            DateTime LaborIntervallS = DateTime.MinValue.AddMinutes(Convert.ToDouble(request.postdata["LaborCheckIntervallM"]));
                            LaborIntervallS = LaborIntervallS.AddSeconds(Convert.ToDouble(request.postdata["LaborCheckIntervallS"]));
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").SetVariable("FloodingCount", Convert.ToInt32(request.postdata["FloodingCount"]));
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").SetVariable("FloodingCountReduction", Convert.ToInt32(request.postdata["FloodingCountReduction"]) * 1000);
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("labor").SetVariable("CheckEnabled", request.postdata.ContainsKey("LaborCheck") ? "true" : "false");
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("labor").SetVariable("Intervall", (int)LaborIntervallS.Subtract(DateTime.MinValue).TotalMilliseconds);
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("whmf").SetVariable("urlResolve", request.postdata.ContainsKey("WhmfUrlResolve") ? "true" : "false");
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("frag").SetVariable("BoxFrage", request.postdata.ContainsKey("BoxFrage") ? "true" : "false");
                            XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").SetVariable("Silence", request.postdata.ContainsKey("Silence") ? "true" : "false");
                        }
                        catch
                        {
                            theresponse.page += "<font color=\"red\">Es ist ein Fehler bei der Verarbeitung deiner Eingabe aufgetreten";
                        }
                    }
                    theresponse.page += "<div><form action=\"settings\" method=\"POST\"><table cellspacing=10px>";
                    theresponse.page += "<tr><td>Auf Labor Neuigkeiten pr&uuml;fen</td><td><input type=\"checkbox\" name=\"LaborCheck\" value=\"true\"";
                    if (XMLStorageEngine.GetManager().GetGlobalSettingsStorage("labor").GetVariable("CheckEnabled", "false") == "true")
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    DateTime LaborIntervall = DateTime.MinValue.AddMilliseconds(Convert.ToInt32(XMLStorageEngine.GetManager().GetGlobalSettingsStorage("labor").GetVariable("Intervall", "0")));
                    theresponse.page += "<tr><td>Wartezeit zwischen Labor Neuigkeiten Checks</td><td><input type=\"text\" name=\"LaborCheckIntervallM\" value=\"" + LaborIntervall.Minute + "\"> Minuten<br><input type=\"text\" name=\"LaborCheckIntervallS\" value=\"" + LaborIntervall.Second + "\"> Sekunden</td></tr>";
                    theresponse.page += "<tr><td>Die Anzahl von Nachrichten bevor die Flooding Protection anspringt</td><td><input type=\"text\" name=\"FloodingCount\" value=\"" + XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").GetVariable("FloodingCount", "0") + "\"> Nachrichten</td></tr>";
                    theresponse.page += "<tr><td>Zeit bis der interne FloodingCounter runtergez&auml;hlt wird</td><td><input type=\"text\" name=\"FloodingCountReduction\" value=\"" + Convert.ToInt32(XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").GetVariable("FloodingCountReduction", "1000")) / 1000 + "\"> Sekunden</td></tr>";
                    theresponse.page += "<tr><td>Wehavemorefun Adressen aufl&ouml;sen</td><td><input type=\"checkbox\" name=\"WhmfUrlResolve\" value=\"true\"";
                    if (XMLStorageEngine.GetManager().GetGlobalSettingsStorage("whmf").GetVariable("urlResolve", "false") == "true")
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    theresponse.page += "<tr><td>Neue Benutzer nach ihrer Box Fragen</td><td><input type=\"checkbox\" name=\"BoxFrage\" value=\"true\"";
                    if (XMLStorageEngine.GetManager().GetGlobalSettingsStorage("frag").GetVariable("BoxFrage", "false") == "true")
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    theresponse.page += "<tr><td>Nur im Query antworten</td><td><input type=\"checkbox\" name=\"Silence\" value=\"true\"";
                    if (XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").GetVariable("Silence", "false") == "true")
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    theresponse.page += "<tr><td><input type=\"submit\" value=\"Speichern\"></td></tr>";
                    theresponse.page += "</table></form></div>";
                }
                else
                {
                    theresponse.page += "Du bist nicht berechtigt, diese Seite anzuzeigen";
                }
            }
            else
            {
                theresponse.page += "Diese Seite erfordert einen Login";
            }
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
