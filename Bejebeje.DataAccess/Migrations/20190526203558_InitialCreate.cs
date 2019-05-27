using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bejebeje.DataAccess.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    first_name = table.Column<string>(nullable: true),
                    last_name = table.Column<string>(nullable: true),
                    full_name = table.Column<string>(nullable: true),
                    is_approved = table.Column<bool>(nullable: false),
                    user_id = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: false),
                    is_deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "artist_images",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    data = table.Column<byte[]>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: true),
                    artist_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artist_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_artist_images_artists_artist_id",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "artist_slug",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(nullable: true),
                    is_primary = table.Column<bool>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: false),
                    is_deleted = table.Column<bool>(nullable: false),
                    artist_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artist_slug", x => x.id);
                    table.ForeignKey(
                        name: "fk_artist_slug_artists_artist_id",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    title = table.Column<string>(nullable: true),
                    body = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: false),
                    is_deleted = table.Column<bool>(nullable: false),
                    is_approved = table.Column<bool>(nullable: false),
                    artist_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_artists_artist_id",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyric_slugs",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(nullable: true),
                    is_primary = table.Column<bool>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: false),
                    is_deleted = table.Column<bool>(nullable: false),
                    lyric_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyric_slugs", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyric_slugs_lyrics_lyric_id",
                        column: x => x.lyric_id,
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_artist_images_artist_id",
                table: "artist_images",
                column: "artist_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_artist_slug_artist_id",
                table: "artist_slug",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyric_slugs_lyric_id",
                table: "lyric_slugs",
                column: "lyric_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_artist_id",
                table: "lyrics",
                column: "artist_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artist_images");

            migrationBuilder.DropTable(
                name: "artist_slug");

            migrationBuilder.DropTable(
                name: "lyric_slugs");

            migrationBuilder.DropTable(
                name: "lyrics");

            migrationBuilder.DropTable(
                name: "artists");
        }
    }
}
