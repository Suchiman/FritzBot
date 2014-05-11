using System;

namespace FritzBot.Database
{
    public class ServerChannel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public Server Server { get; set; }
    }
}