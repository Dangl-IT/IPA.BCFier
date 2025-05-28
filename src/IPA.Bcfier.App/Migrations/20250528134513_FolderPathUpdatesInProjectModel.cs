using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPA.Bcfier.App.Migrations
{
    /// <inheritdoc />
    public partial class FolderPathUpdatesInProjectModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Projects",
                newName: "RevitFilePath");

            migrationBuilder.AddColumn<string>(
                name: "BcfFilesFolder",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BcfFilesFolder",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "RevitFilePath",
                table: "Projects",
                newName: "FilePath");
        }
    }
}
