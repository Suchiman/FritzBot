using System;

namespace FritzBot.Database
{
    public class UserKeyValueEntry
    {
        public Int64 Id { get; set; }
        public User User { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}