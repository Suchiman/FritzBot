using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("push")]
    [Module.Help("Pushover")]
    [Module.ParameterRequired(false)]
    class pushover : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Dictionary<String, String> Parameter = new Dictionary<String, String>()
            {
                {"token","b6p6augH1KDpxcRxyo4I35Yxl9XP5x"},
                {"user","NHacX7yLbC74ZItmAEIxtGNVqbd11X"},
                {"message","Testnachricht"}
            };
            String antwort = toolbox.GetWeb("https://api.pushover.net/1/messages.json", Parameter);
            theMessage.Answer("habe fertig");
            theMessage.Answer(antwort);
        }
    }
}