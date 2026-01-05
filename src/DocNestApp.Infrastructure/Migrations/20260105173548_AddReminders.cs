using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocNestApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: false),
                    DaysBefore = table.Column<int>(type: "integer", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DispatchedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reminders_DispatchedAtUtc",
                table: "reminders",
                column: "DispatchedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_DocumentId_DaysBefore",
                table: "reminders",
                columns: new[] { "DocumentId", "DaysBefore" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reminders_UserId_DueAtUtc",
                table: "reminders",
                columns: new[] { "UserId", "DueAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reminders");
        }
    }
}
