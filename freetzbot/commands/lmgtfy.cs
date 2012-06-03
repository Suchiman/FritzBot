using System;

namespace FritzBot.commands
{
    class lmgtfy : ICommand
    {
        public String[] Name { get { return new String[] { "lmgtfy" }; } }
        public String HelpText { get { return "Die Funktion benötigt einen Parameter!"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("http://lmgtfy.com/?q=" + toolbox.UrlEncode(theMessage.CommandLine));
        }
    }
}