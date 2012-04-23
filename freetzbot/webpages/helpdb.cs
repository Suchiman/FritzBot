using System;
using FritzBot;

namespace webpages
{
    class helpdb : IWebInterface
    {
        public String Url { get { return "/helpdb"; } }

        public html_response GenPage(html_request request)
        {
            html_response theresponse = new html_response();
            String logincheck = login.CheckLogin(request);
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu();
            theresponse.page += "<table border=2px>";
            theresponse.page += "<tr><td><b>Befehl</b></td><td><b>Beschreibung</b></td></tr>";
            foreach (ICommand thecommand in Program.commands)
            {
                if (!(thecommand.OpNeeded && !toolbox.IsOp(logincheck)))
                {
                    String names = "";
                    foreach (String name in thecommand.Name)
                    {
                        names += ", " + name;
                    }
                    names = names.Remove(0, 2);
                    theresponse.page += "<tr><td>" + names + "</td><td>" + thecommand.HelpText + "</td></tr>";
                }
            }
            theresponse.page += "</table>";
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}