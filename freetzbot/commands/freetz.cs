using System;

namespace FritzBot.commands
{
    [Module.Name("freetz", "f")]
    [Module.Help("Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz Build System")]
    class freetz : ICommand
    {
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