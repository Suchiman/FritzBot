using System;
using FritzBot;

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
                if (Program.TheUsers[logincheck].isOp)
                {
                    if (request.postdata.Count > 0)
                    {
                        try
                        {
                            DateTime FlushIntervallS = DateTime.MinValue.AddMinutes(Convert.ToDouble(request.postdata["UserFlushIntervallM"]));
                            FlushIntervallS = FlushIntervallS.AddSeconds(Convert.ToDouble(request.postdata["UserFlushIntervallS"]));
                            DateTime LaborIntervallS = DateTime.MinValue.AddMinutes(Convert.ToDouble(request.postdata["LaborCheckIntervallM"]));
                            LaborIntervallS = LaborIntervallS.AddSeconds(Convert.ToDouble(request.postdata["LaborCheckIntervallS"]));
                            FritzBot.Properties.Settings.Default.UserFlushIntervall = (int)FlushIntervallS.Subtract(DateTime.MinValue).TotalMilliseconds;
                            FritzBot.Properties.Settings.Default.FloodingCount = Convert.ToInt32(request.postdata["FloodingCount"]);
                            FritzBot.Properties.Settings.Default.FloodingCountReduction = Convert.ToInt32(request.postdata["FloodingCountReduction"]) * 1000;
                            FritzBot.Properties.Settings.Default.LaborCheck = request.postdata.ContainsKey("LaborCheck");
                            FritzBot.Properties.Settings.Default.LaborCheckIntervall = (int)LaborIntervallS.Subtract(DateTime.MinValue).TotalMilliseconds;
                            FritzBot.Properties.Settings.Default.WhmfUrlResolve = request.postdata.ContainsKey("WhmfUrlResolve");
                            FritzBot.Properties.Settings.Default.BoxFrage = request.postdata.ContainsKey("BoxFrage");
                            FritzBot.Properties.Settings.Default.Silence = request.postdata.ContainsKey("Silence");
                            FritzBot.Properties.Settings.Default.Save();
                        }
                        catch
                        {
                            theresponse.page += "<font color=\"red\">Es ist ein Fehler bei der Verarbeitung deiner Eingabe aufgetreten";
                            FritzBot.Properties.Settings.Default.Reload();
                        }
                    }
                    theresponse.page += "<div><form action=\"settings\" method=\"POST\"><table cellspacing=10px>";
                    theresponse.page += "<tr><td>Auf Labor Neuigkeiten pr&uuml;fen</td><td><input type=\"checkbox\" name=\"LaborCheck\" value=\"true\"";
                    if (FritzBot.Properties.Settings.Default.LaborCheck)
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    DateTime LaborIntervall = DateTime.MinValue.AddMilliseconds(FritzBot.Properties.Settings.Default.LaborCheckIntervall);
                    theresponse.page += "<tr><td>Wartezeit zwischen Labor Neuigkeiten Checks</td><td><input type=\"text\" name=\"LaborCheckIntervallM\" value=\"" + LaborIntervall.Minute + "\"> Minuten<br><input type=\"text\" name=\"LaborCheckIntervallS\" value=\"" + LaborIntervall.Second + "\"> Sekunden</td></tr>";
                    DateTime FlushIntervall = DateTime.MinValue.AddMilliseconds(FritzBot.Properties.Settings.Default.UserFlushIntervall);
                    theresponse.page += "<tr><td>Die Zeit zwischen Speichervorg&auml;ngen der Benutzerdatenbank</td><td><input type=\"text\" name=\"UserFlushIntervallM\" value=\"" + FlushIntervall.Minute + "\"> Minuten<br><input type=\"text\" name=\"UserFlushIntervallS\" value=\"" + FlushIntervall.Second + "\"> Sekunden</td></tr>";
                    theresponse.page += "<tr><td>Die Anzahl von Nachrichten bevor die Flooding Protection anspringt</td><td><input type=\"text\" name=\"FloodingCount\" value=\"" + FritzBot.Properties.Settings.Default.FloodingCount + "\"> Nachrichten</td></tr>";
                    theresponse.page += "<tr><td>Zeit bis der interne FloodingCounter runtergez&auml;hlt wird</td><td><input type=\"text\" name=\"FloodingCountReduction\" value=\"" + FritzBot.Properties.Settings.Default.FloodingCountReduction / 1000 + "\"> Sekunden</td></tr>";
                    theresponse.page += "<tr><td>Wehavemorefun Adressen aufl&ouml;sen</td><td><input type=\"checkbox\" name=\"WhmfUrlResolve\" value=\"true\"";
                    if (FritzBot.Properties.Settings.Default.WhmfUrlResolve)
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    theresponse.page += "<tr><td>Neue Benutzer nach ihrer Box Fragen</td><td><input type=\"checkbox\" name=\"BoxFrage\" value=\"true\"";
                    if (FritzBot.Properties.Settings.Default.BoxFrage)
                    {
                        theresponse.page += " checked";
                    }
                    theresponse.page += "></td></tr>";
                    theresponse.page += "<tr><td>Nur im Query antworten</td><td><input type=\"checkbox\" name=\"Silence\" value=\"true\"";
                    if (FritzBot.Properties.Settings.Default.Silence)
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
