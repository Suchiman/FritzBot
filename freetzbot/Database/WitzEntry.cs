using System;

namespace FritzBot.Database
{
    public class WitzEntry
    {
        public Int64 Id { get; set; }
        public string Witz { get; set; }
        public int Frequency { get; set; }
        public User Creator { get; set; }
    }
}