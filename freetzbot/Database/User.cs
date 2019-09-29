using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FritzBot.Database
{
    public class User
    {
        public virtual Int64 Id { get; set; }
        public virtual ICollection<Nickname> Names { get; set; } = null!;
        public virtual Nickname LastUsedName { get; set; } = null!;
        public virtual string? Password { get; set; }
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
        public virtual ICollection<UserKeyValueEntry> UserStorage { get; set; } = null!;

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