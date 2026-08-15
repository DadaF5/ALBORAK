using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAssignments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleRoleId = table.Column<int>(type: "int", nullable: true),
                    BaseId = table.Column<int>(type: "int", nullable: false),
                    IsBaseAdmin = table.Column<bool>(type: "bit", nullable: false),
                    AcMainGroupId = table.Column<int>(type: "int", nullable: true),
                    WingId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokeReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAssignments_AcMainGroups_AcMainGroupId",
                        column: x => x.AcMainGroupId,
                        principalTable: "AcMainGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignments_Bases_BaseId",
                        column: x => x.BaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignments_ModuleRoles_ModuleRoleId",
                        column: x => x.ModuleRoleId,
                        principalTable: "ModuleRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignments_Wings_WingId",
                        column: x => x.WingId,
                        principalTable: "Wings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_AcMainGroupId",
                schema: "dbo",
                table: "UserAssignments",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_BaseId",
                schema: "dbo",
                table: "UserAssignments",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_ModuleRoleId",
                schema: "dbo",
                table: "UserAssignments",
                column: "ModuleRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_UserId",
                schema: "dbo",
                table: "UserAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_WingId",
                schema: "dbo",
                table: "UserAssignments",
                column: "WingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAssignments",
                schema: "dbo");
        }
    }
}
