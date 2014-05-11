using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FritzBot.Database
{
    public class User
    {
        public Int64 Id { get; set; }
        public List<Nickname> Names { get; set; }
        public Nickname LastUsedName { get; set; }
        public string Password { get; set; }
        public DateTime? Authentication { get; set; }
        [NotMapped]
        public bool Authenticated
        {
            get
            {
                return Authentication.HasValue && Authentication.Value > DateTime.Now.AddDays(-1);
            }
        }
        public bool Ignored { get; set; }
        public bool Admin { get; set; }
        public List<UserKeyValueEntry> UserStorage { get; set; }

        public void SetPassword(string pw)
        {
            Password = toolbox.Crypt(pw);
        }

        public bool CheckPassword(string pw)
        {
            if (Password == toolbox.Crypt(pw))
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return LastUsedName.Name;
        }
    }
}