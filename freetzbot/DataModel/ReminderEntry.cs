using FritzBot.Core;
using System;

namespace FritzBot.DataModel
{
    public class ReminderEntry : LinkedData<User>
    {
        public User Creator { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
    }
}
