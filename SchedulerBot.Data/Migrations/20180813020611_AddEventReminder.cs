using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SchedulerBot.Data.Migrations
{
    public partial class AddEventReminder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReminderTimestamp",
                table: "Events",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderTimestamp",
                table: "Events");
        }
    }
}
