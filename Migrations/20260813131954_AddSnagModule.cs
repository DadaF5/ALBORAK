using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSnagModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Snags",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SnagNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    AtaId = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Impact = table.Column<int>(type: "int", nullable: false),
                    ReportedBy = table.Column<int>(type: "int", nullable: false),
                    DiscoveryPhase = table.Column<int>(type: "int", nullable: false),
                    DiscoveredDuringWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    DiscoveryFH = table.Column<int>(type: "int", nullable: false),
                    DiscoveryCycles = table.Column<int>(type: "int", nullable: true),
                    DiscoveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DiscoveryBaseId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsDeferred = table.Column<bool>(type: "bit", nullable: false),
                    DeferralReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeferralAuthorizedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeferralAuthorizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeferralLimitFH = table.Column<int>(type: "int", nullable: true),
                    DeferralLimitCycles = table.Column<int>(type: "int", nullable: true),
                    DeferralLimitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LinkedWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Snags_ATA_AtaId",
                        column: x => x.AtaId,
                        principalTable: "ATA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snags_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snags_Bases_DiscoveryBaseId",
                        column: x => x.DiscoveryBaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snags_WorkOrders_DiscoveredDuringWorkOrderId",
                        column: x => x.DiscoveredDuringWorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snags_WorkOrders_LinkedWorkOrderId",
                        column: x => x.LinkedWorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderSnags",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    SnagId = table.Column<int>(type: "int", nullable: false),
                    ResolvedOnClose = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderSnags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderSnags_Snags_SnagId",
                        column: x => x.SnagId,
                        principalSchema: "dbo",
                        principalTable: "Snags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderSnags_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Snags_AircraftId",
                schema: "dbo",
                table: "Snags",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_Snags_AtaId",
                schema: "dbo",
                table: "Snags",
                column: "AtaId");

            migrationBuilder.CreateIndex(
                name: "IX_Snags_DiscoveredDuringWorkOrderId",
                schema: "dbo",
                table: "Snags",
                column: "DiscoveredDuringWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Snags_DiscoveryBaseId",
                schema: "dbo",
                table: "Snags",
                column: "DiscoveryBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Snags_LinkedWorkOrderId",
                schema: "dbo",
                table: "Snags",
                column: "LinkedWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSnags_SnagId",
                schema: "dbo",
                table: "WorkOrderSnags",
                column: "SnagId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSnags_WorkOrderId",
                schema: "dbo",
                table: "WorkOrderSnags",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderSnags",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Snags",
                schema: "dbo");
        }
    }
}
