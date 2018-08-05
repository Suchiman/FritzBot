using FritzBot.DataModel;
using FritzBot.Functions;
using Meebey.SmartIrc4net;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

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
            catch { }

            Match match = Regex.Match(address, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            if (match.Success && IPAddress.TryParse(match.Groups[0].Value, out IPAddress parsedAddress))
            {
                address = parsedAddress.ToString();
            }

            LocationInfoDetailed details = Geolocation.GetLocationInfoDetailed(address);
            if (details.Success)
            {
                if (String.IsNullOrEmpty(details.timeZone))
                {
                    theMessage.Answer($"Die IP {details.ipAddress} befindet sich im Land {details.countryName} ({details.countryCode})");
                }
                else
                {
                    theMessage.Answer($"Die IP {details.ipAddress} befindet sich im Land {details.countryName} ({details.countryCode}), vermutlich in der Stadt {details.cityName}. Dort ist es gerade {details.LocalTime} Uhr");
                }
            }
            else
            {
                theMessage.Answer("Lokalisierung fehlgeschlagen");
            }
        }
    }
}