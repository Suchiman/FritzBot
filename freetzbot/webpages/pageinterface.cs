using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot
{
    interface pageinterface
    {
        //Ein Interface für die Websesiten des HTTPListeners implementieren
        //Was wird gebraucht und was wird zurückgegeben?
        //Brauch: POST parameter, Cookies, sonst ?
        //Zurückgeben: Webseite, Neue cookies, statuscode evtl.? (404 ...)
        String get_url();
        html_response gen_page(html_request request);
    }
}
