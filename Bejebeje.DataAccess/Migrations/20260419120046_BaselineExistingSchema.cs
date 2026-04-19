using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bejebeje.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BaselineExistingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_imported",
                table: "lyrics",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "lyrics",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "verified_at",
                table: "lyrics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "youtube_link",
                table: "lyrics",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<char>(
                name: "sex",
                table: "artists",
                type: "character(1)",
                nullable: true,
                oldClrType: typeof(char),
                oldType: "character(1)");

            migrationBuilder.AddColumn<bool>(
                name: "is_group",
                table: "artists",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_imported",
                table: "artists",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "likes",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    lyric_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("primary_key", x => new { x.user_id, x.lyric_id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "likes");

            migrationBuilder.DropColumn(
                name: "is_imported",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "verified_at",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "youtube_link",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "is_group",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "is_imported",
                table: "artists");

            migrationBuilder.AlterColumn<char>(
                name: "sex",
                table: "artists",
                type: "character(1)",
                nullable: false,
                defaultValue: '\0',
                oldClrType: typeof(char),
                oldType: "character(1)",
                oldNullable: true);
        }
    }
}
