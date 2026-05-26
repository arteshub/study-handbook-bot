using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HandbookBot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScrollAndDiscard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cached_question_id",
                table: "test_results",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "discarded_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CachedQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discarded_questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reading_positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScrollRatio = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_positions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discarded_questions_UserId_CachedQuestionId",
                table: "discarded_questions",
                columns: new[] { "UserId", "CachedQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reading_positions_UserId_TopicId",
                table: "reading_positions",
                columns: new[] { "UserId", "TopicId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discarded_questions");

            migrationBuilder.DropTable(
                name: "reading_positions");

            migrationBuilder.DropColumn(
                name: "cached_question_id",
                table: "test_results");
        }
    }
}
