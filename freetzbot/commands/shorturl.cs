using System;
using System.Net;

namespace FritzBot.commands
{
    class shorturl : ICommand
    {
        public String[] Name { get { return new String[] { "shorturl", "urlshort", "urlshortener" }; } }
        public String HelpText { get { return "Kürzt den angegebenen Link zu einer tinyurl. Achte darauf, dass die URL ein gültiges http://adresse.tld Format hat. z.b. \"!shorturl http://google.de\""; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            connection.Sendmsg(toolbox.ShortUrl(message), receiver);
        }
    }
}
