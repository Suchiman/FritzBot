using System;

namespace FritzBot
{
    public interface ICommand
    {
        /// <summary>
        /// Die Methode die ausgeführt wird wenn der Befehl aufgerufen wird
        /// </summary>
        /// <param name="theMessage">Das Objekt, dass diie Nachricht die den Befehl ausgelöst hat dastellt</param>
        void Run(ircMessage theMessage);
    }

    public interface IBackgroundTask
    {
        /// <summary>
        /// Wird bei der aktivierung des BackgroundTasks aufgerufen
        /// </summary>
        void Start();
        /// <summary>
        /// Wird beim Anhalten oder Beenden aufgerufen. Eventuelle Daten sollten spätestens hier gesichert werden.
        /// </summary>
        void Stop();
    }
}