using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using FritzBot.Plugins.SubscriptionProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FritzBot.Plugins
{
    [Name("subscribe")]
    [Help("Unterbefehle: add <PluginName> <SubscriptionProvider> <Bedingung>(optional), available, list, setup <SubscriptionProvider> <Adresse>, remove <PluginName> <SubscriptionProvider>, help <SubscriptionProvider>")]
    class subscribe : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            switch (theMessage.CommandArgs.FirstOrDefault())
            {
                case "add":
                    SubscriptionAdd(theMessage);
                    return;
                case "available":
                    SubscriptionsAvailable(theMessage);
                    return;
                case "list":
                    SubscriptionsList(theMessage);
                    return;
                case "setup":
                    SetupSubscription(theMessage);
                    return;
                case "remove":
                    RemoveSubscription(theMessage);
                    return;
                case "help":
                    HelpSubscription(theMessage);
                    return;
                default:
                    theMessage.Answer("Das kommt mir nicht bekannt vor...");
                    theMessage.AnswerHelp(this);
                    return;
            }
        }

        private void HelpSubscription(IrcMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.Answer("Die Funktion benötigt 2 Parameter: !subscribe help <SubscriptionProvider>");
                return;
            }
            SubscriptionProvider provider = PluginManager.Plugins.SingleOrDefault(x => x.Names.Contains(theMessage.CommandArgs[1], StringComparer.OrdinalIgnoreCase)).As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            if (provider.GetType().GetCustomAttribute<HelpAttribute>()?.Help is { } help)
            {
                theMessage.Answer(help);
            }
            else
            {
                theMessage.Answer("Der Subscription Provider bietet keine Hilfe an");
            }
        }

        private void RemoveSubscription(IrcMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Die Funktion benötigt 3 Parameter: !subscribe remove <PluginName> <SubscriptionProvider>");
                return;
            }
            PluginInfo plugin = PluginManager.Plugins.SingleOrDefault(x => x.IsNamed(theMessage.CommandArgs[1]));
            if (plugin == null)
            {
                theMessage.Answer("Ein solches Plugin konnte ich nicht ausfindig machen");
                return;
            }
            SubscriptionProvider provider = PluginManager.Plugins.SingleOrDefault(x => x.IsNamed(theMessage.CommandArgs[2])).As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            using (var context = new BotContext())
            {
                Subscription sub = context.Subscriptions.FirstOrDefault(x => x.User.Names.Any(n => n.Name == theMessage.Nickname) && x.Provider == provider.PluginId && x.Plugin == plugin.Id);
                if (sub != null)
                {
                    context.Subscriptions.Remove(sub);
                    theMessage.Answer("Subscription entfernt");
                }
                else
                {
                    theMessage.Answer("Ich konnte keine Zutreffende Subscription finden die ich hätte entfernen können");
                }
            }
        }

        private static void SetupSubscription(IrcMessage theMessage)
        {
            SubscriptionProvider provider = PluginManager.Plugins.FirstOrDefault(x => x.IsNamed(theMessage.CommandArgs[1])).As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen SubscriptionProvider namens " + theMessage.CommandArgs[1]);
                return;
            }
            provider.ParseSubscriptionSetup(theMessage);
        }

        private static void SubscriptionsAvailable(IrcMessage theMessage)
        {
            string[] names = PluginManager.GetOfType<SubscriptionProvider>().Select(x => x.Names.FirstOrDefault()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
            theMessage.Answer("Es sind folgende SubscriptionProvider verfügbar: " + names.Join(", "));
            string[] plugins = PluginManager.Plugins.Where(x => x.IsSubscribeable).Select(x => x.Names.FirstOrDefault()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
            theMessage.Answer("Folgende Plugins werden unterstützt: " + plugins.Join(", "));
        }

        public void SubscriptionAdd(IrcMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Die Funktion benötigt mindestens 3 Parameter: !subscribe add <PluginName> <SubscriptionProvider> <Bedingung>(optional)");
                return;
            }
            SubscriptionProvider? provider = PluginManager.Get(theMessage.CommandArgs[2]).As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            if (theMessage.CommandArgs[1] != "*")
            {
                PluginBase? plugin = PluginManager.Get(theMessage.CommandArgs[1])?.Plugin;
                if (plugin == null)
                {
                    theMessage.Answer("Ein solches Plugin konnte ich nicht ausfindig machen");
                    return;
                }
                if (plugin.GetType().GetCustomAttribute<SubscribeableAttribute>() == null)
                {
                    theMessage.Answer("Dieses Plugin unterstützt keine Benachrichtigungen");
                    return;
                }
                provider.AddSubscription(theMessage, plugin);
            }
            else
            {
                List<PluginBase> availables = PluginManager.Plugins.Where(x => x.IsSubscribeable).Select(x => x.Plugin).ToList();
                foreach (PluginBase plugin in availables)
                {
                    provider.AddSubscription(theMessage, plugin);
                }
            }
        }

        public void SubscriptionsList(IrcMessage theMessage)
        {
            using (var context = new BotContext())
            {
                List<IGrouping<string, Subscription>> Subscription = context.Subscriptions.Where(x => x.User == context.Nicknames.FirstOrDefault(n => n.Name == theMessage.Nickname).User).GroupBy(x => x.Provider).ToList();
                string output = Subscription.Select(x => $"{PluginManager.GetOfType<SubscriptionProvider>().First(z => z.Id == x.Key).Names.First()}: {x.Select(y => y.Plugin).Join(", ")}").Join("; ");
                theMessage.Answer(output);
            }
        }
    }
}