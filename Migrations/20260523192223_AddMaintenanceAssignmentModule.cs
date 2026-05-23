using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceAssignmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    NextInspectionTypeId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)99)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTypes_AcTypes_AcTypeId",
                        column: x => x.AcTypeId,
                        principalSchema: "dbo",
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionTypes_InspectionTypes_NextInspectionTypeId",
                        column: x => x.NextInspectionTypeId,
                        principalSchema: "dbo",
                        principalTable: "InspectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRoles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)99)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMaintenanceAssignments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BaseId = table.Column<int>(type: "int", nullable: false),
                    AcMainGroupId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceRoleId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaintenanceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceAssignments_AcMainGroups_AcMainGroupId",
                        column: x => x.AcMainGroupId,
                        principalSchema: "dbo",
                        principalTable: "AcMainGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceAssignments_Bases_BaseId",
                        column: x => x.BaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceAssignments_MaintenanceRoles_MaintenanceRoleId",
                        column: x => x.MaintenanceRoleId,
                        principalSchema: "dbo",
                        principalTable: "MaintenanceRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMaintenanceAssignmentGroups",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    AcMainGroupId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaintenanceAssignmentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceAssignmentGroups_AcMainGroups_AcMainGroupId",
                        column: x => x.AcMainGroupId,
                        principalSchema: "dbo",
                        principalTable: "AcMainGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMaintenanceAssignmentGroups_UserMaintenanceAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "dbo",
                        principalTable: "UserMaintenanceAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "MaintenanceRoles",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "TECH", "Maintenance technician. Signs off own task cards.", true, "Technician", (byte)1 },
                    { 2, "BASE_SUP", "Supervises maintenance within one base and aircraft group scope.", true, "Base Supervisor", (byte)2 },
                    { 3, "MASTER_SUP", "Full read/write access across all bases and groups.", true, "Master Supervisor", (byte)3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTypes_NextInspectionTypeId",
                schema: "dbo",
                table: "InspectionTypes",
                column: "NextInspectionTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_InspectionType_AcType_Code",
                schema: "dbo",
                table: "InspectionTypes",
                columns: new[] { "AcTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MaintenanceRole_Code",
                schema: "dbo",
                table: "MaintenanceRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceAssignmentGroup_Assignment_Group",
                schema: "dbo",
                table: "UserMaintenanceAssignmentGroups",
                columns: new[] { "AssignmentId", "AcMainGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceAssignmentGroups_AcMainGroupId",
                schema: "dbo",
                table: "UserMaintenanceAssignmentGroups",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceAssignment_UserId_IsActive",
                schema: "dbo",
                table: "UserMaintenanceAssignments",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceAssignments_AcMainGroupId",
                schema: "dbo",
                table: "UserMaintenanceAssignments",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceAssignments_BaseId",
                schema: "dbo",
                table: "UserMaintenanceAssignments",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaintenanceAssignments_MaintenanceRoleId",
                schema: "dbo",
                table: "UserMaintenanceAssignments",
                column: "MaintenanceRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserMaintenanceAssignmentGroups",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserMaintenanceAssignments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MaintenanceRoles",
                schema: "dbo");
        }
    }
}
