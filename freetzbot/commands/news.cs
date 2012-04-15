using System;
using System.Collections.Generic;
using System.Threading;

namespace FritzBot.commands
{
    class news : ICommand
    {
        public String[] Name { get { return new String[] { "news" }; } }
        public String HelpText { get { return "Eine meiner hintergrund Funktionen. Sie checkt die AVM Firmware News Webseite und gibt beim Fund neuer Nachrichten eine entsprechende Meldung aus"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public news()
        {
            newsthread = new Thread(new ThreadStart(this.news_thread));
            newsthread.Name = "NewsThread";
            newsthread.IsBackground = true;
            newsthread.Start();
        }

        public void Destruct()
        {
            newsthread.Abort();
        }

        Thread newsthread;

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (newsthread.IsAlive)
            {
                connection.Sendmsg("Module geladen und am Laufen", receiver);
            }
            else
            {
                connection.Sendmsg("Es scheint eine Fehlfunktion zu geben, die Funktion ist zum erliegen gekommen", receiver);
            }
        }

        private void news_thread()
        {
            String baseurl = "http://webgw.avm.de/download/UpdateNews.jsp";
            Boolean failed = false;
            String[] news_de_old = new String[0];
            String[] news_en_old = new String[0];
            do
            {
                try
                {
                    news_de_old = news_parse(baseurl + "?lang=de");
                    news_en_old = news_parse(baseurl + "?lang=en");
                    failed = false;
                }
                catch
                {
                    failed = true;
                }
            } while (failed);
            while (true)
            {
                try
                {
                    String[] news_de = null;
                    String[] news_en = null;
                    do
                    {
                        try
                        {
                            news_de = news_parse(baseurl + "?lang=de");
                            news_en = news_parse(baseurl + "?lang=en");
                            failed = false;
                        }
                        catch
                        {
                            failed = true;
                            Thread.Sleep(500);
                        }
                    } while (failed);
                    if (news_de_old[0] != news_de[0])
                    {
                        List<String> differs = new List<String>();
                        for (int i = 0; i < news_de.Length; i++)
                        {
                            if (news_de_old[0] == news_de[i])
                            {
                                break;
                            }
                            differs.Add(news_de[i]);
                        }
                        String output = "Neue Deutsche News gesichtet! ";
                        foreach (String thenews in differs)
                        {
                            output += ", " + thenews;
                        }
                        output = output.Remove(30, 1);
                        output = output.Insert(30, "-");
                        toolbox.Announce(output);
                        news_de_old = news_de;
                    }
                    if (news_en_old[0] != news_en[0])
                    {
                        List<String> differs = new List<String>();
                        for (int i = 0; i < news_en.Length; i++)
                        {
                            if (news_en_old[0] == news_en[i])
                            {
                                break;
                            }
                            differs.Add(news_en[i]);
                        }
                        String output = "Neue englische News gesichtet! ";
                        foreach (String thenews in differs)
                        {
                            output += ", " + thenews;
                        }
                        output = output.Remove(30, 1);
                        output = output.Insert(30, "-");
                        toolbox.Announce(output);
                        news_en_old = news_en;
                    }
                }
                catch
                {

                }
                int sleep;
                if (!int.TryParse(FritzBot.Program.configuration["news_check_intervall"], out sleep))
                {
                    sleep = 300000;
                }
                Thread.Sleep(sleep);
            }
        }

        private static String[] news_parse(String url)
        {
            String news = toolbox.GetWeb(url);
            if (String.IsNullOrEmpty(news))
            {
                throw new InvalidOperationException("Konnte die Webseite nicht lesen");
            }
            String[] newssplit = news.Split(new String[] { "<span class=\"uberschriftblau\">" }, 21, StringSplitOptions.None);
            List<String> news_new = new List<String>();
            for (int i = 1; i < newssplit.Length; i++)
            {
                String[] splitted = newssplit[i].Split(new String[] { "</span>" }, 3, StringSplitOptions.None);
                int nbsp = splitted[0].IndexOf("&nbsp;");
                if(nbsp != -1)
                {
                    splitted[0] = splitted[0].Remove(nbsp);
                }
                splitted[1] = splitted[1].Remove(0, 35);
                splitted[1] = splitted[1].Replace("&nbsp;", " ");
                splitted[2] = splitted[2].Split(new String[] { "\"><u>Weitere" }, 2, StringSplitOptions.None)[0].Split(new String[] { "href=\"" }, 2, StringSplitOptions.None)[1];
                news_new.Add(splitted[0] + ":" + splitted[1] + " - " + toolbox.ShortUrl(splitted[2]));
            }
            if (news_new.Count < 20 || String.IsNullOrEmpty(news_new[0]))
            {
                throw new InvalidOperationException("Verarbeitungsfehler");
            }
            return news_new.ToArray();
        }
    }
}