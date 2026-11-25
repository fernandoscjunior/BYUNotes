using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BYUNotes.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMappingNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BranchesJson",
                table: "Notes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CentralTopic",
                table: "Notes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchesJson",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CentralTopic",
                table: "Notes");
        }
    }
}
