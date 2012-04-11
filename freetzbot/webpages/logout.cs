using System;
using System.Net;

namespace freetzbot.webpages
{
    class logout : pageinterface
    {
        public String get_url()
        {
            return "/logout";
        }

        public html_response gen_page(html_request request)
        {
            html_response theresponse = new html_response();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.gen_menu();
            String name = "";
            if (request.cookies["username"] != null)
            {
                name = request.cookies["username"].Value;
            }
            if (freetzbot.Program.TheUsers.Exists(name))
            {
                freetzbot.Program.TheUsers[name].authcookiedate = DateTime.MinValue;
                Cookie username = new Cookie("username", "");
                username.Expires = DateTime.Now.AddDays(-1d);
                Cookie logindata = new Cookie("logindata", "");
                logindata.Expires = DateTime.Now.AddDays(-1d);
                theresponse.cookies.Add(username);
                theresponse.cookies.Add(logindata);
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
