using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class InitOdvSorties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ODV_AcMainGroups_AcMainGroupID",
                table: "ODV");

            migrationBuilder.DropForeignKey(
                name: "FK_ODV_Missions_MissionId",
                table: "ODV");

            migrationBuilder.DropForeignKey(
                name: "FK_ODV_Squadrons_SquadronID",
                table: "ODV");

            migrationBuilder.DropForeignKey(
                name: "FK_SortieCrews_Persons_PersonId",
                table: "SortieCrews");

            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties");

            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_ODV_OdvID",
                table: "Sorties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ODV",
                table: "ODV");

            migrationBuilder.DropIndex(
                name: "IX_ODV_AcMainGroupID_OdvDate",
                table: "ODV");

            migrationBuilder.DropIndex(
                name: "IX_ODV_MissionId_OdvDate",
                table: "ODV");

            migrationBuilder.DropIndex(
                name: "IX_ODV_SquadronID_OdvDate",
                table: "ODV");

            migrationBuilder.DropColumn(
                name: "CallSignId",
                table: "ODV");

            migrationBuilder.DropColumn(
                name: "ZoneID",
                table: "ODV");

            migrationBuilder.RenameTable(
                name: "ODV",
                newName: "Odvs");

            migrationBuilder.RenameColumn(
                name: "OdvID",
                table: "Sorties",
                newName: "OdvId");

            migrationBuilder.RenameColumn(
                name: "SortieId",
                table: "Sorties",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Sorties_OdvID",
                table: "Sorties",
                newName: "IX_Sorties_OdvId");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "SortieCrews",
                newName: "CrewMemberId");

            migrationBuilder.RenameColumn(
                name: "SortieCrewId",
                table: "SortieCrews",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_SortieCrews_PersonId",
                table: "SortieCrews",
                newName: "IX_SortieCrews_CrewMemberId");

            migrationBuilder.RenameColumn(
                name: "SquadronID",
                table: "Odvs",
                newName: "SquadronId");

            migrationBuilder.RenameColumn(
                name: "AcMainGroupID",
                table: "Odvs",
                newName: "AcMainGroupId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Odvs",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "MissionTypeID",
                table: "Odvs",
                newName: "Zone");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Odvs",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "OdvID",
                table: "Odvs",
                newName: "Id");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "Sorties",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "AircraftId1",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "SortieCrews",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "SortieCrews",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "SortieCrews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TOFF",
                table: "Odvs",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(7)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OdvStatus",
                table: "Odvs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "Planned");

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "Odvs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "AcMainGroupId1",
                table: "Odvs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallSign",
                table: "Odvs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissionType",
                table: "Odvs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Odvs",
                table: "Odvs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Sorties_AircraftId1",
                table: "Sorties",
                column: "AircraftId1");

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_AcMainGroupId",
                table: "Odvs",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_AcMainGroupId1",
                table: "Odvs",
                column: "AcMainGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_MissionId",
                table: "Odvs",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_SquadronId",
                table: "Odvs",
                column: "SquadronId");

            migrationBuilder.AddForeignKey(
                name: "FK_Odvs_AcMainGroups_AcMainGroupId",
                table: "Odvs",
                column: "AcMainGroupId",
                principalTable: "AcMainGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Odvs_AcMainGroups_AcMainGroupId1",
                table: "Odvs",
                column: "AcMainGroupId1",
                principalTable: "AcMainGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Odvs_Missions_MissionId",
                table: "Odvs",
                column: "MissionId",
                principalTable: "Missions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Odvs_Squadrons_SquadronId",
                table: "Odvs",
                column: "SquadronId",
                principalTable: "Squadrons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SortieCrews_CrewMembers_CrewMemberId",
                table: "SortieCrews",
                column: "CrewMemberId",
                principalTable: "CrewMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_Odvs_OdvId",
                table: "Sorties",
                column: "OdvId",
                principalTable: "Odvs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Odvs_AcMainGroups_AcMainGroupId",
                table: "Odvs");

            migrationBuilder.DropForeignKey(
                name: "FK_Odvs_AcMainGroups_AcMainGroupId1",
                table: "Odvs");

            migrationBuilder.DropForeignKey(
                name: "FK_Odvs_Missions_MissionId",
                table: "Odvs");

            migrationBuilder.DropForeignKey(
                name: "FK_Odvs_Squadrons_SquadronId",
                table: "Odvs");

            migrationBuilder.DropForeignKey(
                name: "FK_SortieCrews_CrewMembers_CrewMemberId",
                table: "SortieCrews");

            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties");

            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId1",
                table: "Sorties");

            migrationBuilder.DropForeignKey(
                name: "FK_Sorties_Odvs_OdvId",
                table: "Sorties");

            migrationBuilder.DropIndex(
                name: "IX_Sorties_AircraftId1",
                table: "Sorties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Odvs",
                table: "Odvs");

            migrationBuilder.DropIndex(
                name: "IX_Odvs_AcMainGroupId",
                table: "Odvs");

            migrationBuilder.DropIndex(
                name: "IX_Odvs_AcMainGroupId1",
                table: "Odvs");

            migrationBuilder.DropIndex(
                name: "IX_Odvs_MissionId",
                table: "Odvs");

            migrationBuilder.DropIndex(
                name: "IX_Odvs_SquadronId",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "AircraftId1",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SortieCrews");

            migrationBuilder.DropColumn(
                name: "AcMainGroupId1",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "CallSign",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "MissionType",
                table: "Odvs");

            migrationBuilder.RenameTable(
                name: "Odvs",
                newName: "ODV");

            migrationBuilder.RenameColumn(
                name: "OdvId",
                table: "Sorties",
                newName: "OdvID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sorties",
                newName: "SortieId");

            migrationBuilder.RenameIndex(
                name: "IX_Sorties_OdvId",
                table: "Sorties",
                newName: "IX_Sorties_OdvID");

            migrationBuilder.RenameColumn(
                name: "CrewMemberId",
                table: "SortieCrews",
                newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SortieCrews",
                newName: "SortieCrewId");

            migrationBuilder.RenameIndex(
                name: "IX_SortieCrews_CrewMemberId",
                table: "SortieCrews",
                newName: "IX_SortieCrews_PersonId");

            migrationBuilder.RenameColumn(
                name: "SquadronId",
                table: "ODV",
                newName: "SquadronID");

            migrationBuilder.RenameColumn(
                name: "AcMainGroupId",
                table: "ODV",
                newName: "AcMainGroupID");

            migrationBuilder.RenameColumn(
                name: "Zone",
                table: "ODV",
                newName: "MissionTypeID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "ODV",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "ODV",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ODV",
                newName: "OdvID");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "Sorties",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "SortieCrews",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "SortieCrews",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TOFF",
                table: "ODV",
                type: "time(7)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OdvStatus",
                table: "ODV",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Planned",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "ODV",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CallSignId",
                table: "ODV",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneID",
                table: "ODV",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ODV",
                table: "ODV",
                column: "OdvID");

            migrationBuilder.CreateIndex(
                name: "IX_ODV_AcMainGroupID_OdvDate",
                table: "ODV",
                columns: new[] { "AcMainGroupID", "OdvDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ODV_MissionId_OdvDate",
                table: "ODV",
                columns: new[] { "MissionId", "OdvDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ODV_SquadronID_OdvDate",
                table: "ODV",
                columns: new[] { "SquadronID", "OdvDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_ODV_AcMainGroups_AcMainGroupID",
                table: "ODV",
                column: "AcMainGroupID",
                principalTable: "AcMainGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ODV_Missions_MissionId",
                table: "ODV",
                column: "MissionId",
                principalTable: "Missions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ODV_Squadrons_SquadronID",
                table: "ODV",
                column: "SquadronID",
                principalTable: "Squadrons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SortieCrews_Persons_PersonId",
                table: "SortieCrews",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_Aircrafts_AircraftId",
                table: "Sorties",
                column: "AircraftId",
                principalTable: "Aircrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sorties_ODV_OdvID",
                table: "Sorties",
                column: "OdvID",
                principalTable: "ODV",
                principalColumn: "OdvID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
