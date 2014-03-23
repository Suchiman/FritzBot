using FritzBot;
using FritzBot.Core;
using FritzBot.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace webpages
{
    class aliasdb : IWebInterface
    {
        public string Url { get { return "/aliasdb"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            theresponse.page += "<table border=2px>";
            theresponse.page += "<tr><td><b>Alias</b></td><td><b>Beschreibung</b></td></tr>";
            using (DBProvider db = new DBProvider())
            {
                List<AliasEntry> aliase = db.Query<AliasEntry>().OrderBy(x => x.Key).ToList();
                foreach (AliasEntry alias in aliase)
                {
                    theresponse.page += "<tr><td>" + alias.Key + "</td><td>" + alias.Text + "</td></tr>";
                }
            }
            theresponse.page += "</table>";
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
