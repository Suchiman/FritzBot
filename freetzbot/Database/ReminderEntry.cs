using System;

namespace FritzBot.Database
{
    public class ReminderEntry
    {
        public Int64 Id { get; set; }
        public User Creator { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public User User { get; set; }
    }
}