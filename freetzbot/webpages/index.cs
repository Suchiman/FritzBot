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
                theresponse.page += "<div style=\"position: absolute;top: 35%;left: 35%;border:1px;border-style:dotted;padding:10px\">Bitte einloggen!<br>";
                theresponse.page += "<form action=\"login\" method=\"POST\"><table>";
                theresponse.page += "<tr><td>IRC-Nick:</td><td><input type=\"text\" name=\"name\"></td></tr>";
                theresponse.page += "<tr><td>Passwort:</td><td><input type=\"password\" name=\"pw\"></td></tr>";
                theresponse.page += "<tr><td><input type=\"submit\" value=\"Login\"></td></tr>";
                theresponse.page += "</table></form></div>";
            }
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
