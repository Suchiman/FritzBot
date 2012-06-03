using System;
using System.Collections.Generic;
using System.Text;

namespace FritzBot
{
    interface IWebInterface
    {
        //Ein Interface für die Websesiten des HTTPListeners implementieren
        //Was wird gebraucht und was wird zurückgegeben?
        //Brauch: POST parameter, Cookies, sonst ?
        //Zurückgeben: Webseite, Neue cookies, statuscode evtl.? (404 ...)
        String Url { get; }
        HtmlResponse GenPage(HtmlRequest request);
    }
}
