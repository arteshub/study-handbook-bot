using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HandbookBot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionsJsonToTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                table: "test_results",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionsJson",
                table: "test_results");
        }
    }
}
