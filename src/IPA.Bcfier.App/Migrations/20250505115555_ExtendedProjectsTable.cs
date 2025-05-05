using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPA.Bcfier.App.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedProjectsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Projects");
        }
    }
}
