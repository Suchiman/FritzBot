using System;

namespace FritzBot.Database
{
    public class Nickname
    {
        public virtual Int64 Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public override string ToString()
        {
            return Name;
        }
    }
}