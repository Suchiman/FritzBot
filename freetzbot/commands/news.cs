using System;
using System.Collections.Generic;
using System.Threading;

namespace FritzBot.commands
{
    /*
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

        public void Run(ircMessage theMessage)
        {
            if (newsthread.IsAlive)
            {
                theMessage.Answer("Module geladen und am Laufen");
            }
            else
            {
                theMessage.Answer("Es scheint eine Fehlfunktion zu geben, die Funktion ist zum erliegen gekommen");
            }
        }

        private void news_thread()
        {
            String baseurl = "http://webgw.avm.de/download/UpdateNews.jsp";
            String NewsDeOld = null;
            String NewsEnOld = null;
            while (true)
            {
                if (String.IsNullOrEmpty(NewsDeOld) || String.IsNullOrEmpty(NewsEnOld))
                {
                    NewsDeOld = toolbox.GetWeb(baseurl + "?lang=de");
                    NewsEnOld = toolbox.GetWeb(baseurl + "?lang=en");
                }
                else
                {
                    String NewsDe = toolbox.GetWeb(baseurl + "?lang=de");
                    String NewsEn = toolbox.GetWeb(baseurl + "?lang=en");
                    if (NewsDe != NewsDeOld)
                    {
                        toolbox.Announce("Neue Deutsche News: " + toolbox.ShortUrl(baseurl + "?lang=de"));
                        NewsDeOld = NewsDe;
                    }
                    if (NewsEn != NewsEnOld)
                    {
                        toolbox.Announce("Neue Englische News: " + toolbox.ShortUrl(baseurl + "?lang=en"));
                        NewsEnOld = NewsEn;
                    }
                }
                int sleep;
                if (!int.TryParse(Program.configuration["news_check_intervall"], out sleep))
                {
                    sleep = 300000;
                }
                Thread.Sleep(sleep);
            }
        }
    }
    */
}