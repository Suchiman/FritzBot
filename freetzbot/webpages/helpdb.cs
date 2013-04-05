using FritzBot;
using FritzBot.Core;
using FritzBot.Module;
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

            foreach (ICommand theCommand in PluginManager.GetInstance().Get<ICommand>().HasAttribute<ICommand, FritzBot.Module.NameAttribute>().OrderBy(x => x.GetType().Name))
            {
                bool OPNeeded = toolbox.GetAttribute<AuthorizeAttribute>(theCommand) != null;
                if (!OPNeeded || Admin)
                {
                    theresponse.page += "<tr><td>" + String.Join(", ", toolbox.GetAttribute<NameAttribute>(theCommand).Names) + "</td><td>" + toolbox.GetAttribute<HelpAttribute>(theCommand).Help + "</td></tr>";
                }
            }
            theresponse.page += "</table>";
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}