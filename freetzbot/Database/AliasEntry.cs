using System;

namespace FritzBot.Database
{
    public class AliasEntry
    {
        public virtual Int64 Id { get; set; }
        public virtual string Key { get; set; } = null!;
        public virtual string Text { get; set; } = null!;
        public virtual User Creator { get; set; } = null!;
        public virtual DateTime? Created { get; set; }
        public virtual User? Updater { get; set; }
        public virtual DateTime? Updated { get; set; }
    }
}