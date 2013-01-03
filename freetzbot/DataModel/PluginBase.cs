using FritzBot.Core;
using FritzBot.Plugins.SubscriptionProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.DataModel
{
    public abstract class PluginBase
    {
        public String PluginID { get; protected set; }

        public PluginBase()
        {
            PluginID = this.GetType().Name;
        }

        protected virtual void NotifySubscribers(String message)
        {
            NotifySubscribers(message, new String[0]);
        }

        protected virtual void NotifySubscribers(String message, String[] criteria)
        {
            SaveNotification(message);
            GetSubscribers(criteria).ForEach(x => DoNotification(x, message));
        }

        protected virtual void SaveNotification(String message)
        {
            ModulDataStorage mds = XMLStorageEngine.GetManager().GetGlobalSettingsStorage("NotificationHistory");
            mds.Storage.Add(new XElement("Notification", message, new XAttribute("Created", DateTime.Now), new XAttribute("Plugin", PluginID)));
        }

        protected virtual void DoNotification(User user, String message)
        {
            IEnumerable<XElement> subscription = user.GetSubscriptions().Where(x => x.Value == PluginID);
            IEnumerable<SubscriptionProvider> providers = PluginManager.GetInstance().Get<SubscriptionProvider>();
            foreach (XElement sub in subscription)
            {
                SubscriptionProvider provider = providers.FirstOrDefault(x => x.PluginID == sub.Attribute("Provider").Value);
                if (provider != null)
                {
                    provider.SendNotification(user, message);
                }
            }
        }

        protected virtual IEnumerable<User> GetSubscribers(String[] criteria)
        {
            return UserManager.GetInstance().Where(x => x.GetSubscriptions().Any(y => y.Value == PluginID));
        }

        protected virtual ModulDataStorage PluginStorage
        {
            get
            {
                return XMLStorageEngine.GetManager().GetGlobalSettingsStorage(PluginID);
            }
        }
    }
}
