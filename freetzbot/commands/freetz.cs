using System;

namespace FritzBot.commands
{
    class freetz : ICommand
    {
        public String[] Name { get { return new String[] { "freetz", "f" }; } }
        public String HelpText { get { return "Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz Build System"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            String output = "http://freetz.org/search?q=";
            if (!theMessage.HasArgs)
            {
                output = "http://freetz.org/wiki";
            }
            else
            {
                output += toolbox.UrlEncode(theMessage.CommandLine) + "&wiki=on";
            }
            output = output.Replace("%23", "#");
            theMessage.Answer(output);
        }
    }
}