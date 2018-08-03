using Microsoft.EntityFrameworkCore.Migrations;

namespace SchedulerBot.Data.Migrations
{
    public partial class AddPermissionNodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Node",
                table: "Permissions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Node",
                table: "Permissions");
        }
    }
}
