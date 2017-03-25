using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using FritzBot.Database;

namespace FritzBot.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20170325153515_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.2.0-preview1-24098");

            modelBuilder.Entity("FritzBot.Database.AliasEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("Created");

                    b.Property<long?>("CreatorId");

                    b.Property<string>("Key")
                        .IsRequired();

                    b.Property<string>("Text");

                    b.Property<DateTime?>("Updated");

                    b.Property<long?>("UpdaterId");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("UpdaterId");

                    b.ToTable("AliasEntries");
                });

            modelBuilder.Entity("FritzBot.Database.Box", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FullName");

                    b.Property<string>("ShortName");

                    b.HasKey("Id");

                    b.ToTable("Boxes");
                });

            modelBuilder.Entity("FritzBot.Database.BoxEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("BoxId");

                    b.Property<string>("Text")
                        .IsRequired();

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("BoxId");

                    b.HasIndex("UserId");

                    b.ToTable("BoxEntries");
                });

            modelBuilder.Entity("FritzBot.Database.BoxRegexPattern", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("BoxId");

                    b.Property<string>("Pattern");

                    b.HasKey("Id");

                    b.HasIndex("BoxId");

                    b.ToTable("BoxRegexPattern");
                });

            modelBuilder.Entity("FritzBot.Database.Nickname", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Nicknames");
                });

            modelBuilder.Entity("FritzBot.Database.NotificationHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Notification");

                    b.Property<string>("Plugin");

                    b.HasKey("Id");

                    b.ToTable("NotificationHistories");
                });

            modelBuilder.Entity("FritzBot.Database.ReminderEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<long?>("CreatorId");

                    b.Property<string>("Message");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("UserId");

                    b.ToTable("ReminderEntries");
                });

            modelBuilder.Entity("FritzBot.Database.SeenEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LastMessage");

                    b.Property<DateTime?>("LastMessaged");

                    b.Property<DateTime?>("LastSeen");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("SeenEntries");
                });

            modelBuilder.Entity("FritzBot.Database.Server", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Nickname");

                    b.Property<int>("Port");

                    b.Property<string>("QuitMessage");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("FritzBot.Database.ServerChannel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<long?>("ServerId");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.ToTable("ServerChannel");
                });

            modelBuilder.Entity("FritzBot.Database.Subscription", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Plugin");

                    b.Property<string>("Provider");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("FritzBot.Database.SubscriptionBedingung", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Bedingung");

                    b.Property<long?>("SubscriptionId");

                    b.HasKey("Id");

                    b.HasIndex("SubscriptionId");

                    b.ToTable("SubscriptionBedingung");
                });

            modelBuilder.Entity("FritzBot.Database.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Admin");

                    b.Property<DateTime?>("Authentication");

                    b.Property<bool>("Ignored");

                    b.Property<long?>("LastUsedNameId");

                    b.Property<string>("Password");

                    b.HasKey("Id");

                    b.HasIndex("LastUsedNameId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FritzBot.Database.UserKeyValueEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<long?>("UserId");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserKeyValueEntries");
                });

            modelBuilder.Entity("FritzBot.Database.WitzEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("CreatorId");

                    b.Property<int>("Frequency");

                    b.Property<string>("Witz");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("WitzEntries");
                });

            modelBuilder.Entity("FritzBot.Database.AliasEntry", b =>
                {
                    b.HasOne("FritzBot.Database.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");

                    b.HasOne("FritzBot.Database.User", "Updater")
                        .WithMany()
                        .HasForeignKey("UpdaterId");
                });

            modelBuilder.Entity("FritzBot.Database.BoxEntry", b =>
                {
                    b.HasOne("FritzBot.Database.Box", "Box")
                        .WithMany()
                        .HasForeignKey("BoxId");

                    b.HasOne("FritzBot.Database.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("FritzBot.Database.BoxRegexPattern", b =>
                {
                    b.HasOne("FritzBot.Database.Box", "Box")
                        .WithMany("RegexPattern")
                        .HasForeignKey("BoxId");
                });

            modelBuilder.Entity("FritzBot.Database.Nickname", b =>
                {
                    b.HasOne("FritzBot.Database.User", "User")
                        .WithMany("Names")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("FritzBot.Database.ReminderEntry", b =>
                {
                    b.HasOne("FritzBot.Database.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");

                    b.HasOne("FritzBot.Database.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("FritzBot.Database.SeenEntry", b =>
                {
                    b.HasOne("FritzBot.Database.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("FritzBot.Database.ServerChannel", b =>
                {
                    b.HasOne("FritzBot.Database.Server", "Server")
                        .WithMany("Channels")
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("FritzBot.Database.Subscription", b =>
                {
                    b.HasOne("FritzBot.Database.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("FritzBot.Database.SubscriptionBedingung", b =>
                {
                    b.HasOne("FritzBot.Database.Subscription")
                        .WithMany("Bedingungen")
                        .HasForeignKey("SubscriptionId");
                });

            modelBuilder.Entity("FritzBot.Database.User", b =>
                {
                    b.HasOne("FritzBot.Database.Nickname", "LastUsedName")
                        .WithMany()
                        .HasForeignKey("LastUsedNameId");
                });

            modelBuilder.Entity("FritzBot.Database.UserKeyValueEntry", b =>
                {
                    b.HasOne("FritzBot.Database.User", "User")
                        .WithMany("UserStorage")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("FritzBot.Database.WitzEntry", b =>
                {
                    b.HasOne("FritzBot.Database.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");
                });
        }
    }
}
