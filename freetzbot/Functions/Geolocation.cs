using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;

namespace FritzBot.Functions
{
    public static class Geolocation
    {
        private const string APIUrl = "http://api.ipinfodb.com/v3/ip-{0}/?format=json&key=a97de1a8f890097cc2e32558555d836957229706b9b3ac264ef3cfe10e54ea69&ip={1}";

        public static string GetCountryCode(string ip)
        {
            LocationInfo info = GetLocationInfoSimple(ip);
            if (info.Success)
            {
                return info.countryCode;
            }
            return String.Empty;
        }

        private static string SafeGet(string ip, string preferedMode)
        {
            IPAddress ad;
            if (IPAddress.TryParse(ip, out ad))
            {
                if (ad.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    preferedMode = "country"; //IPv6 Lokalisierung scheint nur im Country Mode zu funktionieren
                }
            }
            else
            {
                try
                {
                    IPHostEntry entry = Dns.GetHostEntry(ip);
                    if (entry != null && entry.AddressList.Length > 0)
                    {
                        ad = entry.AddressList[0];
                        if (ad.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            preferedMode = "country"; //IPv6 Lokalisierung scheint nur im Country Mode zu funktionieren
                        }
                        ip = ad.ToString();
                    }
                }
                catch
                {
                }
            }

            string url = String.Format(APIUrl, preferedMode, ip);
            string response = toolbox.GetWeb(url);

            if (String.IsNullOrEmpty(response) || response.Contains("ERROR"))
            {
                response = "{\"statusCode\":\"NOK\"}";
            }

            return response;
        }

        public static LocationInfo GetLocationInfoSimple(string ip)
        {
            return JsonConvert.DeserializeObject<LocationInfo>(SafeGet(ip, "country"));
        }

        public static LocationInfoDetailed GetLocationInfoDetailed(string ip)
        {
            return JsonConvert.DeserializeObject<LocationInfoDetailed>(SafeGet(ip, "city"));
        }
    }

    public class LocationInfo
    {
        public bool Success
        {
            get
            {
                return statusCode == "OK" && !String.IsNullOrEmpty(countryCode) && countryCode != "-";
            }
        }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string ipAddress { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
    }

    public class LocationInfoDetailed : LocationInfo
    {
        public string regionName { get; set; }
        public string cityName { get; set; }
        public string zipCode { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string timeZone { get; set; }
        public DateTime LocalTime
        {
            get
            {
                if (String.IsNullOrEmpty(timeZone))
                {
                    throw new InvalidOperationException("Es wurde keine timeZone geliefert");
                }
                return DateTime.UtcNow.AddHours(Int32.Parse(timeZone.Substring(0, 3)));
            }
        }
    }
}