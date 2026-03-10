using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIMformative.DynamoExtension.Migrations
{
    /// <inheritdoc />
    public partial class SyncStatusColumnAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasUpdate",
                table: "DownloadedScripts");

            migrationBuilder.RenameColumn(
                name: "IsModifiedLocally",
                table: "DownloadedScripts",
                newName: "SyncStatus");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocalFileWriteTime",
                table: "DownloadedScripts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLocalFileWriteTime",
                table: "DownloadedScripts");

            migrationBuilder.RenameColumn(
                name: "SyncStatus",
                table: "DownloadedScripts",
                newName: "IsModifiedLocally");

            migrationBuilder.AddColumn<bool>(
                name: "HasUpdate",
                table: "DownloadedScripts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
