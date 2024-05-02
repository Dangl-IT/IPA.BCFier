using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPA.Bcfier.App.Migrations
{
    /// <inheritdoc />
    public partial class LastOpenedFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "CreatedAtUtc",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "LastOpenedUserFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpenedAtAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastOpenedUserFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LastOpenedUserFiles_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LastOpenedUserFiles_ProjectId",
                table: "LastOpenedUserFiles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LastOpenedUserFiles_UserName",
                table: "LastOpenedUserFiles",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastOpenedUserFiles");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }
    }
}
