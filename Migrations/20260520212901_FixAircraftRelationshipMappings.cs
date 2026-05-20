using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class FixAircraftRelationshipMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aircrafts_AcStatusType_AcStatusTypeId",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircrafts_AcTypes_AcTypeId",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircrafts_Bases_BaseId",
                table: "Aircrafts");

            migrationBuilder.RenameTable(
                name: "AcTypes",
                schema: "dbo",
                newName: "AcTypes");

            migrationBuilder.RenameColumn(
                name: "MaxGrossweight",
                table: "AcTypes",
                newName: "MaxGrossWeight");

            migrationBuilder.AlterColumn<string>(
                name: "Obs",
                table: "Aircrafts",
                type: "nvarchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftVersionId",
                table: "Aircrafts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Aircrafts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Aircrafts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DossierId",
                table: "Aircrafts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Aircrafts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Aircrafts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManufacturerId",
                table: "Aircrafts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MissionRoleId",
                table: "Aircrafts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginCountryId",
                table: "Aircrafts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationDate",
                table: "Aircrafts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ServiceEntryDate",
                table: "Aircrafts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SortOrder",
                table: "Aircrafts",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)99);

            migrationBuilder.AddColumn<int>(
                name: "TotalCycles",
                table: "Aircrafts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalFlightMinutes",
                table: "Aircrafts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLandings",
                table: "Aircrafts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AcTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AcTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "AcTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_AircraftVersionId",
                table: "Aircrafts",
                column: "AircraftVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_DossierId",
                table: "Aircrafts",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_ManufacturerId",
                table: "Aircrafts",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_MissionRoleId",
                table: "Aircrafts",
                column: "MissionRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_OriginCountryId",
                table: "Aircrafts",
                column: "OriginCountryId");

            migrationBuilder.CreateIndex(
                name: "UX_Aircraft_Registration",
                table: "Aircrafts",
                column: "Registration",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Aircraft_TailNo",
                table: "Aircrafts",
                column: "TailNo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_AcStatusType",
                table: "Aircrafts",
                column: "AcStatusTypeId",
                principalTable: "AcStatusType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_AcType",
                table: "Aircrafts",
                column: "AcTypeId",
                principalTable: "AcTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_AircraftVersion",
                table: "Aircrafts",
                column: "AircraftVersionId",
                principalTable: "AircraftVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_Base",
                table: "Aircrafts",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_Dossier",
                table: "Aircrafts",
                column: "DossierId",
                principalTable: "ImmatriculationDossier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_ManufacturerFk",
                table: "Aircrafts",
                column: "ManufacturerId",
                principalTable: "AircraftManufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_MissionRole",
                table: "Aircrafts",
                column: "MissionRoleId",
                principalTable: "MissionRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_OriginCountry",
                table: "Aircrafts",
                column: "OriginCountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_AcStatusType",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_AcType",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_AircraftVersion",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_Base",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_Dossier",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_ManufacturerFk",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_MissionRole",
                table: "Aircrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_OriginCountry",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "IX_Aircrafts_AircraftVersionId",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "IX_Aircrafts_DossierId",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "IX_Aircrafts_ManufacturerId",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "IX_Aircrafts_MissionRoleId",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "IX_Aircrafts_OriginCountryId",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "UX_Aircraft_Registration",
                table: "Aircrafts");

            migrationBuilder.DropIndex(
                name: "UX_Aircraft_TailNo",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "AircraftVersionId",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "DossierId",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "MissionRoleId",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "OriginCountryId",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "ServiceEntryDate",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "TotalCycles",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "TotalFlightMinutes",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "TotalLandings",
                table: "Aircrafts");

            migrationBuilder.RenameTable(
                name: "AcTypes",
                newName: "AcTypes",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "MaxGrossWeight",
                schema: "dbo",
                table: "AcTypes",
                newName: "MaxGrossweight");

            migrationBuilder.AlterColumn<string>(
                name: "Obs",
                table: "Aircrafts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "AcTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "AcTypes",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "dbo",
                table: "AcTypes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Aircrafts_AcStatusType_AcStatusTypeId",
                table: "Aircrafts",
                column: "AcStatusTypeId",
                principalTable: "AcStatusType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircrafts_AcTypes_AcTypeId",
                table: "Aircrafts",
                column: "AcTypeId",
                principalSchema: "dbo",
                principalTable: "AcTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircrafts_Bases_BaseId",
                table: "Aircrafts",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "Id");
        }
    }
}
