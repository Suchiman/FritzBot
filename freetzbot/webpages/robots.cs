using System;
using FritzBot;

namespace webpages
{
    class robots : IWebInterface
    {
        public String Url { get { return "/robots.txt"; } }

        public HtmlResponse GenPage(HtmlRequest request)
        {
            HtmlResponse response = new HtmlResponse();
            response.page += "User-agent: *\n";
            response.page += "Disallow: /\n";
            response.status_code = 200;
            response.content_type = "text/plain; charset=utf-8";
            return response;
        }
    }
}
