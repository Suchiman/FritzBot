using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FritzBot.Core;
using System.Diagnostics.Contracts;

namespace FritzBot.Plugins.SubscriptionProviders
{
    public abstract class SubscriptionProvider : PluginBase
    {
        public abstract void SendNotification(User user, string message);

        public virtual void AddSubscription(ircMessage theMessage, PluginBase plugin)
        {
            Contract.Requires(theMessage != null && plugin != null);

            using (DBProvider db = new DBProvider())
            {
                Subscription SpecificSubscription = db.QueryLinkedData<Subscription, User>(theMessage.TheUser).FirstOrDefault(x => x.Provider == PluginID && x.Plugin == plugin.PluginID);

                if (SpecificSubscription == null)
                {
                    SpecificSubscription = new Subscription()
                    {
                        Plugin = plugin.PluginID,
                        Provider = PluginID,
                        Reference = theMessage.TheUser
                    };
                    if (theMessage.CommandArgs.Count > 3 && !String.IsNullOrEmpty(theMessage.CommandArgs[3]))
                    {
                        SpecificSubscription.Bedingungen.Add(theMessage.CommandArgs[3]);
                    }
                    theMessage.Answer(String.Format("Du wirst absofort mit {0} für {1} benachrichtigt", toolbox.GetAttribute<Module.NameAttribute>(this).Names[0], toolbox.GetAttribute<Module.NameAttribute>(plugin).Names[0]));
                }
                else
                {
                    if (theMessage.CommandArgs.Count > 3 && !String.IsNullOrEmpty(theMessage.CommandArgs[3]) && SpecificSubscription.Bedingungen.Count == 0)
                    {
                        SpecificSubscription.Bedingungen.Add(theMessage.CommandArgs[3]);
                        SpecificSubscription.Bedingungen = SpecificSubscription.Bedingungen.Distinct().OrderBy(x => x).ToList();
                        theMessage.Answer("Bedingung für Subscription hinzugefügt");
                    }
                    else if (SpecificSubscription.Bedingungen.Count > 0)
                    {
                        SpecificSubscription.Bedingungen.Clear();
                        db.SaveOrUpdate(SpecificSubscription);
                        theMessage.Answer("Bedingungen entfernt");
                    }
                    else
                    {
                        theMessage.Answer("Du bist bereits für dieses Plugin eingetragen");
                    }
                }
                db.SaveOrUpdate(SpecificSubscription);
            }
        }

        public virtual void ParseSubscriptionSetup(ircMessage theMessage)
        {
            Contract.Requires(theMessage != null);

            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Zu wenig Parameter, probier mal: !subscribe setup <SubscriptionProvider> <Einstellung>");
                return;
            }
            using (DBProvider db = new DBProvider())
            {
                SimpleStorage storage = GetSettings(db, theMessage.TheUser);
                storage.Store(PluginID, theMessage.CommandArgs[2]);
                db.SaveOrUpdate(storage);
            }
            theMessage.Answer("Einstellungen erfolgreich gespeichert");
        }

        public virtual SimpleStorage GetSettings(DBProvider db, User user)
        {
            Contract.Requires(db != null && user != null);
            Contract.Ensures(Contract.Result<SimpleStorage>() != null);

            return db.GetSimpleStorage(user, "SubscriptionSettings");
        }
    }
}