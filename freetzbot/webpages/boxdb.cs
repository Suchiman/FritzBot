using FritzBot;
using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace webpages
{
    class boxdb : IWebInterface
    {
        public string Url { get { return "/boxdb"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            string LoginCheck = login.CheckLogin(request);
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            if (!String.IsNullOrEmpty(LoginCheck))
            {
                theresponse.page += "<table border=2px>";
                theresponse.page += "<tr><td><b>Besitzer</b></td><td><b>Boxen</b></td></tr>";
                using (DBProvider db = new DBProvider())
                {
                    foreach (BoxEntry entry in db.Query<BoxEntry>().Where(x => x.Reference != null))
                    {
                        theresponse.page += "<tr><td>" + String.Join(", ", entry.Reference.Names) + "</td><td>" + String.Join(", ", entry.GetRawUserBoxen()) + "</td></tr>";
                    }
                }
                theresponse.page += "</table>";
            }
            else
            {
                theresponse.page += "Logge dich bitte zuerst ein <a href=\"login\">zum Login</a>";
            }
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
