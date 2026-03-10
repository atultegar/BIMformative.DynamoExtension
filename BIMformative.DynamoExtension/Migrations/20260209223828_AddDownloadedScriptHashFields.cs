using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIMformative.DynamoExtension.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadedScriptHashFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentLocalHash",
                table: "DownloadedScripts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DownloadedHash",
                table: "DownloadedScripts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLocalHash",
                table: "DownloadedScripts");

            migrationBuilder.DropColumn(
                name: "DownloadedHash",
                table: "DownloadedScripts");
        }
    }
}
