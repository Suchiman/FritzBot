using System;

namespace FritzBot.Database
{
    public class NotificationHistory
    {
        public virtual Int64 Id { get; set; }
        public virtual string Plugin { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual string Notification { get; set; }
    }
}