using System;
using FritzBot;

namespace FritzBot.commands
{
    class about : ICommand
    {
        public String[] Name { get { return new String[] { "about" }; } }
        public String HelpText { get { return "Ich würde dir dann kurz etwas über mich erzählen."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            connection.Sendmsg("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit 4509 Zeilen. Entwickler: Suchiman", receiver);
        }
    }
}