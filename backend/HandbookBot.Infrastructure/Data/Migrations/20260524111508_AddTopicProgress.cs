using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HandbookBot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "topic_progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectStreak = table.Column<int>(type: "integer", nullable: false),
                    EaseFactor = table.Column<float>(type: "real", nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NextReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Mastery = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_progress", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_topic_progress_UserId_NextReviewAt",
                table: "topic_progress",
                columns: new[] { "UserId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_progress_UserId_TopicId",
                table: "topic_progress",
                columns: new[] { "UserId", "TopicId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "topic_progress");
        }
    }
}
