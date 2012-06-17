using System;

namespace FritzBot
{
    public interface ICommand
    {
        /// <summary>
        /// Die Namen auf die die Funktion reagiert
        /// </summary>
        String[] Name { get; }
        /// <summary>
        /// Ein Hilfetext für den Befehl
        /// </summary>
        String HelpText { get; }
        /// <summary>
        /// Bestimmt ob OP Rechte für diesen Befehl erforderlich sind
        /// </summary>
        Boolean OpNeeded { get; }
        /// <summary>
        /// Bestimmt ob Parameter erforderlich sind
        /// </summary>
        Boolean ParameterNeeded { get; }
        /// <summary>
        /// Bestimmt ob die Funktion sowohl mit als auch ohne Parameter funktioniert
        /// </summary>
        Boolean AcceptEveryParam { get; }
        /// <summary>
        /// Die Methode die ausgeführt wird wenn der Befehl aufgerufen wird
        /// </summary>
        /// <param name="theMessage">Das Objekt, dass diie Nachricht die den Befehl ausgelöst hat dastellt</param>
        void Run(ircMessage theMessage);
        /// <summary>
        /// Die Methode die ausgeführt wird, wenn der Befehl entladen oder deaktiviert wird
        /// </summary>
        void Destruct();
    }
}