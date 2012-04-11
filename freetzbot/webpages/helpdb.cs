using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.webpages
{
    class helpdb : pageinterface
    {
        public string get_url()
        {
            return "/helpdb";
        }

        public html_response gen_page(html_request request)
        {
            html_response theresponse = new html_response();
            theresponse.page += "<!DOCTYPE html><html><body>";
            theresponse.page += index.gen_menu();
            String logincheck = login.check_login(request);
            if (logincheck != "")
            {
                theresponse.page += "<table border=2px>";
                theresponse.page += "<tr><td><b>Befehl</b></td><td><b>Beschreibung</b></td></tr>";
                foreach (command thecommand in freetzbot.Program.commands)
                {
                    if (!(thecommand.get_op_needed() && !toolbox.op_check(logincheck)))
                    {
                        String names = "";
                        foreach (String name in thecommand.get_name())
                        {
                            names += ", " + name;
                        }
                        names = names.Remove(0, 2);
                        theresponse.page += "<tr><td>" + names + "</td><td>" + thecommand.get_helptext() + "</td></tr>";
                    }
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
