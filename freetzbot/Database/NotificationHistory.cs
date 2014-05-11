using System;

namespace FritzBot.Database
{
    public class NotificationHistory
    {
        public Int64 Id { get; set; }
        public string Plugin { get; set; }
        public DateTime Created { get; set; }
        public string Notification { get; set; }
    }
}