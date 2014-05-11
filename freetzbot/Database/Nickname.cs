using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FritzBot.Database
{
    public class Nickname
    {
        public Int64 Id { get; set; }
        [Index(IsUnique = true)]
        public string Name { get; set; }
        [Required]
        public User User { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}