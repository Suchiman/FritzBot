using FritzBot.DataModel;
using FritzBot.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzBot.Plugins
{
    [Module.Name("geolocator")]
    [Module.Help("Findet die Geolocation einer IP Adresse !geolocator <ip-address>")]
    [Module.ParameterRequired]
    public class geolocator : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            LocationInfoDetailed details = Geolocation.GetLocationInfoDetailed(theMessage.CommandArgs.FirstOrDefault());
            if (details.Success)
            {
                theMessage.Answer(String.Format("Die IP {0} befindet sich im Land {1} ({2}), vermutlich in der Stadt {3}. Dort ist es gerade {4} Uhr", details.ipAddress, details.countryName, details.countryCode, details.cityName, details.LocalTime.ToString()));
            }
            else
            {
                theMessage.Answer("Lokalisierung fehlgeschlagen");
            }
        }
    }
}
