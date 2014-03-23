using FritzBot;
using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace webpages
{
    class logout : IWebInterface
    {
        public string Url { get { return "/logout"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            string name = "";
            if (request.cookies["username"] != null)
            {
                name = request.cookies["username"].Value;
            }
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(name);
                if (u != null)
                {
                    SimpleStorage storage = db.GetSimpleStorage(u, "login");
                    storage.Store("authcookiedate", DateTime.MinValue);
                    db.SaveOrUpdate(storage);
                    theresponse.cookies["username"] = "";
                    theresponse.cookies["logindata"] = "";
                    theresponse.page += "Du bist jetzt ausgeloggt";
                }
                else
                {
                    theresponse.page += "Du bist gar nicht eingeloggt";
                }
            }
            theresponse.page += "</div></body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
