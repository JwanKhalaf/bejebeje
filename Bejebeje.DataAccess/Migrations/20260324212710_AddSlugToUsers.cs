using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bejebeje.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_slug",
                table: "users",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_slug",
                table: "users");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "users");
        }
    }
}
