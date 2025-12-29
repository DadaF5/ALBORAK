using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class MedicalCheck_and_Bilan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalChecks",
                columns: table => new
                {
                    MedCheckID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewMemberId = table.Column<int>(type: "int", nullable: false),
                    MedCheckType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaysValid = table.Column<int>(type: "int", nullable: true),
                    Obs = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Speciality = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Constatations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OBESITE = table.Column<bool>(type: "bit", nullable: true),
                    C_Optique = table.Column<bool>(type: "bit", nullable: true),
                    Aptitude = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Next_VU_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VU_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LateCheckReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CaptainType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    vu_LateCheckReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalChecks", x => x.MedCheckID);
                    table.ForeignKey(
                        name: "FK_MedicalChecks_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalBilans",
                columns: table => new
                {
                    BilanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalCheckId = table.Column<int>(type: "int", nullable: false),
                    BilanType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BilanDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalBilans", x => x.BilanID);
                    table.ForeignKey(
                        name: "FK_MedicalBilans_MedicalChecks_MedicalCheckId",
                        column: x => x.MedicalCheckId,
                        principalTable: "MedicalChecks",
                        principalColumn: "MedCheckID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalBilans_MedicalCheckId",
                table: "MedicalBilans",
                column: "MedicalCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalChecks_CrewMemberId",
                table: "MedicalChecks",
                column: "CrewMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalBilans");

            migrationBuilder.DropTable(
                name: "MedicalChecks");
        }
    }
}
