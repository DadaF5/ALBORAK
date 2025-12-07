using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewMemberAndQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CrewMembers",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "QualificationNotes",
                table: "CrewMembers");

            migrationBuilder.RenameColumn(
                name: "Rank",
                table: "CrewMembers",
                newName: "Mobile");

            migrationBuilder.RenameColumn(
                name: "IsInstructor",
                table: "CrewMembers",
                newName: "AllowedToSign");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "CrewMembers",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "CrewMemberId",
                table: "CrewMembers",
                newName: "SquadronId");

            migrationBuilder.AlterColumn<int>(
                name: "SquadronId",
                table: "CrewMembers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CrewMembers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Captain",
                table: "CrewMembers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CrewMemberType",
                table: "CrewMembers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "CrewMembers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "CrewMembers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrimaryQualificationId",
                table: "CrewMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "CrewMembers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequenceNo",
                table: "CrewMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CrewMembers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CrewMembers",
                table: "CrewMembers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Qualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    QualificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrewMemberQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewMemberId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMemberQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewMemberQualifications_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrewMemberQualifications_Qualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_PrimaryQualificationId",
                table: "CrewMembers",
                column: "PrimaryQualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_SquadronId",
                table: "CrewMembers",
                column: "SquadronId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberQualifications_CrewMemberId_QualificationId",
                table: "CrewMemberQualifications",
                columns: new[] { "CrewMemberId", "QualificationId" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberQualifications_QualificationId",
                table: "CrewMemberQualifications",
                column: "QualificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                table: "CrewMembers",
                column: "PrimaryQualificationId",
                principalTable: "Qualifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrewMembers_Squadrons_SquadronId",
                table: "CrewMembers",
                column: "SquadronId",
                principalTable: "Squadrons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                table: "CrewMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CrewMembers_Squadrons_SquadronId",
                table: "CrewMembers");

            migrationBuilder.DropTable(
                name: "CrewMemberQualifications");

            migrationBuilder.DropTable(
                name: "Qualifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CrewMembers",
                table: "CrewMembers");

            migrationBuilder.DropIndex(
                name: "IX_CrewMembers_PrimaryQualificationId",
                table: "CrewMembers");

            migrationBuilder.DropIndex(
                name: "IX_CrewMembers_SquadronId",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "Captain",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "CrewMemberType",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "Photo",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "PrimaryQualificationId",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "SequenceNo",
                table: "CrewMembers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CrewMembers");

            migrationBuilder.RenameColumn(
                name: "SquadronId",
                table: "CrewMembers",
                newName: "CrewMemberId");

            migrationBuilder.RenameColumn(
                name: "Mobile",
                table: "CrewMembers",
                newName: "Rank");

            migrationBuilder.RenameColumn(
                name: "AllowedToSign",
                table: "CrewMembers",
                newName: "IsInstructor");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "CrewMembers",
                newName: "IsActive");

            migrationBuilder.AlterColumn<int>(
                name: "CrewMemberId",
                table: "CrewMembers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "CrewMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualificationNotes",
                table: "CrewMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CrewMembers",
                table: "CrewMembers",
                column: "CrewMemberId");
        }
    }
}
