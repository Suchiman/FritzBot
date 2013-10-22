using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.Plugins.SubscriptionProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("subscribe")]
    [Module.Help("Unterbefehle: add <PluginName> <SubscriptionProvider> <Bedingung>(optional), available, list, setup <SubscriptionProvider> <Adresse>, remove <PluginName> <SubscriptionProvider>, help <SubscriptionProvider>")]
    class subscribe : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
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

        private void HelpSubscription(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.Answer("Die Funktion benötigt 2 Parameter: !subscribe help <SubscriptionProvider>");
                return;
            }
            SubscriptionProvider provider = PluginManager.GetInstance().Where(x => x.Names.Contains(theMessage.CommandArgs[1], StringComparer.OrdinalIgnoreCase)).SingleOrDefault().As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            else
            {
                Module.HelpAttribute help = toolbox.GetAttribute<Module.HelpAttribute>(provider);
                if (help != null)
                {
                    theMessage.Answer(help.Help);
                }
                else
                {
                    theMessage.Answer("Der Subscription Provider bietet keine Hilfe an");
                }
            }
        }

        private void RemoveSubscription(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Die Funktion benötigt 3 Parameter: !subscribe remove <PluginName> <SubscriptionProvider>");
                return;
            }
            PluginInfo plugin = PluginManager.GetInstance().Where(x => x.IsNamed(theMessage.CommandArgs[1])).SingleOrDefault();
            if (plugin == null)
            {
                theMessage.Answer("Ein solches Plugin konnte ich nicht ausfindig machen");
                return;
            }
            SubscriptionProvider provider = PluginManager.GetInstance().Where(x => x.IsNamed(theMessage.CommandArgs[2])).SingleOrDefault().As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            using (DBProvider db = new DBProvider())
            {
                Subscription sub = db.QueryLinkedData<Subscription, User>(theMessage.TheUser).FirstOrDefault(x => x.Provider == provider.PluginID && x.Plugin == plugin.ID);
                if (sub != null)
                {
                    db.Remove(sub);
                    theMessage.Answer("Subscription entfernt");
                }
                else
                {
                    theMessage.Answer("Ich konnte keine Zutreffende Subscription finden die ich hätte entfernen können");
                }
            }
        }

        private static void SetupSubscription(ircMessage theMessage)
        {
            Contract.Requires(theMessage != null);

            SubscriptionProvider provider = PluginManager.GetInstance().Where(x => x.IsNamed(theMessage.CommandArgs[1])).FirstOrDefault().As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen SubscriptionProvider namens " + theMessage.CommandArgs[1]);
                return;
            }
            provider.ParseSubscriptionSetup(theMessage);
        }

        private static void SubscriptionsAvailable(ircMessage theMessage)
        {
            string[] names = PluginManager.GetInstance().GetOfType<SubscriptionProvider>().Select(x => x.Names.FirstOrDefault()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
            theMessage.Answer("Es sind folgende SubscriptionProvider verfügbar: " + String.Join(", ", names));
            string[] plugins = PluginManager.GetInstance().Where(x => x.IsSubscribeable).Select(x => x.Names.FirstOrDefault()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
            theMessage.Answer("Folgende Plugins werden unterstützt: " + String.Join(", ", plugins));
        }

        public void SubscriptionAdd(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Die Funktion benötigt mindestens 3 Parameter: !subscribe add <PluginName> <SubscriptionProvider> <Bedingung>(optional)");
                return;
            }
            SubscriptionProvider provider = PluginManager.GetInstance().Get(theMessage.CommandArgs[2]).As<SubscriptionProvider>();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            if (theMessage.CommandArgs[1] != "*")
            {
                PluginBase plugin = PluginManager.GetInstance().Get(theMessage.CommandArgs[1]).As<PluginBase>();
                if (plugin == null)
                {
                    theMessage.Answer("Ein solches Plugin konnte ich nicht ausfindig machen");
                    return;
                }
                if (toolbox.GetAttribute<Module.SubscribeableAttribute>(plugin) == null)
                {
                    theMessage.Answer("Dieses Plugin unterstützt keine Benachrichtigungen");
                    return;
                }
                provider.AddSubscription(theMessage, plugin);
            }
            else
            {
                List<PluginBase> availables = PluginManager.GetInstance().Where(x => x.IsSubscribeable).Select(x => x.Plugin).ToList();
                foreach (PluginBase plugin in availables)
                {
                    provider.AddSubscription(theMessage, plugin);
                }
            }
        }

        public void SubscriptionsList(ircMessage theMessage)
        {
            IEnumerable<IGrouping<string, Subscription>> Subscription;
            using (DBProvider db = new DBProvider())
            {
                Subscription = db.QueryLinkedData<Subscription, User>(theMessage.TheUser).GroupBy(x => x.Provider);
            }
            string output = String.Join("; ", Subscription.Select(x => String.Format("{0}: {1}", PluginManager.GetInstance().GetOfType<SubscriptionProvider>().First(z => z.ID == x.Key).Names.First(), String.Join(", ", x.Select(y => y.Plugin)))));
            theMessage.Answer(output);
        }
    }
}