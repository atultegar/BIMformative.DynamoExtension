using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIMformative.DynamoExtension.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadedScripts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ScriptType = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadedVersion = table.Column<string>(type: "TEXT", nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsModifiedLocally = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<string>(type: "TEXT", nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadedScripts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadedScripts");
        }
    }
}
