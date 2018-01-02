using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FritzBot.Database
{
    public class User
    {
        public virtual Int64 Id { get; set; }
        public virtual List<Nickname> Names { get; set; }
        public virtual Nickname LastUsedName { get; set; }
        public virtual string Password { get; set; }
        public virtual DateTime? Authentication { get; set; }
        [NotMapped]
        public bool Authenticated
        {
            get
            {
                return Authentication.HasValue && Authentication.Value > DateTime.Now.AddDays(-1);
            }
        }
        public virtual bool Ignored { get; set; }
        public virtual bool Admin { get; set; }
        public virtual ICollection<UserKeyValueEntry> UserStorage { get; set; }

        public void SetPassword(string pw)
        {
            Password = Toolbox.Crypt(pw);
        }

        public bool CheckPassword(string pw)
        {
            return Password == Toolbox.Crypt(pw);
        }

        public override string ToString()
        {
            return LastUsedName.Name;
        }
    }
}