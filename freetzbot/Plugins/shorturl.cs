using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("shorturl", "urlshort", "urlshortener")]
    [Help("Kürzt den angegebenen Link zu einer tinyurl. Achte darauf, dass die URL ein gültiges http://adresse.tld Format hat. z.b. \"!shorturl http://google.de\"")]
    [ParameterRequired]
    class shorturl : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            theMessage.Answer(Toolbox.ShortUrl(theMessage.CommandLine));
        }
    }
}