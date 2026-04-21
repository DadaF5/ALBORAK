using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoCoordinatesToBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseNameLocal",
                table: "Bases");

            migrationBuilder.AddColumn<string>(
                name: "BaseCode",
                table: "Bases",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Bases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Bases",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Bases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Bases",
                type: "decimal(10,7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseCode",
                table: "Bases");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Bases");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Bases");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Bases");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Bases");

            migrationBuilder.AddColumn<string>(
                name: "BaseNameLocal",
                table: "Bases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
