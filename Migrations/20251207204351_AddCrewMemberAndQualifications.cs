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
            // Drop dependent child table first (if exists)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CrewMemberQualifications', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CrewMemberQualifications;
END
");

            // Drop CrewMembers (if exists)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CrewMembers', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CrewMembers;
END
");

            // Drop Qualifications (if exists)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Qualifications', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Qualifications;
END
");

            // Create Qualifications
            migrationBuilder.CreateTable(
                name: "Qualifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    QualificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Other"),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualifications", x => x.Id);
                });

            // Create CrewMembers with Id as IDENTITY PK
            migrationBuilder.CreateTable(
                name: "CrewMembers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SequenceNo = table.Column<int>(nullable: true),
                    Captain = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Ready"),
                    AllowedToSign = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CrewMemberType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pilot"),
                    SquadronId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PrimaryQualificationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMembers", x => x.Id);

                    // FK to Persons table (principal table "Persons" PK "Id")
                    table.ForeignKey(
                        name: "FK_CrewMembers_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    // FK to Squadrons table
                    table.ForeignKey(
                        name: "FK_CrewMembers_Squadrons_SquadronId",
                        column: x => x.SquadronId,
                        principalTable: "Squadrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    // FK to Qualifications for primary qualification
                    table.ForeignKey(
                        name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                        column: x => x.PrimaryQualificationId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_PersonId",
                table: "CrewMembers",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_SquadronId",
                table: "CrewMembers",
                column: "SquadronId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_PrimaryQualificationId",
                table: "CrewMembers",
                column: "PrimaryQualificationId");

            // Create CrewMemberQualifications
            migrationBuilder.CreateTable(
                name: "CrewMemberQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
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
                name: "IX_CrewMemberQualifications_CrewMemberId",
                table: "CrewMemberQualifications",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberQualifications_QualificationId",
                table: "CrewMemberQualifications",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberQualifications_CrewMember_Qualification",
                table: "CrewMemberQualifications",
                columns: new[] { "CrewMemberId", "QualificationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewMemberQualifications");

            migrationBuilder.DropTable(
                name: "CrewMembers");

            migrationBuilder.DropTable(
                name: "Qualifications");
        }
    }
}
