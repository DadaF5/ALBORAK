using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BadgeNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeBaseId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)99)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullOfficialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LMAMNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LMAMExpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    Section = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RoleCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CanWrite = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SignOffLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShowBaseScope = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowGroupScope = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowWingScope = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)99)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleRoles_Modules",
                        column: x => x.ModuleCode,
                        principalTable: "Modules",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Code", "Description", "IconClass", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { "HEALTHCARE", "Suivi médical du personnel navigant", "fas fa-heartbeat", true, "Service Médical", (byte)30 },
                    { "HR", "Gestion du personnel", "fas fa-users", true, "Ressources Humaines", (byte)20 },
                    { "MAINTENANCE", "Maintenance planifiée et corrective", "fas fa-wrench", true, "Maintenance Aéronefs", (byte)10 },
                    { "SETTINGS", "Paramétrage de la plateforme", "fas fa-cog", true, "Administration Système", (byte)99 },
                    { "SQUADRONOPS", "Planification et suivi des sorties", "fas fa-plane-departure", true, "Opérations Escadron", (byte)40 }
                });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "ShowGroupScope", "SignOffLevel", "SortOrder" },
                values: new object[,]
                {
                    { 1, true, null, true, "MAINTENANCE", "TECHNICIAN", "Technicien", true, true, "TECHNICIAN", (byte)10 },
                    { 2, true, null, true, "MAINTENANCE", "APRS", "Inspecteur APRS", true, true, "APRS", (byte)20 },
                    { 3, true, null, true, "MAINTENANCE", "NAVIGABILITY_OFFICER", "Officier de Navigabilité", true, true, "NAVIGABILITY", (byte)30 }
                });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 4, true, null, true, "MAINTENANCE", "COMMANDER", "Commandant", true, "COMMANDER", (byte)40 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 5, null, true, "MAINTENANCE", "BASE_SUPERVISOR", "Superviseur de Base", true, null, (byte)50 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "SignOffLevel", "SortOrder" },
                values: new object[] { 6, null, true, "MAINTENANCE", "MASTER_SUPERVISOR", "Superviseur Central", null, (byte)60 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 7, true, null, true, "HR", "HR_OFFICER", "Officier RH", true, null, (byte)10 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "SignOffLevel", "SortOrder" },
                values: new object[] { 8, true, null, true, "HR", "HR_MANAGER", "Chef du personnel", null, (byte)20 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 9, null, true, "HR", "HR_READONLY", "Consultation RH", true, null, (byte)30 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[,]
                {
                    { 10, true, null, true, "HEALTHCARE", "DOCTOR", "Médecin", true, null, (byte)10 },
                    { 11, true, null, true, "HEALTHCARE", "NURSE", "Infirmier", true, null, (byte)20 }
                });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 12, null, true, "HEALTHCARE", "MEDICAL_ADMIN", "Admin médical", true, null, (byte)30 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "ShowGroupScope", "ShowWingScope", "SignOffLevel", "SortOrder" },
                values: new object[,]
                {
                    { 13, true, null, true, "SQUADRONOPS", "PILOT", "Pilote", true, true, true, null, (byte)10 },
                    { 14, true, null, true, "SQUADRONOPS", "INSTRUCTOR", "Instructeur de vol", true, true, true, null, (byte)20 },
                    { 15, true, null, true, "SQUADRONOPS", "OPS_SCHEDULER", "Planificateur OPS", true, true, true, null, (byte)30 }
                });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "CanWrite", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 16, true, null, true, "SQUADRONOPS", "OPS_OFFICER", "Officier OPS", true, null, (byte)40 });

            migrationBuilder.InsertData(
                table: "ModuleRoles",
                columns: new[] { "Id", "Description", "IsActive", "ModuleCode", "RoleCode", "RoleName", "ShowBaseScope", "SignOffLevel", "SortOrder" },
                values: new object[] { 17, null, true, "SQUADRONOPS", "OPS_COMMANDER", "Commandant OPS", true, null, (byte)50 });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleRoles_Module_RoleCode",
                table: "ModuleRoles",
                columns: new[] { "ModuleCode", "RoleCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleRoles");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropColumn(
                name: "BadgeNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HomeBaseId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "AspNetUsers");
        }
    }
}
