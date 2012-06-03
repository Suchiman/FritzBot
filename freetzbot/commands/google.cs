using System;

namespace FritzBot.commands
{
    class google : ICommand
    {
        public String[] Name { get { return new String[] { "google", "g" }; } }
        public String HelpText { get { return "Syntax: (!g) !google etwas das du suchen möchtest"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

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