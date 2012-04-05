using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;

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

        HttpListener listener;
        Thread listener_thread;

        public webinterface()
        {
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

        private String getresponse(String url, String[] post)
        {
            String output = "";
            /*
            switch (url)
            {
                case "/":
                    output += "<html><body>hallo welt</body></html>";
                    break;
                case "/index":
                    output += "<!DOCTYPE html><html><body>";
                    foreach (String data in post)
                    {
                        output += "<br>" + data + "<br>";
                    }
                    output += "<form action=\"index\" method=\"POST\">";
                    output += "<input type=\"text\" name=\"sometext\"><br>";
                    output += "<input type=\"text\" name=\"othertext\"><br>";
                    output += "<input type=\"submit\">";
                    output += "</form>";
                    output += "</body></html>";
                    break;
                default:
                    output += "<html><body>Hallihall&ouml;chen</body></html>";
                    break;
            }*/
            String[] boxdatabase = toolbox.getDatabaseByName("box.db").GetAll();
            output += "<!DOCTYPE html><html><body><table border=2px>";
            foreach (String data in boxdatabase)
            {
                String[] split = data.Split(':');
                output += "<tr>";
                foreach (String tddata in split)
                {
                    output += "<td>" + tddata + "</td>";
                }
                output += "</tr>";
            }
            output += "</table></body></html>";
            return output;
        }

        private void httplistener()
        {
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                response.Headers.Add(HttpResponseHeader.Server, "FreetzBot");
                String[] post = new String[0];
                if (request.HasEntityBody)
                {
                    StreamReader stream = new StreamReader(request.InputStream, request.ContentEncoding);
                    String streamdata = stream.ReadToEnd();
                    streamdata = System.Web.HttpUtility.UrlDecode(streamdata, request.ContentEncoding);
                    if (streamdata.Contains("&"))
                    {
                        post = streamdata.Split('&');
                    }
                    stream.Close();
                }
                String responseString = getresponse(request.RawUrl, post);
                StreamWriter output = new StreamWriter(response.OutputStream);
                output.Write(responseString);
                output.Close();
            }
        }
    }
}