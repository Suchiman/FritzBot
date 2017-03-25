using System;
using System.ComponentModel.DataAnnotations;

namespace FritzBot.Database
{
    public class AliasEntry
    {
        public virtual Int64 Id { get; set; }
        [Required]
        public virtual string Key { get; set; }
        public virtual string Text { get; set; }
        public virtual User Creator { get; set; }
        public virtual DateTime? Created { get; set; }
        public virtual User Updater { get; set; }
        public virtual DateTime? Updated { get; set; }
    }
}