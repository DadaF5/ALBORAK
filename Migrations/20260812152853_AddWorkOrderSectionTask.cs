using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderSectionTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderSectionTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderSectionId = table.Column<int>(type: "int", nullable: false),
                    DesignationTravaux = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TempsAlloueMinutes = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    TempsPasseSystemeMinutes = table.Column<int>(type: "int", nullable: true),
                    TempsPasseRetouchesMinutes = table.Column<int>(type: "int", nullable: true),
                    ExecutantSpecial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutantNom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutantSignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderSectionTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderSectionTasks_WorkOrderSections_WorkOrderSectionId",
                        column: x => x.WorkOrderSectionId,
                        principalTable: "WorkOrderSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSectionTasks_WorkOrderSectionId",
                table: "WorkOrderSectionTasks",
                column: "WorkOrderSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderSectionTasks");
        }
    }
}
