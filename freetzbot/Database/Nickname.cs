using System;
using System.ComponentModel.DataAnnotations;

namespace FritzBot.Database
{
    public class Nickname
    {
        public virtual Int64 Id { get; set; }
        public virtual string Name { get; set; }
        [Required]
        public virtual User User { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}