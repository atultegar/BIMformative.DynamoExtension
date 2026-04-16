using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BIMformative.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadedScripts",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    ScriptType = table.Column<string>(nullable: false),
                    LocalPath = table.Column<string>(nullable: false),
                    DownloadedVersion = table.Column<string>(nullable: false),
                    LatestVersion = table.Column<string>(nullable: true),
                    DownloadedHash = table.Column<string>(nullable: true),
                    CurrentLocalHash = table.Column<string>(nullable: true),
                    SyncStatus = table.Column<int>(nullable: false),
                    DownloadedAt = table.Column<DateTime>(nullable: false),
                    LastCheckedAt = table.Column<DateTime>(nullable: true),
                    LastLocalFileWriteTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadedScripts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadedScripts");
        }
    }
}
