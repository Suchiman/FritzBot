using System;

namespace freetzbot.webpages
{
    class index : pageinterface
    {
        public String get_url()
        {
            return "/";
        }

        public static String gen_menu()
        {
            String menu = "";
            menu += "<div><table cellspacing=10px><tr>";
            menu += "<td><a href=\"/\">Startseite</a></td>";
            menu += "<td><a href=\"boxdb\">BoxDB</a></td>";
            menu += "<td><a href=\"aliasdb\">Aliase</a></td>";
            menu += "<td><a href=\"/helpdb\">Hilfe</a></td>";
            menu += "<td><a href=\"/logout\">Logout</a></td>";
            menu += "</div></table></tr>";
            return menu;
        }

        public html_response gen_page(html_request request)
        {
            html_response theresponse = new html_response();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += gen_menu();
            String logincheck = login.check_login(request);
            if (logincheck != "")
            {
                theresponse.page += "Willkommen " + logincheck;
            }
            else
            {
                theresponse.page += "<div style=\"position: absolute;top: 40%;left: 40%;\">Bitte einloggen!<br>";
                theresponse.page += "<form action=\"login\" method=\"POST\">";
                theresponse.page += "<input type=\"text\" name=\"name\"><br>";
                theresponse.page += "<input type=\"password\" name=\"pw\"><br>";
                theresponse.page += "<input type=\"submit\" value=\"Login\">";
                theresponse.page += "</form></div>";
            }
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
