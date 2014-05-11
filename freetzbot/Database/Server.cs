using System;
using System.Collections.Generic;

namespace FritzBot.Database
{
    public class Server
    {
        public Int64 Id { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Nickname { get; set; }
        public string QuitMessage { get; set; }
        public List<ServerChannel> Channels { get; set; }
    }
}