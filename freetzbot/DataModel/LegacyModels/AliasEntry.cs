using System;

namespace FritzBot.DataModel.LegacyModels
{
    public class AliasEntry
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public User Creator { get; set; }
        public DateTime Created { get; set; }
        public User Updater { get; set; }
        public DateTime Updated { get; set; }
    }
}
