using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.webpages
{
    class boxdb : pageinterface
    {
        public String get_url()
        {
            return "/boxdb";
        }

        public html_response gen_page(html_request request)
        {
            html_response theresponse = new html_response();
            String logincheck = login.check_login(request);
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.gen_menu();
            if (logincheck != "")
            {
                theresponse.page += "<table border=2px>";
                theresponse.page += "<tr><td><b>Besitzer</b></td><td><b>Boxen</b></td></tr>";
                foreach (User theuser in freetzbot.Program.TheUsers)
                {
                    if (!(theuser.boxes.Count > 0))
                    {
                        continue;
                    }
                    String boxes = "";
                    String names = "";
                    foreach (String thename in theuser.names)
                    {
                        names += ", " + thename;
                    }
                    names = names.Remove(0, 2);
                    foreach (String tddata in theuser.boxes)
                    {
                        boxes += ", " + tddata;
                    }
                    if (boxes.Length > 0)
                    {
                        boxes = boxes.Remove(0, 2);
                    }
                    theresponse.page += "<tr><td>" + names + "</td><td>" + boxes + "</td></tr>";
                }
                theresponse.page += "</table>";
            }
            else
            {
                theresponse.page += "Logge dich bitte zuerst ein <a href=\"/\">zum Login</a>";
            }
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
