using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSquadronOpsCurrentBaseAndCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ATA_AtaCategories_AtaCategoryId",
                table: "ATA");

            migrationBuilder.AddColumn<int>(
                name: "CurrentBaseId",
                table: "Squadrons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BlockDurationMinutes",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Sorties",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "Sorties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Odvs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Odvs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Squadrons_CurrentBaseId",
                table: "Squadrons",
                column: "CurrentBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ATA_AtaCategories_AtaCategoryId",
                table: "ATA",
                column: "AtaCategoryId",
                principalTable: "AtaCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Squadrons_Bases_CurrentBaseId",
                table: "Squadrons",
                column: "CurrentBaseId",
                principalTable: "Bases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ATA_AtaCategories_AtaCategoryId",
                table: "ATA");

            migrationBuilder.DropForeignKey(
                name: "FK_Squadrons_Bases_CurrentBaseId",
                table: "Squadrons");

            migrationBuilder.DropIndex(
                name: "IX_Squadrons_CurrentBaseId",
                table: "Squadrons");

            migrationBuilder.DropColumn(
                name: "CurrentBaseId",
                table: "Squadrons");

            migrationBuilder.DropColumn(
                name: "BlockDurationMinutes",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Odvs");

            migrationBuilder.AddForeignKey(
                name: "FK_ATA_AtaCategories_AtaCategoryId",
                table: "ATA",
                column: "AtaCategoryId",
                principalTable: "AtaCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
