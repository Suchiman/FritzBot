using System;
using System.Collections.Generic;

namespace FritzBot.Database
{
    public class Subscription
    {
        public Int64 Id { get; set; }
        public string Provider { get; set; }
        public string Plugin { get; set; }
        public User User { get; set; }
        public List<SubscriptionBedingung> Bedingungen { get; set; }

        public Subscription()
        {
            Bedingungen = new List<SubscriptionBedingung>();
        }
    }
}