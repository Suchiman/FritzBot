using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.webpages
{
    class aliasdb : pageinterface
    {
        public String get_url()
        {
            return "/aliasdb";
        }

        public html_response gen_page(html_request request)
        {
            html_response theresponse = new html_response();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.gen_menu();
            theresponse.page += "<table border=2px>";
            theresponse.page += "<tr><td><b>Alias</b></td><td><b>Beschreibung</b></td></tr>";
            alias_db thealiases = freetzbot.Program.TheUsers.AllAliases();
            for (int i = 0; i < thealiases.alias.Count; i++)
            {
                theresponse.page += "<tr><td>" + thealiases.alias[i] + "</td><td>" + thealiases.description[i] + "</td></tr>";
            }
            theresponse.page += "</table>";
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
