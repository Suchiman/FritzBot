using System;

namespace FritzBot.Database
{
    public class SeenEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual DateTime? LastSeen { get; set; }
        public virtual DateTime? LastMessaged { get; set; }
        public virtual string? LastMessage { get; set; }
        public virtual User? User { get; set; }
    }
}