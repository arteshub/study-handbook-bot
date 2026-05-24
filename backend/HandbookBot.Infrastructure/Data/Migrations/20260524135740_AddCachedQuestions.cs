using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HandbookBot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cached_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    ModelAnswer = table.Column<string>(type: "text", nullable: true),
                    OptionsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cached_questions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cached_questions_TopicId_Mode_ContentHash",
                table: "cached_questions",
                columns: new[] { "TopicId", "Mode", "ContentHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cached_questions");
        }
    }
}
