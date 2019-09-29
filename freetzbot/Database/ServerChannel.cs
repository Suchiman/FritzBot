using System;

namespace FritzBot.Database
{
    public class ServerChannel
    {
        public virtual Int64 Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual Server Server { get; set; } = null!;
    }
}