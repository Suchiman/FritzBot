using System;

namespace FritzBot.commands
{
    class boxremove : ICommand
    {
        public String[] Name { get { return new String[] { "boxremove" }; } }
        public String HelpText { get { return "Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\"."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            for (int i = 0; i < theMessage.getUser.boxes.Count; i++)
            {
                if (theMessage.getUser.boxes[i] == theMessage.CommandLine)
                {
                    theMessage.getUser.boxes.RemoveAt(i);
                    theMessage.Answer("Erledigt!");
                    return;
                }
            }
            theMessage.Answer("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht");
        }
    }
}