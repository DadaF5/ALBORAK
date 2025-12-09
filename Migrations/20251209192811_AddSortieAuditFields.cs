using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSortieAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties");

            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId1",
                table: "Sorties");

            migrationBuilder.DropIndex(
                name: "IX_Sorties_AircraftId1",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "AircraftId1",
                table: "Sorties");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "Sorties",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CompletedBy",
                table: "Sorties",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Sorties",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Sorties",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Sorties",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Sorties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Sorties",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sorties_IsCompleted",
                table: "Sorties",
                column: "IsCompleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties",
                column: "AircraftId",
                principalTable: "Aircrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties");

            migrationBuilder.DropIndex(
                name: "IX_Sorties_IsCompleted",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Sorties");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "Sorties",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CompletedBy",
                table: "Sorties",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftId1",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sorties_AircraftId1",
                table: "Sorties",
                column: "AircraftId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties",
                column: "AircraftId",
                principalTable: "Aircrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId1",
                table: "Sorties",
                column: "AircraftId1",
                principalTable: "Aircrafts",
                principalColumn: "Id");
        }
    }
}
