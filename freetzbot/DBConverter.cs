using FritzBot.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DM = FritzBot.DataModel.LegacyModels;

namespace FritzBot
{
    public static class DBConverter
    {
        public static void Convert()
        {
            var dcr = new DefaultContractResolver();
            dcr.DefaultMembersSearchFlags |= BindingFlags.NonPublic;

            List<object> db = JsonConvert.DeserializeObject<List<object>>(File.ReadAllText("output.json"), new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All, TypeNameHandling = TypeNameHandling.All, ContractResolver = dcr });
            using (BotContext context = new BotContext())
            {
                context.Database.Delete();

                Dictionary<User, string> LastUseds = new Dictionary<User, string>(2000);
                foreach (DM.User user in db.OfType<DM.User>())
                {
                    User u = new User();
                    u.Names = user.Names.Select(x => new Nickname { Name = x, User = u }).ToList();
                    u.Admin = user.Admin;
                    u.Authentication = user.Authentication > DateTime.MinValue ? (DateTime?)user.Authentication : null;
                    u.Ignored = user.Ignored;
                    u.Password = String.IsNullOrWhiteSpace(user.Password) ? null : user.Password;
                    LastUseds[u] = user.LastUsedName;
                    context.Users.Add(u);
                }
                var list = context.Users.Local.SelectMany(x => x.Names).GroupBy(x => x.Name).Where(x => x.Count() > 1).Select(x => new { Name = x.Key, Users = x.Select(s => s.User).ToList() }).ToList();
                context.SaveChanges();

                foreach (KeyValuePair<User, string> pair in LastUseds)
                {
                    pair.Key.LastUsedName = pair.Key.Names.FirstOrDefault(x => x.Name == pair.Value) ?? pair.Key.Names.FirstOrDefault();
                }
                context.SaveChanges();

                foreach (DM.AliasEntry aliasEntry in db.OfType<DM.AliasEntry>())
                {
                    AliasEntry entry = new AliasEntry();
                    entry.Created = aliasEntry.Created > DateTime.MinValue ? (DateTime?)aliasEntry.Created : null;
                    if (aliasEntry.Creator != null)
                    {
                        entry.Creator = context.Users.FirstOrDefault(x => x.LastUsedName.Name == aliasEntry.Creator.LastUsedName);
                    }
                    entry.Key = aliasEntry.Key;
                    entry.Text = aliasEntry.Text;
                    entry.Updated = aliasEntry.Updated > DateTime.MinValue ? (DateTime?)aliasEntry.Updated : null;
                    if (aliasEntry.Updater != null)
                    {
                        entry.Updater = context.Users.FirstOrDefault(x => x.LastUsedName.Name == aliasEntry.Updater.LastUsedName);
                    }
                    context.AliasEntries.Add(entry);
                }
                context.SaveChanges();

                foreach (DM.Box box in db.OfType<DM.Box>())
                {
                    Box b = new Box();
                    b.AddRegex(box.RegexPattern.ToArray());
                    b.FullName = box.FullName;
                    b.ShortName = box.ShortName;
                    context.Boxes.Add(b);
                }
                context.SaveChanges();

                foreach (DM.BoxEntry boxEntry in db.OfType<DM.BoxEntry>())
                {
                    foreach (KeyValuePair<string, DM.Box> pair in boxEntry.Entrys)
                    {
                        BoxEntry entry = new BoxEntry();
                        entry.Text = pair.Key;
                        if (pair.Value != null)
                        {
                            entry.Box = context.Boxes.FirstOrDefault(x => x.FullName == pair.Value.FullName && x.ShortName == pair.Value.ShortName);
                        }
                        entry.User = context.Users.FirstOrDefault(x => x.LastUsedName.Name == boxEntry.Reference.LastUsedName);
                        context.BoxEntries.Add(entry);
                    }
                }
                context.SaveChanges();

                foreach (DM.WitzEntry wizEntry in db.OfType<DM.WitzEntry>())
                {
                    WitzEntry entry = new WitzEntry();
                    entry.Frequency = 0;
                    if (wizEntry.Reference != null)
                    {
                        entry.Creator = context.Users.FirstOrDefault(x => wizEntry.Reference.LastUsedName == x.LastUsedName.Name);
                    }
                    entry.Witz = wizEntry.Witz;
                    context.WitzEntries.Add(entry);
                }
                context.SaveChanges();

                foreach (DM.SeenEntry seenEntry in db.OfType<DM.SeenEntry>())
                {
                    if (String.IsNullOrWhiteSpace(seenEntry.LastMessage) && seenEntry.LastMessaged < DateTime.MinValue && seenEntry.LastSeen < DateTime.MinValue)
                    {
                        continue;
                    }
                    SeenEntry entry = new SeenEntry();
                    entry.LastMessage = seenEntry.LastMessage;
                    entry.LastMessaged = seenEntry.LastMessaged == DateTime.MinValue ? null : (DateTime?)seenEntry.LastMessaged;
                    entry.LastSeen = seenEntry.LastSeen == DateTime.MinValue ? null : (DateTime?)seenEntry.LastSeen;
                    if (seenEntry.Reference != null)
                    {
                        entry.User = context.Users.FirstOrDefault(x => x.LastUsedName.Name == seenEntry.Reference.LastUsedName);
                    }
                    context.SeenEntries.Add(entry);
                }
                context.SaveChanges();

                foreach (DM.ReminderEntry reminderEntry in db.OfType<DM.ReminderEntry>())
                {
                    ReminderEntry entry = new ReminderEntry();
                    entry.Created = reminderEntry.Created;
                    if (reminderEntry.Creator != null)
                    {
                        entry.Creator = context.Users.FirstOrDefault(x => x.LastUsedName.Name == reminderEntry.Creator.LastUsedName);
                    }
                    entry.Message = reminderEntry.Message;
                    if (reminderEntry.Reference != null)
                    {
                        entry.User = context.Users.FirstOrDefault(x => x.LastUsedName.Name == reminderEntry.Reference.LastUsedName);
                    }
                    context.ReminderEntries.Add(entry);
                }
                context.SaveChanges();

                foreach (DM.NotificationHistory notificationHistory in db.OfType<DM.NotificationHistory>())
                {
                    NotificationHistory entry = new NotificationHistory();
                    entry.Created = notificationHistory.Created;
                    entry.Notification = notificationHistory.Notification;
                    entry.Plugin = notificationHistory.Plugin;
                    context.NotificationHistories.Add(entry);
                }
                context.SaveChanges();

                foreach (DM.Subscription subscription in db.OfType<DM.Subscription>())
                {
                    Subscription entry = new Subscription();
                    entry.Bedingungen = subscription.Bedingungen.Select(x => new SubscriptionBedingung { Bedingung = x }).ToList();
                    entry.Plugin = subscription.Plugin;
                    entry.Provider = subscription.Provider;
                    if (subscription.Reference != null)
                    {
                        entry.User = context.Users.FirstOrDefault(x => x.LastUsedName.Name == subscription.Reference.LastUsedName);
                    }
                    context.Subscriptions.Add(entry);
                }
                context.SaveChanges();

                foreach (DM.SimpleStorage simpleStorage in db.OfType<DM.SimpleStorage>())
                {
                    switch (simpleStorage.ID)
                    {
                        case "frag":
                            if (simpleStorage.Get("asked", false))
                            {
                                DM.User reference0 = simpleStorage.Reference as DM.User;
                                if (reference0 != null)
                                {
                                    User user = context.GetUser(reference0.LastUsedName);
                                    if (user != null)
                                    {
                                        context.GetStorageOrCreate(user, "frag_asked").Value = "true";
                                    }
                                }
                            }
                            break;
                        case "SubscriptionSettings":
                            DM.User reference1 = simpleStorage.Reference as DM.User;
                            if (reference1 != null)
                            {
                                User user = context.GetUser(reference1.LastUsedName);
                                if (user != null)
                                {
                                    foreach (KeyValuePair<string, object> value in simpleStorage._values)
                                    {
                                        context.GetStorageOrCreate(user, value.Key).Value = value.Value.ToString();
                                    }
                                }
                            }
                            break;
                        case "login":
                            break;
                        default:
                            throw new NotImplementedException(simpleStorage.ID);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}