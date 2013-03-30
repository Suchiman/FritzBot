using FritzBot.Core;
using System;

namespace FritzBot.DataModel
{
    public class SeenEntry : LinkedData<User>
    {
        public DateTime LastSeen { get; set; }
        public DateTime LastMessaged { get; set; }
        public string LastMessage { get; set; }
    }
}
