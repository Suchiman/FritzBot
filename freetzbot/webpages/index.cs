using System;
using FritzBot;

namespace webpages
{
    class index : IWebInterface
    {
        public String Url { get { return "/"; } }

        public static String GenMenu(HtmlRequest request)
        {
            String menu = "";
            menu += "<div><table cellspacing=10px><tr>";
            menu += "<td><a href=\"boxdb\">BoxDB</a></td>";
            menu += "<td><a href=\"aliasdb\">Aliase</a></td>";
            menu += "<td><a href=\"/helpdb\">Hilfe</a></td>";
            if (Program.TheUsers[login.CheckLogin(request)].isOp)
            {
                menu += "<td><a href=\"/settings\">Einstellungen</a></td>";
            }
            if (String.IsNullOrEmpty(login.CheckLogin(request)))
            {
                menu += "<td><a href=\"/login\">Login</a></td>";
            }
            else
            {
                menu += "<td><a href=\"/logout\">Logout</a></td>";
            }
            menu += "</div></table></tr>";
            return menu;
        }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            theresponse.refer = "helpdb";
            return theresponse;
        }
    }
}
