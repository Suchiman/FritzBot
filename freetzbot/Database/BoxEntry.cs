using System;
using System.ComponentModel.DataAnnotations;

namespace FritzBot.Database
{
    public class BoxEntry
    {
        public Int64 Id { get; set; }
        [Required]
        public string Text { get; set; }
        public Box Box { get; set; }
        public User User { get; set; }
    }
}