using System;
using System.Collections.Generic;

namespace FritzBot.Database
{
    public class Subscription
    {
        public virtual Int64 Id { get; set; }
        public virtual string Provider { get; set; }
        public virtual string Plugin { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<SubscriptionBedingung> Bedingungen { get; set; }
    }
}