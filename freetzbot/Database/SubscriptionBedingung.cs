using System;

namespace FritzBot.Database
{
    public class SubscriptionBedingung
    {
        public virtual Int64 Id { get; set; }
        public virtual string Bedingung { get; set; } = null!;
    }
}