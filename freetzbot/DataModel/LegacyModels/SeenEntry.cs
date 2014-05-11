using System;

namespace FritzBot.DataModel.LegacyModels
{
    public class SeenEntry : LinkedData<User>
    {
        public DateTime LastSeen { get; set; }
        public DateTime LastMessaged { get; set; }
        public string LastMessage { get; set; }
    }
}
