using System;

namespace FritzBot.Database
{
    public class ReminderEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual User Creator { get; set; }
        public virtual string Message { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual User User { get; set; }
    }
}