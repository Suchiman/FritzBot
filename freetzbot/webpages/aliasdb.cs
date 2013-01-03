using FritzBot;
using FritzBot.Plugins;
using FritzBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace webpages
{
    class aliasdb : IWebInterface
    {
        public String Url { get { return "/aliasdb"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse theresponse = new HtmlResponse();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.GenMenu(request);
            theresponse.page += "<table border=2px>";
            theresponse.page += "<tr><td><b>Alias</b></td><td><b>Beschreibung</b></td></tr>";
            IEnumerable<XElement> thealiases = UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage("alias").Storage.Elements("alias"));
            foreach (XElement alias in thealiases.Where(x => x.HasElements))
            {
                theresponse.page += "<tr><td>" + alias.Element("name").Value + "</td><td>" + alias.Element("beschreibung").Value + "</td></tr>";
            }
            theresponse.page += "</table>";
            theresponse.page += "</body></html>";
            theresponse.status_code = 200;
            return theresponse;
        }
    }
}
