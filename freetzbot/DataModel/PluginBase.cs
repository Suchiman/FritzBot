using FritzBot.Core;
using FritzBot.Plugins.SubscriptionProviders;
using System;
using System.Linq;

namespace FritzBot.DataModel
{
    public abstract class PluginBase
    {
        public string PluginID { get; protected set; }

        public PluginBase()
        {
            PluginID = this.GetType().Name;
        }

        protected virtual void NotifySubscribers(string message)
        {
            NotifySubscribers(message, new string[0]);
        }

        protected virtual void NotifySubscribers(string message, string[] criteria)
        {
            SaveNotification(message);
            GetSubscribers(criteria).ForEach(x => DoNotification(x, message));
        }

        protected virtual void SaveNotification(string message)
        {
            using (DBProvider db = new DBProvider())
            {
                NotificationHistory h = new NotificationHistory()
                {
                    Created = DateTime.Now,
                    Notification = message,
                    Plugin = PluginID
                };
                db.SaveOrUpdate(h);
            }
        }

        protected virtual void DoNotification(Subscription subscription, string message)
        {
            SubscriptionProvider provider = PluginManager.GetInstance().Get<SubscriptionProvider>(x => x.PluginID == subscription.Provider);
            if (provider != null)
            {
                provider.SendNotification(subscription.Reference, message);
            }
        }

        protected virtual IQueryable<Subscription> GetSubscribers(string[] criteria)
        {
            using (DBProvider db = new DBProvider())
            {
                return db.Query<Subscription>(x => x.Plugin == PluginID);
            }
        }

        protected virtual SimpleStorage GetPluginStorage(DBProvider db)
        {
            return db.GetSimpleStorage(PluginID);
        }
    }
}