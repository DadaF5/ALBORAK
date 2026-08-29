using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AircraftReadings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    DimensionTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AircraftReadings_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AircraftReadings_ComponentLifeLimitDimensionTypes_DimensionTypeId",
                        column: x => x.DimensionTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitDimensionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftReadings_Aircraft_Dimension",
                schema: "dbo",
                table: "AircraftReadings",
                columns: new[] { "AircraftId", "DimensionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AircraftReadings_DimensionTypeId",
                schema: "dbo",
                table: "AircraftReadings",
                column: "DimensionTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftReadings",
                schema: "dbo");
        }
    }
}
