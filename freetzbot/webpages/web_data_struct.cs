using System;
using System.Collections.Generic;
using System.Net;

namespace FritzBot
{
    class HtmlResponse
    {
        public OwnCookieCollection cookies = new OwnCookieCollection();
        public String page = "";
        public String refer = "";
        public String content_type = "text/html; charset=iso-8859-1";
        public int status_code = 404;
    }
    class HtmlRequest
    {
        public Dictionary<String, String> postdata = new Dictionary<String, String>();
        public Dictionary<String, String> getdata = new Dictionary<String, String>();
        public CookieCollection cookies = new CookieCollection();
        public IPAddress useradress = IPAddress.Loopback;
    }
    class OwnCookieCollection
    {
        List<OwnCookie> TheCookies = new List<OwnCookie>();
        public String this[String name]
        {
            get
            {
                foreach (OwnCookie onecookie in TheCookies)
                {
                    if (onecookie.name == name)
                    {
                        return onecookie.value;
                    }
                }
                return "";
            }
            set
            {
                foreach (OwnCookie onecookie in TheCookies)
                {
                    if (onecookie.name == name)
                    {
                        onecookie.value = value;
                        return;
                    }
                }
                TheCookies.Add(new OwnCookie(name, value));
            }
        }
        public List<String> GetHeader()
        {
            List<String> header = new List<String>();
            foreach (OwnCookie onecookie in TheCookies)
            {
                header.Add("Set-Cookie: " + onecookie.GetCookie());
            }
            return header;
        }
    }
    class OwnCookie
    {
        public String name;
        public String value;
        public OwnCookie()
        {
            name = "";
            value = "";
        }
        public OwnCookie(String name, String value)
        {
            this.name = name;
            this.value = value;
        }
        public String GetCookie()
        {
            return name + "=" + value;
        }
    }
}
