using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections.Generic;

namespace freetzbot.commands
{
    class webinterface : command
    {
        private String[] name = { "webinterface" };
        private String helptext = "Information über mein Webinterface";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public void destruct()
        {
            listener_thread.Abort();
            listener.Abort();
        }

        private HttpListener listener;
        private Thread listener_thread;
        private List<pageinterface> pages;

        public webinterface()
        {
            pages = new List<pageinterface>();
            List<Type> typelist = new List<Type>();
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.Namespace == "freetzbot.webpages")
                {
                    pages.Add((pageinterface)Activator.CreateInstance(t));
                }
            }
            listener = new HttpListener();
            listener.Prefixes.Add("http://+:6666/");
            listener.Start();
            listener_thread = new Thread(new ThreadStart(this.httplistener));
            listener_thread.Name = "WebinterfaceThread";
            listener_thread.IsBackground = true;
            listener_thread.Start();
        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            if (listener.IsListening && listener_thread.IsAlive)
            {
                connection.sendmsg("Webinterface läuft und ist unter http://teneon.de:6666 zu erreichen", receiver);
            }
            else
            {
                connection.sendmsg("Scheinbar läuft mein Webinterface nicht mehr :(", receiver);
            }
        }

        private void httplistener()
        {
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                response.Headers.Add(HttpResponseHeader.Server, "FreetzBot");
                String url = request.RawUrl;
                html_response theresponse = new html_response();
                html_request therequest = new html_request();
                therequest.useradress = request.RemoteEndPoint.Address;
                therequest.host = request.Url.Scheme + "://" + request.Url.Host + ":" + request.Url.Port;
                therequest.cookies = request.Cookies;
                if (request.HasEntityBody)
                {
                    StreamReader stream = new StreamReader(request.InputStream, request.ContentEncoding);
                    String streamdata = stream.ReadToEnd();
                    List<String> post = new List<String>();
                    if (streamdata.Contains("&"))
                    {
                        post = new List<String>(streamdata.Split('&'));
                    }
                    else
                    {
                        post.Add(streamdata);
                    }
                    foreach (String data in post)
                    {
                        String[] split = data.Split('=');
                        split[0] = System.Web.HttpUtility.UrlDecode(split[0], request.ContentEncoding);
                        split[1] = System.Web.HttpUtility.UrlDecode(split[1], request.ContentEncoding);
                        therequest.postdata.Add(split[0], split[1]);
                    }
                    stream.Close();
                }
                if (url.Contains("?"))
                {
                    String[] urlsub = url.Split('?');
                    url = urlsub[0];
                    List<String> getdata = new List<String>();
                    if (urlsub[1].Contains("&"))
                    {
                        getdata = new List<String>(urlsub[1].Split('&'));
                    }
                    else
                    {
                        getdata.Add(urlsub[1]);
                    }
                    foreach (String data in getdata)
                    {
                        String[] split = data.Split('=');
                        therequest.getdata.Add(split[0], split[1]);
                    }
                }
                foreach (pageinterface thepage in pages)
                {
                    if (thepage.get_url() == url)
                    {
                        theresponse = thepage.gen_page(therequest);
                    }
                }
                if (theresponse.refer == "")
                {
                    response.Headers.Add(HttpResponseHeader.ContentType, theresponse.content_type);
                    response.StatusCode = theresponse.status_code;
                    if (theresponse.status_code == 404)
                    {
                        theresponse.page = "<!DOCTYPE html><html><body>";
                        theresponse.page += "<center>Die angegebene Seite konnte nicht gefunden werden!</center>";
                        theresponse.page += "</body></html>";
                    }
                    else
                    {
                        response.Cookies = theresponse.cookies;
                    }
                }
                else
                {
                    response.Redirect(theresponse.refer);
                }
                try
                {
                    StreamWriter output = new StreamWriter(response.OutputStream, Encoding.GetEncoding("iso-8859-1"));
                    output.Write(theresponse.page);
                    output.Close();
                }
                catch { }
            }
        }
    }
}