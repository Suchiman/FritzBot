using System;

namespace FritzBot.Functions
{
    /// <summary>
    /// Stellt eine Cache Klasse da, die den Cache selbstständig erneuert
    /// </summary>
    /// <typeparam name="T">Der Typ der gekapselt werden soll</typeparam>
    public class DataCache<T>
    {
        private T Item;
        private Func<T, T> ValueFactory;
        private TimeSpan Expires;
        public DateTime Renewed { get; protected set; }
        public Exception LastUpdateFail { get; protected set; }
        public bool IsUpToDate
        {
            get
            {
                return Expires == TimeSpan.Zero || Renewed + Expires >= DateTime.Now;
            }
        }
        /// <summary>
        /// Initialisiert eine neue Cache Instanz
        /// </summary>
        /// <param name="valueFactory">Die Methode mit der der gekapselte Typ bei Ablauf erneuert werden kann. Erhält als Parameter die gecachten Daten</param>
        /// <param name="expiresIn">Die Zeit bis der Cache erneuert werden muss oder TimeSpan.Zero wenn er nicht automatisch erneuert werden soll</param>
        public DataCache(Func<T, T> valueFactory, TimeSpan expiresIn)
        {
            ValueFactory = valueFactory;
            Renewed = DateTime.MinValue;
            Expires = expiresIn;
            LastUpdateFail = null;
        }

        /// <summary>
        /// Erneuert den Cache unabhängig davon, ob er bereits abgelaufen ist
        /// </summary>
        public void ForceRefresh(bool ignoreUpdateFail)
        {
            LastUpdateFail = null;
            try
            {
                T tmp = ValueFactory(Item);
                if (!tmp.Equals(Item))
                {
                    Item = tmp;
                    Renewed = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                LastUpdateFail = ex;
                if (!ignoreUpdateFail)
                {
                    throw new RenewalFailedException("Erneuern des Caches fehlgeschlagen", ex);
                }
            }
        }

        /// <summary>
        /// Holt den gekapselten Typen aus dem Cache und erneuert ihn wenn er abgelaufen ist
        /// </summary>
        public T GetItem(bool ignoreUpdateFail)
        {
            if (!IsUpToDate)
            {
                ForceRefresh(ignoreUpdateFail);
            }
            return Item;
        }

        public static implicit operator T(DataCache<T> cache)
        {
            return cache.GetItem(true);
        }
    }

    public class RenewalFailedException : Exception
    {
        public RenewalFailedException() : base() { }
        public RenewalFailedException(string message) : base(message) { }
        public RenewalFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}