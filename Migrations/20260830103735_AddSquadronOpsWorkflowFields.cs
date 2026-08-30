using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSquadronOpsWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostFlightNotes",
                table: "Sorties",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PostFlightOilUsedLiters",
                table: "Sorties",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostFlightReportedAtUtc",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostFlightReportedBy",
                table: "Sorties",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SnagId",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OilUsedLiters",
                table: "FlightLogs",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sorties_SnagId",
                table: "Sorties",
                column: "SnagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_Snags_SnagId",
                table: "Sorties",
                column: "SnagId",
                principalSchema: "dbo",
                principalTable: "Snags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Snags_SnagId",
                table: "Sorties");

            migrationBuilder.DropIndex(
                name: "IX_Sorties_SnagId",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "PostFlightNotes",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "PostFlightOilUsedLiters",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "PostFlightReportedAtUtc",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "PostFlightReportedBy",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "SnagId",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "OilUsedLiters",
                table: "FlightLogs");
        }
    }
}
