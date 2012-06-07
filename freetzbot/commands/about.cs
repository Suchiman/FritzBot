using System;

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

        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit 5233 Zeilen. Entwickler: Suchiman");
        }
    }
}