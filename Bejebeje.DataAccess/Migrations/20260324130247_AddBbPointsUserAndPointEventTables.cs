using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bejebeje.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBbPointsUserAndPointEventTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cognito_user_id = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    artist_submission_points = table.Column<int>(type: "integer", nullable: false),
                    artist_approval_points = table.Column<int>(type: "integer", nullable: false),
                    lyric_submission_points = table.Column<int>(type: "integer", nullable: false),
                    lyric_approval_points = table.Column<int>(type: "integer", nullable: false),
                    report_submission_points = table.Column<int>(type: "integer", nullable: false),
                    report_acknowledgement_points = table.Column<int>(type: "integer", nullable: false),
                    last_seen_points = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "point_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    entity_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_point_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_point_events_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_point_events_user_id_action_type_entity_id",
                table: "point_events",
                columns: new[] { "user_id", "action_type", "entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_point_events_user_id_created_at",
                table: "point_events",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_users_cognito_user_id",
                table: "users",
                column: "cognito_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "point_events");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
