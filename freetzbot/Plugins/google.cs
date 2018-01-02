using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("google", "g", "gg")]
    [Help("Syntax: (!g) !google etwas das du suchen möchtest")]
    class google : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            string output = "https://www.google.de/search?q=";
            if (String.IsNullOrEmpty(theMessage.CommandLine))
            {
                output = "http://www.google.de/";
            }
            else
            {
                if (theMessage.CommandName == "gg")
                {
                    output += Toolbox.UrlEncode("\"" + theMessage.CommandLine + "\"");
                }
                else
                {
                    output += Toolbox.UrlEncode(theMessage.CommandLine);
                }
            }
            theMessage.Answer(output);
        }
    }
}