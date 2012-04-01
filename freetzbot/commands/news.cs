using System;
using System.Collections.Generic;
using System.Threading;

namespace freetzbot.commands
{
    class news : command
    {
        private String[] name = { "news" };
        private String helptext = "Eine meiner hintergrund Funktionen. Sie checkt die AVM Firmware News Webseite und gibt beim Fund neuer Nachrichten eine entsprechende Meldung aus";
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

        public void run(irc connection, String sender, String receiver, String message)
        {
            if (newsthread.IsAlive)
            {
                connection.sendmsg("Module geladen und am Laufen", receiver);
            }
            else
            {
                connection.sendmsg("Es scheint eine Fehlfunktion zu geben, die Funktion ist zum erliegen gekommen", receiver);
            }
        }

        public news()
        {
            newsthread = new Thread(new ThreadStart(news_thread));
            newsthread.IsBackground = true;
            newsthread.Start();
        }

        Thread newsthread = new Thread(new ThreadStart(news_thread));

        private static void news_thread()
        {
            String baseurl = "http://webgw.avm.de/download/UpdateNews.jsp";
            String[] news_de_old = news_parse(baseurl + "?lang=de");
            String[] news_en_old = news_parse(baseurl + "?lang=en");
            while (true)
            {
                String[] news_de = news_parse(baseurl + "?lang=de");
                String[] news_en = news_parse(baseurl + "?lang=en");
                if (news_de_old[0] != news_de[0])
                {
                    List<String> differs = new List<String>();
                    for (int i = 0; i < news_de.Length; i++)
                    {
                        if (news_de_old[0] != news_de[i])
                        {
                            differs.Add(news_de[i]);
                        }
                    }
                    String output = "Neue Deutsche News gesichtet! ";
                    foreach (String thenews in differs)
                    {
                        output += ", " + thenews;
                    }
                    toolbox.announce(output);
                    news_de_old = news_de;
                }
                if (news_en_old[0] != news_en[0])
                {
                    List<String> differs = new List<String>();
                    for (int i = 0; i < news_de.Length; i++)
                    {
                        if (news_de_old[0] != news_de[i])
                        {
                            differs.Add(news_de[i]);
                        }
                    }
                    String output = "Neue englische News gesichtet! ";
                    foreach (String thenews in differs)
                    {
                        output += ", " + thenews;
                    }
                    toolbox.announce(output);
                    news_en_old = news_en;
                }
                int sleep;
                if (!int.TryParse(freetzbot.Program.configuration.get("news_check_intervall"), out sleep))
                {
                    sleep = 300000;
                }
                Thread.Sleep(sleep);
            }
        }

        private static String[] news_parse(String url)
        {
            String news = toolbox.get_web(url);
            List<String> newstopic = new List<String>();
            List<String> subnews = new List<String>();
            List<String> uberschriftblau = new List<String>(news.Split(new String[] { "<span class=\"uberschriftblau\">" }, 21, StringSplitOptions.None));
            uberschriftblau.RemoveAt(0);
            foreach (String uberschrift in uberschriftblau)
            {
                String text = uberschrift;
                int nbsp = uberschrift.IndexOf("&nbsp;");
                if (nbsp != -1)
                {
                    text = text.Remove(nbsp);
                }
                newstopic.Add(text.Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0]);
            }
            List<String> fliesstextblau = new List<String>(news.Split(new String[] { "<span class=\"fliesstextblau\">" }, 21, StringSplitOptions.None));
            fliesstextblau.RemoveAt(0);
            foreach (String fliesstext in fliesstextblau)
            {
                String text = fliesstext.Replace("&nbsp;", " ");
                subnews.Add(text.Split(new String[] { "</span>" }, 2, StringSplitOptions.None)[0]);
            }
            List<String> news_new = new List<String>();
            for (int i = 0; i < uberschriftblau.Count; i++)
            {
                news_new.Add(newstopic[i] + ":" + subnews[i]);
            }
            return news_new.ToArray();
        }
    }
}