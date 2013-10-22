using System;
using System.Diagnostics.Contracts;

namespace FritzBot
{
    [ContractClass(typeof(CommandContract))]
    public interface ICommand
    {
        /// <summary>
        /// Die Methode die ausgeführt wird wenn der Befehl aufgerufen wird
        /// </summary>
        /// <param name="theMessage">Das Objekt, dass diie Nachricht die den Befehl ausgelöst hat dastellt</param>
        void Run(ircMessage theMessage);
    }

    [ContractClass(typeof(BackgroundTaskContract))]
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

    [ContractClassFor(typeof(ICommand))]
    public abstract class CommandContract : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            Contract.Requires<ArgumentNullException>(theMessage != null, "theMessage darf nicht null sein");
        }
    }

    [ContractClassFor(typeof(IBackgroundTask))]
    public abstract class BackgroundTaskContract : IBackgroundTask
    {
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}