using System;

namespace FritzBot.commands
{
    [Module.Name("google", "g")]
    [Module.Help("Syntax: (!g) !google etwas das du suchen möchtest")]
    class google : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            String output = "https://www.google.de/search?q=";
            if (String.IsNullOrEmpty(theMessage.CommandLine))
            {
                output = "http://www.google.de/";
            }
            else
            {
                output += toolbox.UrlEncode("\"" + theMessage.CommandLine + "\"");
            }
            theMessage.Answer(output);
        }
    }
}