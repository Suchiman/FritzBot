using FritzBot;
using FritzBot.Core;
using System;
using System.Linq;

namespace webpages
{
    class helpdb : IWebInterface
    {
        public string Url { get { return "/helpdb"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            string logincheck = login.CheckLogin(request);
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            theresponse.page += "<table border=2px>";
            theresponse.page += "<tr><td><b>Befehl</b></td><td><b>Beschreibung</b></td></tr>";
            bool Admin = false;
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(logincheck);
                if (u != null)
                {
                    Admin = toolbox.IsOp(u);
                }
            }

            foreach (PluginInfo info in PluginManager.GetInstance().Where(x => x.Names.Count > 0).OrderBy(x => x.Names[0]))
            {
                if (!info.AuthenticationRequired || Admin)
                {
                    theresponse.page += "<tr><td>" + String.Join(", ", info.Names) + "</td><td>" + info.HelpText + "</td></tr>";
                }
            }
            theresponse.page += "</table>";
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}