using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("about")]
    [Module.Help("Ich würde dir dann kurz etwas über mich erzählen.")]
    [Module.ParameterRequired(false)]
    class about : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit ~6000 Zeilen. Entwickler: Suchiman");
        }
    }
}