using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Name("about")]
    [Help("Ich würde dir dann kurz etwas über mich erzählen.")]
    [ParameterRequired(false)]
    class about : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            theMessage.Answer("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit 6770 Zeilen. Entwickler: Suchiman");
        }
    }
}