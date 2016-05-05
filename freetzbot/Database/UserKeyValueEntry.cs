using System;

namespace FritzBot.Database
{
    public class UserKeyValueEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual User User { get; set; }
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }
    }
}