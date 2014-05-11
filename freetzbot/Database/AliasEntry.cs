using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FritzBot.Database
{
    public class AliasEntry
    {
        public Int64 Id { get; set; }
        [Required]
        [Index(IsUnique = true)]
        public string Key { get; set; }
        public string Text { get; set; }
        public User Creator { get; set; }
        public DateTime? Created { get; set; }
        public User Updater { get; set; }
        public DateTime? Updated { get; set; }
    }
}