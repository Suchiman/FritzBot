using System;
using FritzBot;
using FritzBot.Core;

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
            if (UserManager.GetInstance().Exists(name))
            {
                UserManager.GetInstance()[name].GetModulUserStorage("login").SetVariable("authcookiedate", DateTime.MinValue);
                theresponse.cookies["username"] = "";
                theresponse.cookies["logindata"] = "";
                theresponse.page += "Du bist jetzt ausgeloggt";
            }
            else
            {
                theresponse.page += "Du bist gar nicht eingeloggt";
            }
            theresponse.page += "</div></body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
