using System;
using System.Net;

namespace FritzBot.webpages
{
    class logout : IWebInterface
    {
        public String Url { get { return "/logout"; } }

        public html_response GenPage(html_request request)
        {
            html_response theresponse = new html_response();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu();
            String name = "";
            if (request.cookies["username"] != null)
            {
                name = request.cookies["username"].Value;
            }
            if (FritzBot.Program.TheUsers.Exists(name))
            {
                FritzBot.Program.TheUsers[name].authcookiedate = DateTime.MinValue;
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
