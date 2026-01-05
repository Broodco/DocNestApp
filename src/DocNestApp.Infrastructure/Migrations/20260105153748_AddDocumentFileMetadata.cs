using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocNestApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "documents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileKey",
                table: "documents",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "documents",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "documents",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "FileKey",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "documents");
        }
    }
}
