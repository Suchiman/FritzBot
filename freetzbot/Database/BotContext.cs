using Microsoft.EntityFrameworkCore;

namespace FritzBot.Database
{
    public class BotContext : DbContext
    {
        public DbSet<AliasEntry> AliasEntries { get; set; } = null!;
        public DbSet<Box> Boxes { get; set; } = null!;
        public DbSet<BoxEntry> BoxEntries { get; set; } = null!;
        public DbSet<Nickname> Nicknames { get; set; } = null!;
        public DbSet<NotificationHistory> NotificationHistories { get; set; } = null!;
        public DbSet<ReminderEntry> ReminderEntries { get; set; } = null!;
        public DbSet<SeenEntry> SeenEntries { get; set; } = null!;
        public DbSet<Server> Servers { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserKeyValueEntry> UserKeyValueEntries { get; set; } = null!;
        public DbSet<WitzEntry> WitzEntries { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlite(@"Data Source=database.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(x => x.Names)
                .WithOne(x => x.User);

            modelBuilder.Entity<AliasEntry>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<Nickname>()
                .HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}