using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections.Generic;

namespace FritzBot.commands
{
    class webinterface : ICommand
    {
        public String[] Name { get { return new String[] { "webinterface", "web" }; } }
        public String HelpText { get { return "Information über mein Webinterface"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {
            listener_thread.Abort();
            listener.Abort();
        }

        private HttpListener listener;
        private Thread listener_thread;
        private List<IWebInterface> pages;

        public webinterface()
        {
            try
            {
                pages = new List<IWebInterface>();
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t.Namespace == "FritzBot.webpages")
                    {
                        pages.Add((IWebInterface)Activator.CreateInstance(t));
                    }
                }
                listener_thread = new Thread(new ThreadStart(this.httplistener));
                listener_thread.Name = "WebinterfaceThread";
                listener_thread.IsBackground = true;
                listener_thread.Start();
            }
            catch
            {
                toolbox.Logging("Exception in \"public webinterface()\"");
            }
        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (listener.IsListening && listener_thread.IsAlive)
            {
                connection.Sendmsg("Webinterface läuft und ist unter http://teneon.de:8080 zu erreichen", receiver);
            }
            else
            {
                connection.Sendmsg("Scheinbar läuft mein Webinterface nicht mehr :(", receiver);
            }
        }

        private void httplistener()
        {
            int ErrorCount = 0;
            while (true)
            {
                try
                {
                    if (listener != null)
                    {
                        listener.Close();
                    }
                    listener = new HttpListener();
                    listener.Prefixes.Add("http://+:6666/");
                    listener.Prefixes.Add("http://+:8080/");
                    listener.Start();
                    while (true)
                    {
                        HttpListenerContext context = listener.GetContext();
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;
                        response.Headers.Add(HttpResponseHeader.Server, "FritzBot");
                        String url = request.RawUrl;
                        html_response theresponse = new html_response();
                        html_request therequest = new html_request();
                        therequest.useradress = request.RemoteEndPoint.Address;
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
                        foreach (IWebInterface thepage in pages)
                        {
                            if (thepage.Url == url)
                            {
                                theresponse = thepage.GenPage(therequest);
                            }
                        }
                        if (String.IsNullOrEmpty(theresponse.refer))
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
                                List<String> myCookies = theresponse.cookies.GetHeader();
                                foreach (String oneheader in myCookies)
                                {
                                    response.Headers.Add(oneheader);
                                }
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
                catch (Exception ex)
                {
                    toolbox.Logging("Exception im Webinterface Arbeiter Thread " + ex.Message);
                    Thread.Sleep(1000);
                    ErrorCount++;
                    if (ErrorCount > 3)
                    {
                        return;
                    }
                }
            }
        }
    }
}