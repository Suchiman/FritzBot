using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FritzBot.Plugins
{
    [Module.Name("webinterface", "web")]
    [Module.Help("Information über mein Webinterface")]
    [Module.ParameterRequired(false)]
    class webinterface : PluginBase, ICommand, IBackgroundTask
    {
        private HttpListener Listener;
        private Thread ListenerThread;
        private List<IWebInterface> pages;
        public const String Address = "http://teneon.de:8080/";

        public void Stop()
        {
            ListenerThread.Abort();
            Listener.Abort();
        }

        public void Start()
        {
            try
            {
                pages = new List<IWebInterface>();
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t.Name != "IWebInterface" && (typeof(IWebInterface)).IsAssignableFrom(t))
                    {
                        pages.Add((IWebInterface)Activator.CreateInstance(t));
                    }
                }
                ListenerThread = new Thread(new ThreadStart(this.httplistener));
                ListenerThread.Name = "WebinterfaceThread";
                ListenerThread.IsBackground = true;
                ListenerThread.Start();
            }
            catch
            {
                toolbox.Logging("Exception in \"public webinterface()\"");
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (Listener.IsListening && ListenerThread.IsAlive)
            {
                theMessage.Answer("Webinterface läuft und ist unter " + Address + " zu erreichen");
            }
            else
            {
                theMessage.Answer("Scheinbar läuft mein Webinterface nicht mehr :(");
            }
        }

        private void httplistener()
        {
            int ErrorCount = 0;
            while (true)
            {
                try
                {
                    if (Listener != null)
                    {
                        Listener.Close();
                    }
                    Listener = new HttpListener();
                    Listener.Prefixes.Add("http://+:8080/");
                    Listener.Start();
                    while (true)
                    {
                        HttpListenerContext Context = Listener.GetContext();
                        HttpListenerRequest Request = Context.Request;
                        HttpListenerResponse Response = Context.Response;
                        Response.Headers.Add(HttpResponseHeader.Server, "FritzBot");
                        String url = Request.RawUrl;
                        HtmlResponse TheResponse = new HtmlResponse();
                        HtmlRequest TheRequest = new HtmlRequest();
                        TheRequest.useradress = Request.RemoteEndPoint.Address;
                        TheRequest.cookies = Request.Cookies;
                        if (Request.HasEntityBody)
                        {
                            StreamReader stream = new StreamReader(Request.InputStream, Request.ContentEncoding);
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
                                String[] ToAdd = new String[2];
                                if (split.Length > 0)
                                {
                                    ToAdd[0] = System.Web.HttpUtility.UrlDecode(split[0], Request.ContentEncoding);
                                }
                                else
                                {
                                    ToAdd[0] = "";
                                }
                                if (split.Length > 1)
                                {
                                    ToAdd[1] = System.Web.HttpUtility.UrlDecode(split[1], Request.ContentEncoding);
                                }
                                else
                                {
                                    ToAdd[1] = "";
                                }
                                TheRequest.postdata.Add(ToAdd[0], ToAdd[1]);
                            }
                            stream.Close();
                        }
                        if (url.Contains("?"))
                        {
                            String[] UrlSub = url.Split('?');
                            url = UrlSub[0];
                            List<String> GetData = new List<String>();
                            if (UrlSub[1].Contains("&"))
                            {
                                GetData = new List<String>(UrlSub[1].Split('&'));
                            }
                            else
                            {
                                GetData.Add(UrlSub[1]);
                            }
                            foreach (String data in GetData)
                            {
                                String[] split = data.Split('=');
                                String[] ToAdd = new String[2];
                                if (split.Length > 0)
                                {
                                    ToAdd[0] = System.Web.HttpUtility.UrlDecode(split[0], Request.ContentEncoding);
                                }
                                else
                                {
                                    ToAdd[0] = "";
                                }
                                if (split.Length > 1)
                                {
                                    ToAdd[1] = System.Web.HttpUtility.UrlDecode(split[1], Request.ContentEncoding);
                                }
                                else
                                {
                                    ToAdd[1] = "";
                                }
                                TheRequest.getdata.Add(ToAdd[0], ToAdd[1]);
                            }
                        }
                        foreach (IWebInterface thepage in pages)
                        {
                            if (thepage.Url == url)
                            {
                                TheResponse = thepage.GenPage(TheRequest);
                            }
                        }
                        if (String.IsNullOrEmpty(TheResponse.refer))
                        {
                            Response.Headers.Add(HttpResponseHeader.ContentType, TheResponse.content_type);
                            Response.StatusCode = TheResponse.status_code;
                            if (TheResponse.status_code == 404)
                            {
                                TheResponse.page = "<!DOCTYPE html><html><body>";
                                TheResponse.page += "<center>Die angegebene Seite konnte nicht gefunden werden!</center>";
                                TheResponse.page += "</body></html>";
                            }
                            else
                            {
                                List<String> myCookies = TheResponse.cookies.GetHeader();
                                foreach (String oneheader in myCookies)
                                {
                                    Response.Headers.Add(oneheader);
                                }
                            }
                        }
                        else
                        {
                            Response.Redirect(TheResponse.refer);
                        }
                        try
                        {
                            StreamWriter output = new StreamWriter(Response.OutputStream, Encoding.GetEncoding("iso-8859-1"));
                            output.Write(TheResponse.page);
                            output.Close();
                        }
                        catch { }
                    }
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 5)
                    {
                        toolbox.Logging("Besitze nicht die Notwendigen Rechte um das Webinterface zu starten");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    toolbox.Logging("Exception im Webinterface Arbeiter Thread " + ex.Message + "\r\n" + ex.StackTrace);
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