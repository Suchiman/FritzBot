using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace FritzBot.Plugins.SubscriptionProviders
{
    public abstract class SubscriptionProvider : PluginBase
    {
        public abstract void SendNotification(User user, string message);

        public virtual void AddSubscription(IrcMessage theMessage, PluginBase plugin)
        {
            Contract.Requires(theMessage != null && plugin != null);

            using (var context = new BotContext())
            {
                User u = context.GetUser(theMessage.Nickname);
                Subscription SpecificSubscription = context.Subscriptions.Include(x => x.Bedingungen).FirstOrDefault(x => x.User.Id == u.Id && x.Provider == PluginID && x.Plugin == plugin.PluginID);
                if (SpecificSubscription == null)
                {
                    SpecificSubscription = new Subscription()
                    {
                        Plugin = plugin.PluginID,
                        Provider = PluginID,
                        User = u,
                        Bedingungen = new List<SubscriptionBedingung>()
                    };

                    if (theMessage.CommandArgs.Count > 3 && !String.IsNullOrEmpty(theMessage.CommandArgs[3]))
                    {
                        SpecificSubscription.Bedingungen.Add(new SubscriptionBedingung { Bedingung = theMessage.CommandArgs[3] });
                    }

                    context.Subscriptions.Add(SpecificSubscription);
                    theMessage.Answer($"Du wirst absofort mit {GetType().GetCustomAttribute<NameAttribute>().Names[0]} für {plugin.GetType().GetCustomAttribute<NameAttribute>().Names[0]} benachrichtigt");
                }
                else if (theMessage.CommandArgs.Count > 3 && !String.IsNullOrEmpty(theMessage.CommandArgs[3]) && SpecificSubscription.Bedingungen.Count == 0)
                {
                    SpecificSubscription.Bedingungen.Add(new SubscriptionBedingung { Bedingung = theMessage.CommandArgs[3] });
                    SpecificSubscription.Bedingungen = SpecificSubscription.Bedingungen.Distinct(x => x.Bedingung).OrderBy(x => x.Bedingung).ToList();
                    theMessage.Answer("Bedingung für Subscription hinzugefügt");
                }
                else if (SpecificSubscription.Bedingungen.Count > 0)
                {
                    SpecificSubscription.Bedingungen.Clear();
                    theMessage.Answer("Bedingungen entfernt");
                }
                else
                {
                    theMessage.Answer("Du bist bereits für dieses Plugin eingetragen");
                }
                context.SaveChanges();
            }
        }

        public virtual void ParseSubscriptionSetup(IrcMessage theMessage)
        {
            Contract.Requires(theMessage != null);

            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Zu wenig Parameter, probier mal: !subscribe setup <SubscriptionProvider> <Einstellung>");
                return;
            }
            using (var context = new BotContext())
            {
                UserKeyValueEntry entry = context.GetStorageOrCreate(theMessage.Nickname, PluginID);
                entry.Value = theMessage.CommandArgs[2];
                context.SaveChanges();
            }
            theMessage.Answer("Einstellungen erfolgreich gespeichert");
        }
    }
}