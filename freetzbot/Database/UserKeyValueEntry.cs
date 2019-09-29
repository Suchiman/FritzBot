using System;

namespace FritzBot.Database
{
    public class UserKeyValueEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual string Key { get; set; } = null!;
        public virtual string Value { get; set; } = null!;
    }
}