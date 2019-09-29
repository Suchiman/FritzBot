using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FritzBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boxes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShortName = table.Column<string>(nullable: false),
                    FullName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationHistories",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Plugin = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Notification = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address = table.Column<string>(nullable: false),
                    Port = table.Column<int>(nullable: false),
                    Nickname = table.Column<string>(nullable: false),
                    QuitMessage = table.Column<string>(nullable: true),
                    NickServPassword = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoxRegexPattern",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Pattern = table.Column<string>(nullable: false),
                    BoxId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxRegexPattern", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoxRegexPattern_Boxes_BoxId",
                        column: x => x.BoxId,
                        principalTable: "Boxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerChannel",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    ServerId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerChannel_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoxEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(nullable: false),
                    BoxId = table.Column<long>(nullable: true),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoxEntries_Boxes_BoxId",
                        column: x => x.BoxId,
                        principalTable: "Boxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastUsedNameId = table.Column<long>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    Authentication = table.Column<DateTime>(nullable: true),
                    Ignored = table.Column<bool>(nullable: false),
                    Admin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AliasEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    CreatorId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: true),
                    UpdaterId = table.Column<long>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AliasEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AliasEntries_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AliasEntries_Users_UpdaterId",
                        column: x => x.UpdaterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Nicknames",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nicknames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nicknames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReminderEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatorId = table.Column<long>(nullable: false),
                    Message = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReminderEntries_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReminderEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeenEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastSeen = table.Column<DateTime>(nullable: true),
                    LastMessaged = table.Column<DateTime>(nullable: true),
                    LastMessage = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeenEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeenEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(nullable: false),
                    Plugin = table.Column<string>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserKeyValueEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserKeyValueEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserKeyValueEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WitzEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Witz = table.Column<string>(nullable: false),
                    Frequency = table.Column<int>(nullable: false),
                    CreatorId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WitzEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WitzEntries_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionBedingung",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bedingung = table.Column<string>(nullable: false),
                    SubscriptionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionBedingung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionBedingung_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AliasEntries_CreatorId",
                table: "AliasEntries",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AliasEntries_Key",
                table: "AliasEntries",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AliasEntries_UpdaterId",
                table: "AliasEntries",
                column: "UpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxEntries_BoxId",
                table: "BoxEntries",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxEntries_UserId",
                table: "BoxEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BoxRegexPattern_BoxId",
                table: "BoxRegexPattern",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Nicknames_Name",
                table: "Nicknames",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nicknames_UserId",
                table: "Nicknames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderEntries_CreatorId",
                table: "ReminderEntries",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderEntries_UserId",
                table: "ReminderEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeenEntries_UserId",
                table: "SeenEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerChannel_ServerId",
                table: "ServerChannel",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionBedingung_SubscriptionId",
                table: "SubscriptionBedingung",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserKeyValueEntries_UserId",
                table: "UserKeyValueEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastUsedNameId",
                table: "Users",
                column: "LastUsedNameId");

            migrationBuilder.CreateIndex(
                name: "IX_WitzEntries_CreatorId",
                table: "WitzEntries",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoxEntries_Users_UserId",
                table: "BoxEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Nicknames_LastUsedNameId",
                table: "Users",
                column: "LastUsedNameId",
                principalTable: "Nicknames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nicknames_Users_UserId",
                table: "Nicknames");

            migrationBuilder.DropTable(
                name: "AliasEntries");

            migrationBuilder.DropTable(
                name: "BoxEntries");

            migrationBuilder.DropTable(
                name: "BoxRegexPattern");

            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "ReminderEntries");

            migrationBuilder.DropTable(
                name: "SeenEntries");

            migrationBuilder.DropTable(
                name: "ServerChannel");

            migrationBuilder.DropTable(
                name: "SubscriptionBedingung");

            migrationBuilder.DropTable(
                name: "UserKeyValueEntries");

            migrationBuilder.DropTable(
                name: "WitzEntries");

            migrationBuilder.DropTable(
                name: "Boxes");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Nicknames");
        }
    }
}
