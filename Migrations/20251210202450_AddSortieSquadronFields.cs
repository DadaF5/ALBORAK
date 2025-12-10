using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSortieSquadronFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AirframeCycles",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AirframeHours",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppContacts",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Approachs",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BrakeChuteUsed",
                table: "Sorties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cycles",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DayHours",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAtUtc",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalizedBy",
                table: "Sorties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelUsedLiters",
                table: "Sorties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HobbsEnd",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HobbsStart",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HobbsUsed",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IFRHours",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InstActual",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InstSimulated",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Interceptions",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalized",
                table: "Sorties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Landings",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Malfunctions",
                table: "Sorties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NightHours",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RadarContacts",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RealLandingTime",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RealTOFF",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SquadronReportNotes",
                table: "Sorties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Sorties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TGOsLandings",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TachEnd",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TachStart",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TachUsed",
                table: "Sorties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreflightApproved",
                table: "Odvs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Odvs",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Aircrafts",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Aircrafts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirframeCycles",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "AirframeHours",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "AppContacts",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Approachs",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "BrakeChuteUsed",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Cycles",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "DayHours",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "FinalizedAtUtc",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "FinalizedBy",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "FuelUsedLiters",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "HobbsEnd",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "HobbsStart",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "HobbsUsed",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "IFRHours",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "InstActual",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "InstSimulated",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Interceptions",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "IsFinalized",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Landings",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Malfunctions",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "NightHours",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "RadarContacts",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "RealLandingTime",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "RealTOFF",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "SquadronReportNotes",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "TGOsLandings",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "TachEnd",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "TachStart",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "TachUsed",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "IsPreflightApproved",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Aircrafts");
        }
    }
}
