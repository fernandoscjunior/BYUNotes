using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BYUNotes.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSentenceNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentencesJson",
                table: "Notes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentencesJson",
                table: "Notes");
        }
    }
}
