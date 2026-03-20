using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITrade.DB.Migrations
{
    /// <inheritdoc />
    public partial class SeventhMigration_UserMatchingPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserMatchingPreferences",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TagMatchMaxPercentage = table.Column<int>(type: "integer", nullable: false),
                    ExperienceMaxPercentage = table.Column<int>(type: "integer", nullable: false),
                    ReviewsMaxPercentage = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMatchingPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserMatchingPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMatchingPreferences");
        }
    }
}
