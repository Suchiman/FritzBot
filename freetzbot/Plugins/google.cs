using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("google", "g", "gg")]
    [Module.Help("Syntax: (!g) !google etwas das du suchen möchtest")]
    class google : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
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
                    output += toolbox.UrlEncode("\"" + theMessage.CommandLine + "\"");
                }
                else
                {
                    output += toolbox.UrlEncode(theMessage.CommandLine);
                }
            }
            theMessage.Answer(output);
        }
    }
}