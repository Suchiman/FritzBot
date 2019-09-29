using System;

namespace FritzBot.Database
{
    public class BoxRegexPattern
    {
        public virtual Int64 Id { get; set; }
        public virtual string Pattern { get; set; } = null!;
        public virtual Box Box { get; set; } = null!;
    }
}