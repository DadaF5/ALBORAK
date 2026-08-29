using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSortieEngineAndBlockTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlockOffTime",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockOnTime",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EngineStartTime",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EngineStopTime",
                table: "Sorties",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockOffTime",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "BlockOnTime",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "EngineStartTime",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "EngineStopTime",
                table: "Sorties");
        }
    }
}
