using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BYUNotes.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddChartingNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColumnsJson",
                table: "Notes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowsJson",
                table: "Notes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnsJson",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "RowsJson",
                table: "Notes");
        }
    }
}
