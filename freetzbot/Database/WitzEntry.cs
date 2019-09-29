using System;

namespace FritzBot.Database
{
    public class WitzEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual string Witz { get; set; } = null!;
        public virtual int Frequency { get; set; }
        public virtual User Creator { get; set; } = null!;
    }
}