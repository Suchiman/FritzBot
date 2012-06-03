using System;
using FritzBot;

namespace webpages
{
    class boxdb : IWebInterface
    {
        public String Url { get { return "/boxdb"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            String LoginCheck = login.CheckLogin(request);
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            if (!String.IsNullOrEmpty(LoginCheck))
            {
                theresponse.page += "<table border=2px>";
                theresponse.page += "<tr><td><b>Besitzer</b></td><td><b>Boxen</b></td></tr>";
                foreach (User theuser in Program.TheUsers)
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
                theresponse.page += "Logge dich bitte zuerst ein <a href=\"login\">zum Login</a>";
            }
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
