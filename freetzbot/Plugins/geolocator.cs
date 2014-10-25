using FritzBot.DataModel;
using FritzBot.Functions;
using Meebey.SmartIrc4net;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Name("geolocator", "geolocation")]
    [Help("Findet die Geolocation einer IP Adresse !geolocator <ip-address>")]
    [ParameterRequired]
    public class geolocator : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            string address = theMessage.CommandArgs.FirstOrDefault();
            try
            {
                IrcUser user = theMessage.Data.Irc.GetIrcUser(address);
                address = user.Host;
            }
            catch
            {
            }
            Console.WriteLine("Führe ortung durch für: " + address);

            LocationInfoDetailed details = Geolocation.GetLocationInfoDetailed(address);
            if (details.Success)
            {
                if (String.IsNullOrEmpty(details.timeZone))
                {
                    theMessage.Answer(String.Format("Die IP {0} befindet sich im Land {1} ({2})", details.ipAddress, details.countryName, details.countryCode));
                }
                else
                {
                    theMessage.Answer(String.Format("Die IP {0} befindet sich im Land {1} ({2}), vermutlich in der Stadt {3}. Dort ist es gerade {4} Uhr", details.ipAddress, details.countryName, details.countryCode, details.cityName, details.LocalTime.ToString()));
                }
            }
            else
            {
                theMessage.Answer("Lokalisierung fehlgeschlagen");
            }
        }
    }
}