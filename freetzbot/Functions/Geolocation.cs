using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace FritzBot.Functions
{
    public static class Geolocation
    {
        private const string APIUrl = "http://api.ipinfodb.com/v3/ip-{0}/?key=a97de1a8f890097cc2e32558555d836957229706b9b3ac264ef3cfe10e54ea69&ip={1}&format=json";

        public static string GetCountryCode(string ip)
        {
            LocationInfo info = GetLocationInfoSimple(ip);
            if (info.Success)
            {
                return info.countryCode;
            }
            return String.Empty;
        }

        public static LocationInfo GetLocationInfoSimple(string ip)
        {
            string url = String.Format(APIUrl, "country", ip);
            string response = toolbox.GetWeb(url);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            LocationInfo location = serializer.Deserialize<LocationInfo>(response);
            return location;
        }

        public static LocationInfoDetailed GetLocationInfoDetailed(string ip)
        {
            string url = String.Format(APIUrl, "city", ip);
            string response = toolbox.GetWeb(url);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            LocationInfoDetailed location = serializer.Deserialize<LocationInfoDetailed>(response);
            return location;
        }
    }

    public class LocationInfo
    {
        public bool Success
        {
            get
            {
                return statusCode == "OK" && !String.IsNullOrEmpty(countryCode);
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