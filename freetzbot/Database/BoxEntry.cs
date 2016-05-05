using System;
using System.ComponentModel.DataAnnotations;

namespace FritzBot.Database
{
    public class BoxEntry
    {
        public virtual Int64 Id { get; set; }
        [Required]
        public virtual string Text { get; set; }
        public virtual Box Box { get; set; }
        public virtual User User { get; set; }
    }
}