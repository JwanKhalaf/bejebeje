namespace Bejebeje.DataAccess.Migrations
{
  using System;
  using Microsoft.EntityFrameworkCore.Migrations;
  using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

  public partial class InitialCreate : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "artists",
          columns: table => new
          {
            id = table.Column<int>(nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            first_name = table.Column<string>(nullable: true),
            last_name = table.Column<string>(nullable: true),
            full_name = table.Column<string>(nullable: true),
            is_approved = table.Column<bool>(nullable: false),
            user_id = table.Column<string>(nullable: true),
            created_at = table.Column<DateTime>(nullable: false),
            modified_at = table.Column<DateTime>(nullable: true),
            is_deleted = table.Column<bool>(nullable: false),
            has_image = table.Column<bool>(nullable: false),
            sex = table.Column<int>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_artists", x => x.id);
          });

      migrationBuilder.CreateTable(
          name: "authors",
          columns: table => new
          {
            id = table.Column<int>(nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            first_name = table.Column<string>(nullable: true),
            last_name = table.Column<string>(nullable: true),
            full_name = table.Column<string>(nullable: true),
            biography = table.Column<string>(nullable: true),
            is_approved = table.Column<bool>(nullable: false),
            user_id = table.Column<string>(nullable: true),
            created_at = table.Column<DateTime>(nullable: false),
            modified_at = table.Column<DateTime>(nullable: true),
            is_deleted = table.Column<bool>(nullable: false),
            has_image = table.Column<bool>(nullable: false),
            sex = table.Column<int>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_authors", x => x.id);
          });

      migrationBuilder.CreateTable(
          name: "artist_slugs",
          columns: table => new
          {
            id = table.Column<int>(nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(nullable: true),
            is_primary = table.Column<bool>(nullable: false),
            created_at = table.Column<DateTime>(nullable: false),
            modified_at = table.Column<DateTime>(nullable: true),
            is_deleted = table.Column<bool>(nullable: false),
            artist_id = table.Column<int>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_artist_slugs", x => x.id);
            table.ForeignKey(
                      name: "fk_artist_slugs_artists_artist_id",
                      column: x => x.artist_id,
                      principalTable: "artists",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "author_slugs",
          columns: table => new
          {
            id = table.Column<int>(nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(nullable: true),
            is_primary = table.Column<bool>(nullable: false),
            created_at = table.Column<DateTime>(nullable: false),
            modified_at = table.Column<DateTime>(nullable: true),
            is_deleted = table.Column<bool>(nullable: false),
            author_id = table.Column<int>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_author_slugs", x => x.id);
            table.ForeignKey(
                      name: "fk_author_slugs_authors_author_id",
                      column: x => x.author_id,
                      principalTable: "authors",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "lyrics",
          columns: table => new
          {
            id = table.Column<int>(nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            title = table.Column<string>(nullable: true),
            body = table.Column<string>(nullable: true),
            user_id = table.Column<string>(nullable: true),
            created_at = table.Column<DateTime>(nullable: false),
            modified_at = table.Column<DateTime>(nullable: true),
            is_deleted = table.Column<bool>(nullable: false),
            is_approved = table.Column<bool>(nullable: false),
            artist_id = table.Column<int>(nullable: false),
            author_id = table.Column<int>(nullable: true)
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
            table.ForeignKey(
                      name: "fk_lyrics_authors_author_id",
                      column: x => x.author_id,
                      principalTable: "authors",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.CreateTable(
          name: "lyric_slugs",
          columns: table => new
          {
            id = table.Column<int>(nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(nullable: true),
            is_primary = table.Column<bool>(nullable: false),
            created_at = table.Column<DateTime>(nullable: false),
            modified_at = table.Column<DateTime>(nullable: true),
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
          name: "ix_artist_slugs_artist_id",
          table: "artist_slugs",
          column: "artist_id");

      migrationBuilder.CreateIndex(
          name: "ix_author_slugs_author_id",
          table: "author_slugs",
          column: "author_id");

      migrationBuilder.CreateIndex(
          name: "ix_lyric_slugs_lyric_id",
          table: "lyric_slugs",
          column: "lyric_id");

      migrationBuilder.CreateIndex(
          name: "ix_lyrics_artist_id",
          table: "lyrics",
          column: "artist_id");

      migrationBuilder.CreateIndex(
          name: "ix_lyrics_author_id",
          table: "lyrics",
          column: "author_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "artist_slugs");

      migrationBuilder.DropTable(
          name: "author_slugs");

      migrationBuilder.DropTable(
          name: "lyric_slugs");

      migrationBuilder.DropTable(
          name: "lyrics");

      migrationBuilder.DropTable(
          name: "artists");

      migrationBuilder.DropTable(
          name: "authors");
    }
  }
}
