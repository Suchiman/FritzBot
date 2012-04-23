using System;
using FritzBot;

namespace webpages
{
    class robots : IWebInterface
    {
        public String Url { get { return "/robots.txt"; } }

        public html_response GenPage(html_request request)
        {
            html_response response = new html_response();
            response.page += "User-agent: *\n";
            response.page += "Disallow: /\n";
            response.status_code = 200;
            response.content_type = "text/plain; charset=utf-8";
            return response;
        }
    }
}
