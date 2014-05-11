using System;

namespace FritzBot.Database
{
    public class SeenEntry
    {
        public Int64 Id { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? LastMessaged { get; set; }
        public string LastMessage { get; set; }
        public User User { get; set; }
    }
}