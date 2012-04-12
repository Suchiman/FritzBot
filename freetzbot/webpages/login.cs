using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace freetzbot.webpages
{
    class login : pageinterface
    {
        public String get_url()
        {
            return "/login";
        }

        public html_response gen_page(html_request request)
        {
            html_response theresponse = new html_response();
            if (request.postdata.Count > 0)
            {
                String name = request.postdata["name"];
                String passwort = request.postdata["pw"];
                theresponse.page += "<!DOCTYPE html><html><body>";
                theresponse.page += index.gen_menu();
                if (freetzbot.Program.TheUsers.Exists(name))
                {
                    if (freetzbot.Program.TheUsers[name].CheckPassword(passwort))
                    {
                        freetzbot.Program.TheUsers[name].authcookiedate = DateTime.Now;
                        String hash = toolbox.crypt(name + freetzbot.Program.TheUsers[name].authcookiedate.ToString() + request.useradress.ToString());
                        theresponse.cookies["username"] = name;
                        theresponse.cookies["logindata"] = hash;
                        theresponse.page += "Du bist nun eingeloggt " + name;
                    }
                    else
                    {
                        theresponse.page += "Entweder der angegebene Benutzer konnte nicht gefunden werden oder dein Passwort ist falsch";
                    }
                }
                else
                {
                    theresponse.page += "Entweder der angegebene Benutzer konnte nicht gefunden werden oder dein Passwort ist falsch";
                }
                theresponse.page += "</div></body></html>";
            }
            theresponse.status_code = 200;
            return theresponse;
        }

        public static String check_login(html_request request)
        {
            String name = "";
            String hash = "";
            if (request.cookies["username"] != null && request.cookies["logindata"] != null)
            {
                name = request.cookies["username"].Value;
                hash = request.cookies["logindata"].Value;
            }
            if (freetzbot.Program.TheUsers.Exists(name))
            {
                String calchash = toolbox.crypt(name + freetzbot.Program.TheUsers[name].authcookiedate.ToString() + request.useradress.ToString());
                if (calchash == hash)
                {
                    return name;
                }
            }
            return "";
        }
    }
}
