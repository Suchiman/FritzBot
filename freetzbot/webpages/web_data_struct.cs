using System;
using System.Collections.Generic;
using System.Net;

namespace freetzbot
{
    class html_response
    {
        public CookieCollection cookies = new CookieCollection();
        public String page = "";
        public String refer = "";
        public String content_type = "text/html; charset=iso-8859-1";
        public int status_code = 404;
    }
    class html_request
    {
        public Dictionary<String, String> postdata = new Dictionary<String, String>();
        public Dictionary<String, String> getdata = new Dictionary<String, String>();
        public CookieCollection cookies = new CookieCollection();
        public IPAddress useradress = IPAddress.Loopback;
        public String host = "";
    }
}
