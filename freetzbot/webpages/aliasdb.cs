using System;
using FritzBot;

namespace webpages
{
    class aliasdb : IWebInterface
    {
        public String Url { get { return "/aliasdb"; } }

        public html_response GenPage(html_request request)
        {
            html_response theresponse = new html_response();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu();
            theresponse.page += "<table border=2px>";
            theresponse.page += "<tr><td><b>Alias</b></td><td><b>Beschreibung</b></td></tr>";
            AliasDB thealiases = Program.TheUsers.AllAliases();
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
