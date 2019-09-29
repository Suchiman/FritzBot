﻿using FritzBot.Core;
using FritzBot.Database;
using FritzBot.Plugins.SubscriptionProviders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FritzBot.DataModel
{
    public abstract class PluginBase
    {
        public string PluginId { get; }

        protected PluginBase()
        {
            PluginId = GetType().Name;
        }

        protected virtual void NotifySubscribers(string message)
        {
            NotifySubscribers(message, Array.Empty<string>());
        }

        protected virtual void NotifySubscribers(string message, string[] criteria)
        {
            SaveNotification(message);
            using (var context = new BotContext())
            {
                GetSubscribers(context, criteria).ForEach(x => DoNotification(x, message));
            }
        }

        protected virtual void SaveNotification(string message)
        {
            using (var context = new BotContext())
            {
                NotificationHistory h = new NotificationHistory()
                {
                    Created = DateTime.Now,
                    Notification = message,
                    Plugin = PluginId
                };
                context.NotificationHistories.Add(h);
                context.SaveChanges();
            }
        }

        protected virtual void DoNotification(Subscription subscription, string message)
        {
            SubscriptionProvider provider = PluginManager.Plugins.FirstOrDefault(x => x.Id == subscription.Provider).As<SubscriptionProvider>();
            provider?.SendNotification(subscription.User, message);
        }

        protected virtual IQueryable<Subscription> GetSubscribers(BotContext context, string[] criteria)
        {
            return context.Subscriptions.Include(x => x.User).Where(x => x.Plugin == PluginId);
        }
    }
}