using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bejebeje.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserSlugNotNullAndUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "slug",
                table: "users",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "slug",
                table: "users",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);
        }
    }
}
