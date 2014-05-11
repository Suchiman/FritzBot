using System;

namespace FritzBot.Database
{
    public class BoxRegexPattern
    {
        public Int64 Id { get; set; }
        public string Pattern { get; set; }
        public Box Box { get; set; }
    }
}