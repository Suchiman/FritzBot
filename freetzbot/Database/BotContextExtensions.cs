using System.Data.Entity;
using System.Linq;

namespace FritzBot.Database
{
    public static class BotContextExtensions
    {
        public static User GetUser(this BotContext context, string nickname)
        {
            Nickname nick = context.Nicknames.Include(x => x.User).FirstOrDefault(x => x.Name == nickname);
            if (nick != null)
            {
                return nick.User;
            }
            return null;
        }

        public static UserKeyValueEntry GetStorage(this BotContext context, string nickname, string key)
        {
            return context.UserKeyValueEntries.FirstOrDefault(x => x.User == context.Nicknames.FirstOrDefault(n => n.Name == nickname).User && x.Key == key);
        }

        public static UserKeyValueEntry GetStorage(this BotContext context, User user, string key)
        {
            return context.UserKeyValueEntries.FirstOrDefault(x => x.User == user && x.Key == key);
        }

        public static UserKeyValueEntry GetStorageOrCreate(this BotContext context, User user, string key)
        {
            UserKeyValueEntry entry = context.UserKeyValueEntries.FirstOrDefault(x => x.User.Id == user.Id && x.Key == key);
            if (entry == null)
            {
                entry = new UserKeyValueEntry { Key = key, User = user };
                context.UserKeyValueEntries.Add(entry);
            }
            return entry;
        }

        public static UserKeyValueEntry GetStorageOrCreate(this BotContext context, string nickname, string key)
        {
            User user = context.GetUser(nickname);
            UserKeyValueEntry entry = context.UserKeyValueEntries.FirstOrDefault(x => x.User.Id == user.Id && x.Key == key);
            if (entry == null)
            {
                entry = new UserKeyValueEntry { Key = key, User = user };
                context.UserKeyValueEntries.Add(entry);
            }
            return entry;
        }
    }
}