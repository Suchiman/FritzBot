using System;

namespace FritzBot.commands
{
    [Module.Name("boxremove")]
    [Module.Help("Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".")]
    [Module.ParameterRequired]
    class boxremove : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            for (int i = 0; i < theMessage.TheUser.boxes.Count; i++)
            {
                if (theMessage.TheUser.boxes[i] == theMessage.CommandLine)
                {
                    theMessage.TheUser.boxes.RemoveAt(i);
                    theMessage.Answer("Erledigt!");
                    return;
                }
            }
            theMessage.Answer("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht");
        }
    }
}