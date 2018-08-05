using Microsoft.EntityFrameworkCore.Migrations;

namespace FritzBot.Migrations
{
    public partial class NickServPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NickServPassword",
                table: "Servers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NickServPassword",
                table: "Servers");
        }
    }
}
