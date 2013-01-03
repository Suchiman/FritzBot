using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FritzBot.Core;

namespace FritzBot.Plugins.SubscriptionProviders
{
    public abstract class SubscriptionProvider : PluginBase
    {
        public abstract void SendNotification(User user, String message);

        public virtual void AddSubscription(ircMessage theMessage, PluginBase plugin, XElement storage)
        {
            XElement SpecificSubscription = storage.Elements("Plugin").FirstOrDefault(x => x.Attribute("Provider") != null && x.Attribute("Provider").Value == PluginID && x.Value == plugin.PluginID);
            if (SpecificSubscription == null)
            {
                SpecificSubscription = new XElement("Plugin", new XAttribute("Provider", PluginID), plugin.PluginID);
                if (theMessage.CommandArgs.Count > 3 && !String.IsNullOrEmpty(theMessage.CommandArgs[3]))
                {
                    SpecificSubscription.Add(new XAttribute("Bedingung", theMessage.CommandArgs[3]));
                }
                theMessage.Answer(String.Format("Du wirst absofort mit {0} für {1} benachrichtigt", toolbox.GetAttribute<Module.NameAttribute>(this).Names[0], toolbox.GetAttribute<Module.NameAttribute>(plugin).Names[0]));
                storage.Add(SpecificSubscription);
            }
            else
            {
                if (theMessage.CommandArgs.Count > 3 && !String.IsNullOrEmpty(theMessage.CommandArgs[3]))
                {
                    XAttribute bedingung = SpecificSubscription.Attribute("Bedingung");
                    if (bedingung == null)
                    {
                        bedingung = new XAttribute("Bedingung", theMessage.CommandArgs[3]);
                        SpecificSubscription.Add(bedingung);
                        theMessage.Answer("Bedingung für Subscription hinzugefügt");
                    }
                    else
                    {
                        bedingung.Value = theMessage.CommandArgs[3];
                        theMessage.Answer("Bedingung für Subscription geändert");
                    }
                }
                else if (SpecificSubscription.Attribute("Bedingung") != null)
                {
                    SpecificSubscription.Attribute("Bedingung").Remove();
                    theMessage.Answer("Bedingung entfernt");
                }
                else
                {
                    theMessage.Answer("Du bist bereits für dieses Plugin eingetragen");
                }
                return;
            }
        }

        public virtual void ParseSubscriptionSetup(ircMessage theMessage, XElement storage)
        {
            if (theMessage.CommandArgs.Count < 3)
            {
                theMessage.Answer("Zu wenig Parameter, probier mal: !subscribe setup <SubscriptionProvider> <Einstellung>");
                return;
            }
            XElement UserSettingsForProvider = storage.Elements("Provider").FirstOrDefault(x => x.Attribute("Name") != null && x.Attribute("Name").Value == PluginID);
            if (UserSettingsForProvider == null)
            {
                UserSettingsForProvider = new XElement("Provider", new XAttribute("Name", PluginID));
                storage.Add(UserSettingsForProvider);
            }
            UserSettingsForProvider.Value = theMessage.CommandArgs[2];
            theMessage.Answer("Einstellungen erfolgreich gespeichert");
        }

        public virtual XElement GetSettings(User user)
        {
            XElement settings = user.GetModulUserStorage("subscribe").GetElement("Settings", false);
            if (settings == null || settings.Elements("Provider").FirstOrDefault(x => x.Attribute("Name") != null && x.Attribute("Name").Value == PluginID) == null)
            {
                return null;
            }
            return settings.Elements("Provider").FirstOrDefault(x => x.Attribute("Name").Value == PluginID);
        }
    }
}
