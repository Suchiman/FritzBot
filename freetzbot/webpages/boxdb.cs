using FritzBot;
using FritzBot.Core;
using System;
using System.Linq;

namespace webpages
{
    class boxdb : IWebInterface
    {
        public string Url { get { return "/boxdb"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            string LoginCheck = login.CheckLogin(request);
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            if (!String.IsNullOrEmpty(LoginCheck))
            {
                theresponse.page += "<table border=2px>";
                theresponse.page += "<tr><td><b>Besitzer</b></td><td><b>Boxen</b></td></tr>";
                foreach (User theuser in UserManager.GetInstance().Where(x => x.GetModulUserStorage("box").Storage.Elements("box").Count() > 0))
                {
                    theresponse.page += "<tr><td>" + String.Join(", ", theuser.names.ToArray<string>()) + "</td><td>" + String.Join(", ", theuser.GetModulUserStorage("box").Storage.Elements("box").Select(x => x.Value).ToArray<string>()) + "</td></tr>";
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
