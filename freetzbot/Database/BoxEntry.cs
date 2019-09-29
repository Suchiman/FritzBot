using System;

namespace FritzBot.Database
{
    public class BoxEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual string Text { get; set; } = null!;
        public virtual Box? Box { get; set; }
        public virtual User User { get; set; } = null!;
    }
}