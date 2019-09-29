﻿using System;
using System.Collections.Generic;

namespace FritzBot.Database
{
    public class Server
    {
        public virtual Int64 Id { get; set; }
        public virtual string Address { get; set; } = null!;
        public virtual int Port { get; set; }
        public virtual string Nickname { get; set; } = null!;
        public virtual string? QuitMessage { get; set; }
        public virtual string? NickServPassword { get; set; }
        public virtual ICollection<ServerChannel> Channels { get; set; } = null!;
    }
}