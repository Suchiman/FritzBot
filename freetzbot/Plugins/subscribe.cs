using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FritzBot.Plugins.SubscriptionProviders;

namespace FritzBot.Plugins
{
    [Module.Name("subscribe")]
    [Module.Help("Unterbefehle: add <PluginName> <SubscriptionProvider> <Bedingung>(optional), available, list, setup <SubscriptionProvider> <Adresse>, remove <PluginName> <SubscriptionProvider>, help <SubscriptionProvider>")]
    class subscribe : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            ModulDataStorage MDS = theMessage.TheUser.GetModulUserStorage(this);
            IEnumerable<SubscriptionProvider> providers = PluginManager.GetInstance().Get<SubscriptionProvider>();

            switch (theMessage.CommandArgs[0])
            {
                case "add":
                    SubscriptionAdd(theMessage, MDS, providers);
                    return;
                case "available":
                    SubscriptionsAvailable(theMessage, providers);
                    return;
                case "list":
                    SubscriptionsList(theMessage, MDS);
                    return;
                case "setup":
                    SetupSubscription(theMessage, providers, MDS);
                    return;
                case "remove":
                    RemoveSubscription(theMessage, providers, MDS);
                    return;
                case "help":
                    HelpSubscription(theMessage, providers, MDS);
                    return;
            }
        }

        private void HelpSubscription(ircMessage theMessage, IEnumerable<SubscriptionProvider> providers, ModulDataStorage MDS)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.Answer("Die Funktion benötigt 2 Parameter: !subscribe help <SubscriptionProvider>");
                return;
            }
            SubscriptionProvider provider = PluginManager.GetInstance().Get<SubscriptionProvider>().Where(x => Module.NameAttribute.IsNamed(x, theMessage.CommandArgs[1])).Single();
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

        private void RemoveSubscription(ircMessage theMessage, IEnumerable<SubscriptionProvider> providers, ModulDataStorage MDS)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Die Funktion benötigt 3 Parameter: !subscribe remove <PluginName> <SubscriptionProvider>");
                return;
            }
            PluginBase plugin = PluginManager.GetInstance().Get<PluginBase>().Where(x => Module.NameAttribute.IsNamed(x, theMessage.CommandArgs[1])).Single();
            if (plugin == null)
            {
                theMessage.Answer("Ein solches Plugin konnte ich nicht ausfindig machen");
                return;
            }
            SubscriptionProvider provider = PluginManager.GetInstance().Get<SubscriptionProvider>().Where(x => Module.NameAttribute.IsNamed(x, theMessage.CommandArgs[2])).Single();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            XElement Subscriptions = MDS.GetElement("Subscriptions", false);
            if (Subscriptions != null)
            {
                XElement Subscription = Subscriptions.Elements("Plugin").FirstOrDefault(x => x.Attribute("Provider") != null && x.Attribute("Provider").Value == provider.PluginID && x.Value == plugin.PluginID);
                if (Subscription != null)
                {
                    Subscription.Remove();
                    theMessage.Answer("Subscription entfernt");
                }
                else
                {
                    theMessage.Answer("Ich konnte keine Zutreffende Subscription finden die ich hätte entfernen können");
                }
            }
        }

        private static void SetupSubscription(ircMessage theMessage, IEnumerable<SubscriptionProvider> providers, ModulDataStorage MDS)
        {
            SubscriptionProvider provider = providers.FirstOrDefault(x => Module.NameAttribute.IsNamed(x, theMessage.CommandArgs[1]));
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen SubscriptionProvider namens " + theMessage.CommandArgs[1]);
                return;
            }
            provider.ParseSubscriptionSetup(theMessage, MDS.GetElement("Settings", true));
        }

        private static void SubscriptionsAvailable(ircMessage theMessage, IEnumerable<SubscriptionProvider> providers)
        {
            string[] names = providers.Select(x => toolbox.GetAttribute<Module.NameAttribute>(x)).Where(x => x != null && x.Names != null && x.Names.Length > 0).Select(x => x.Names[0]).ToArray();
            theMessage.Answer("Es sind folgende SubscriptionProvider verfügbar: " + String.Join(", ", names));
            string[] plugins = PluginManager.GetInstance().Get<PluginBase>().Where(x => toolbox.GetAttribute<Module.SubscribeableAttribute>(x) != null).Select(x => toolbox.GetAttribute<Module.NameAttribute>(x)).Where(x => x != null && x.Names != null && x.Names.Length > 0).Select(x => x.Names[0]).ToArray();
            theMessage.Answer("Folgende Plugins werden unterstützt: " + String.Join(", ", plugins));
        }

        public void SubscriptionAdd(ircMessage theMessage, ModulDataStorage MDS, IEnumerable<SubscriptionProvider> providers)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Die Funktion benötigt mindestens 3 Parameter: !subscribe add <PluginName> <SubscriptionProvider> <Bedingung>(optional)");
                return;
            }
            SubscriptionProvider provider = PluginManager.GetInstance().Get<SubscriptionProvider>().Where(x => Module.NameAttribute.IsNamed(x, theMessage.CommandArgs[2])).SingleOrDefault();
            if (provider == null)
            {
                theMessage.Answer("Es gibt keinen solchen SubscriptionProvider");
                return;
            }
            if (theMessage.CommandArgs[1] != "*")
            {
                PluginBase plugin = PluginManager.GetInstance().Get<PluginBase>().Where(x => Module.NameAttribute.IsNamed(x, theMessage.CommandArgs[1])).SingleOrDefault();
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
                XElement Subscriptions = MDS.GetElement("Subscriptions", true);
                provider.AddSubscription(theMessage, plugin, Subscriptions);
            }
            else
            {
                XElement Subscriptions = MDS.GetElement("Subscriptions", true);
                List<PluginBase> availables = PluginManager.GetInstance().Get<PluginBase>().Where(x => toolbox.GetAttribute<Module.SubscribeableAttribute>(x) != null).ToList();
                foreach (PluginBase plugin in availables)
                {
                    provider.AddSubscription(theMessage, plugin, Subscriptions);
                }
            }
        }

        public void SubscriptionsList(ircMessage theMessage, ModulDataStorage MDS)
        {
            XElement Subscriptions = MDS.GetElement("Subscriptions", false);
            if (Subscriptions != null)
            {
                IEnumerable<IGrouping<String, XElement>> Subscription = Subscriptions.Elements("Plugin").Where(x => x.Attribute("Provider") != null).GroupBy(x => x.Attribute("Provider").Value);
                string output = String.Join("; ", Subscription.Select(x => String.Format("{0}: {1}", toolbox.GetAttribute<Module.NameAttribute>(PluginManager.GetInstance().Get<SubscriptionProvider>().First(z => z.PluginID == x.Key)).Names.First(), String.Join(", ", x.Select(y => y.Value).ToArray()))).ToArray());
                theMessage.Answer(output);
            }
        }
    }
}