using System;
using System.Collections.Generic;
using System.Net;

namespace FritzBot
{
    public class HtmlResponse
    {
        public OwnCookieCollection cookies = new OwnCookieCollection();
        public string page = "";
        public string refer = "";
        public string content_type = "text/html; charset=iso-8859-1";
        public int status_code = 404;
    }
    public class HtmlRequest
    {
        public Dictionary<String, String> postdata = new Dictionary<String, String>();
        public Dictionary<String, String> getdata = new Dictionary<String, String>();
        public CookieCollection cookies = new CookieCollection();
        public IPAddress useradress = IPAddress.Loopback;
    }
    public class OwnCookieCollection
    {
        List<OwnCookie> TheCookies = new List<OwnCookie>();
        public string this[string name]
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
        public List<string> GetHeader()
        {
            List<string> header = new List<string>();
            foreach (OwnCookie onecookie in TheCookies)
            {
                header.Add("Set-Cookie: " + onecookie.GetCookie());
            }
            return header;
        }
    }
    class OwnCookie
    {
        public string name;
        public string value;
        public OwnCookie()
        {
            name = "";
            value = "";
        }
        public OwnCookie(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        public string GetCookie()
        {
            return name + "=" + value;
        }
    }
}
