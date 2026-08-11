using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderSectionPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderSectionParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderSectionId = table.Column<int>(type: "int", nullable: false),
                    OldNomenclature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldNumero = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldVieillissement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewNomenclature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewNumero = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewVieillissement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesignationEtPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotifDepose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Symbole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TempsAlloueMinutes = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    TempsPasseMinutes = table.Column<int>(type: "int", nullable: true),
                    ExecutantSpecial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutantNom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutantSignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderSectionParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderSectionParts_WorkOrderSections_WorkOrderSectionId",
                        column: x => x.WorkOrderSectionId,
                        principalTable: "WorkOrderSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSectionParts_WorkOrderSectionId",
                table: "WorkOrderSectionParts",
                column: "WorkOrderSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderSectionParts");
        }
    }
}
