using Microsoft.EntityFrameworkCore.Migrations;

namespace SchedulerBot.Persistence.Migrations
{
    public partial class AddEventMentionsToContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventMention_Events_EventId",
                table: "EventMention");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventMention",
                table: "EventMention");

            migrationBuilder.RenameTable(
                name: "EventMention",
                newName: "EventMentions");

            migrationBuilder.RenameIndex(
                name: "IX_EventMention_EventId",
                table: "EventMentions",
                newName: "IX_EventMentions_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventMentions",
                table: "EventMentions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventMentions_Events_EventId",
                table: "EventMentions",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventMentions_Events_EventId",
                table: "EventMentions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventMentions",
                table: "EventMentions");

            migrationBuilder.RenameTable(
                name: "EventMentions",
                newName: "EventMention");

            migrationBuilder.RenameIndex(
                name: "IX_EventMentions_EventId",
                table: "EventMention",
                newName: "IX_EventMention_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventMention",
                table: "EventMention",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventMention_Events_EventId",
                table: "EventMention",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
