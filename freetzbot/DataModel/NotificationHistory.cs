using System;

namespace FritzBot.DataModel
{
    public class NotificationHistory
    {
        public string Plugin { get; set; }
        public DateTime Created { get; set; }
        public string Notification { get; set; }
    }
}
