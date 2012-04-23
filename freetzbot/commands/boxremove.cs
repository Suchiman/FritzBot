using System;
using FritzBot;

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

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            for (int i = 0; i < Program.TheUsers[sender].boxes.Count; i++)
            {
                if (Program.TheUsers[sender].boxes[i] == message)
                {
                    Program.TheUsers[sender].boxes.RemoveAt(i);
                    connection.Sendmsg("Erledigt!", receiver);
                    return;
                }
            }
            connection.Sendmsg("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht", receiver);
        }
    }
}