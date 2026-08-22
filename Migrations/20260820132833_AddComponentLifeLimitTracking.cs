using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentLifeLimitTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentPositions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AtaId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentPositions_ATA_AtaId",
                        column: x => x.AtaId,
                        principalTable: "ATA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentPositions_AcTypes_AcTypeId",
                        column: x => x.AcTypeId,
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nomenclature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AtaId = table.Column<int>(type: "int", nullable: true),
                    AircraftManufacturerId = table.Column<int>(type: "int", nullable: true),
                    TrackingMethod = table.Column<int>(type: "int", nullable: false),
                    IsSerialized = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentTypes_ATA_AtaId",
                        column: x => x.AtaId,
                        principalTable: "ATA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentTypes_AircraftManufacturers_AircraftManufacturerId",
                        column: x => x.AircraftManufacturerId,
                        principalTable: "AircraftManufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentLifeLimitProfiles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    ApplicabilityRuleType = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SerialNumberPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SerialBoundary = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LifeBasis = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLifeLimitProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentLifeLimitProfiles_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StockBaseId = table.Column<int>(type: "int", nullable: true),
                    CurrentAircraftId = table.Column<int>(type: "int", nullable: true),
                    CurrentPositionId = table.Column<int>(type: "int", nullable: true),
                    ManufactureDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentComponentId = table.Column<int>(type: "int", nullable: true),
                    CurrentSlotCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Components_Aircrafts_CurrentAircraftId",
                        column: x => x.CurrentAircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Components_Bases_StockBaseId",
                        column: x => x.StockBaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Components_ComponentPositions_CurrentPositionId",
                        column: x => x.CurrentPositionId,
                        principalSchema: "dbo",
                        principalTable: "ComponentPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Components_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Components_Components_ParentComponentId",
                        column: x => x.ParentComponentId,
                        principalSchema: "dbo",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentTypePositions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    ComponentPositionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTypePositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentTypePositions_ComponentPositions_ComponentPositionId",
                        column: x => x.ComponentPositionId,
                        principalSchema: "dbo",
                        principalTable: "ComponentPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentTypePositions_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponentTypeSlots",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    SlotCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SlotName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxCount = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTypeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentTypeSlots_ComponentTypes_ParentComponentTypeId",
                        column: x => x.ParentComponentTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentLifeLimitStages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentLifeLimitProfileId = table.Column<int>(type: "int", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    StageType = table.Column<int>(type: "int", nullable: false),
                    IntervalFHMinutes = table.Column<int>(type: "int", nullable: true),
                    IntervalCycles = table.Column<int>(type: "int", nullable: true),
                    IntervalCalendarDays = table.Column<int>(type: "int", nullable: true),
                    IntervalTgoLandings = table.Column<int>(type: "int", nullable: true),
                    IntervalFullStopLandings = table.Column<int>(type: "int", nullable: true),
                    BandEndFHMinutes = table.Column<int>(type: "int", nullable: true),
                    BandEndCycles = table.Column<int>(type: "int", nullable: true),
                    BandEndCalendarDays = table.Column<int>(type: "int", nullable: true),
                    BandEndTgoLandings = table.Column<int>(type: "int", nullable: true),
                    BandEndFullStopLandings = table.Column<int>(type: "int", nullable: true),
                    ToleranceType = table.Column<int>(type: "int", nullable: false),
                    ToleranceFHMinutes = table.Column<int>(type: "int", nullable: true),
                    ToleranceCycles = table.Column<int>(type: "int", nullable: true),
                    ToleranceCalendarDays = table.Column<int>(type: "int", nullable: true),
                    ToleranceTgoLandings = table.Column<int>(type: "int", nullable: true),
                    ToleranceFullStopLandings = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLifeLimitStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentLifeLimitStages_ComponentLifeLimitProfiles_ComponentLifeLimitProfileId",
                        column: x => x.ComponentLifeLimitProfileId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponentEvents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    AircraftFHAtEventMinutes = table.Column<int>(type: "int", nullable: true),
                    AircraftCyclesAtEvent = table.Column<int>(type: "int", nullable: true),
                    AircraftTgoLandingsAtEvent = table.Column<int>(type: "int", nullable: true),
                    AircraftFullStopLandingsAtEvent = table.Column<int>(type: "int", nullable: true),
                    RemovalReason = table.Column<int>(type: "int", nullable: true),
                    RelatedParentComponentId = table.Column<int>(type: "int", nullable: true),
                    SlotCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LinkedWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentEvents_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentEvents_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentEvents_ComponentPositions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "dbo",
                        principalTable: "ComponentPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentEvents_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "dbo",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentEvents_Components_RelatedParentComponentId",
                        column: x => x.RelatedParentComponentId,
                        principalSchema: "dbo",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentEvents_WorkOrders_LinkedWorkOrderId",
                        column: x => x.LinkedWorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentLifeStatuses",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    CumulativeFHMinutes = table.Column<int>(type: "int", nullable: false),
                    CumulativeCycles = table.Column<int>(type: "int", nullable: false),
                    CumulativeCalendarDays = table.Column<int>(type: "int", nullable: false),
                    CumulativeTgoLandings = table.Column<int>(type: "int", nullable: false),
                    CumulativeFullStopLandings = table.Column<int>(type: "int", nullable: false),
                    SinceOverhaulFHMinutes = table.Column<int>(type: "int", nullable: false),
                    SinceOverhaulCycles = table.Column<int>(type: "int", nullable: false),
                    SinceOverhaulCalendarDays = table.Column<int>(type: "int", nullable: false),
                    SinceOverhaulTgoLandings = table.Column<int>(type: "int", nullable: false),
                    SinceOverhaulFullStopLandings = table.Column<int>(type: "int", nullable: false),
                    LastOverhaulDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RemainingFHMinutes = table.Column<int>(type: "int", nullable: true),
                    RemainingCycles = table.Column<int>(type: "int", nullable: true),
                    RemainingCalendarDays = table.Column<int>(type: "int", nullable: true),
                    RemainingTgoLandings = table.Column<int>(type: "int", nullable: true),
                    RemainingFullStopLandings = table.Column<int>(type: "int", nullable: true),
                    MatchedLifeLimitProfileId = table.Column<int>(type: "int", nullable: true),
                    CurrentStageSequence = table.Column<int>(type: "int", nullable: true),
                    MissedOverhaulCount = table.Column<int>(type: "int", nullable: false),
                    LifeLimitExceeded = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLifeStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentLifeStatuses_ComponentLifeLimitProfiles_MatchedLifeLimitProfileId",
                        column: x => x.MatchedLifeLimitProfileId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComponentLifeStatuses_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "dbo",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponentTypeSubAssemblySlotEligibilities",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    ChildComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTypeSubAssemblySlotEligibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentTypeSubAssemblySlotEligibilities_ComponentTypeSlots_SlotId",
                        column: x => x.SlotId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentTypeSubAssemblySlotEligibilities_ComponentTypes_ChildComponentTypeId",
                        column: x => x.ChildComponentTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEvents_AircraftId",
                schema: "dbo",
                table: "ComponentEvents",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEvents_ComponentId_EventDate",
                schema: "dbo",
                table: "ComponentEvents",
                columns: new[] { "ComponentId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEvents_LinkedWorkOrderId",
                schema: "dbo",
                table: "ComponentEvents",
                column: "LinkedWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEvents_PerformedByUserId",
                schema: "dbo",
                table: "ComponentEvents",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEvents_PositionId",
                schema: "dbo",
                table: "ComponentEvents",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEvents_RelatedParentComponentId",
                schema: "dbo",
                table: "ComponentEvents",
                column: "RelatedParentComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitProfiles_ComponentTypeId",
                schema: "dbo",
                table: "ComponentLifeLimitProfiles",
                column: "ComponentTypeId",
                unique: true,
                filter: "[ApplicabilityRuleType] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitStages_ComponentLifeLimitProfileId_SequenceOrder",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                columns: new[] { "ComponentLifeLimitProfileId", "SequenceOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeStatuses_ComponentId",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                column: "ComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeStatuses_MatchedLifeLimitProfileId",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                column: "MatchedLifeLimitProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentPositions_AcTypeId_Code",
                schema: "dbo",
                table: "ComponentPositions",
                columns: new[] { "AcTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentPositions_AtaId",
                schema: "dbo",
                table: "ComponentPositions",
                column: "AtaId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_ComponentTypeId_SerialNumber",
                schema: "dbo",
                table: "Components",
                columns: new[] { "ComponentTypeId", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Components_CurrentAircraftId",
                schema: "dbo",
                table: "Components",
                column: "CurrentAircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_CurrentPositionId",
                schema: "dbo",
                table: "Components",
                column: "CurrentPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_ParentComponentId",
                schema: "dbo",
                table: "Components",
                column: "ParentComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_StockBaseId",
                schema: "dbo",
                table: "Components",
                column: "StockBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypePositions_ComponentPositionId",
                schema: "dbo",
                table: "ComponentTypePositions",
                column: "ComponentPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypePositions_ComponentTypeId_ComponentPositionId",
                schema: "dbo",
                table: "ComponentTypePositions",
                columns: new[] { "ComponentTypeId", "ComponentPositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypes_AircraftManufacturerId",
                schema: "dbo",
                table: "ComponentTypes",
                column: "AircraftManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypes_AtaId",
                schema: "dbo",
                table: "ComponentTypes",
                column: "AtaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypes_PartNumber",
                schema: "dbo",
                table: "ComponentTypes",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypeSlots_ParentComponentTypeId_SlotCode",
                schema: "dbo",
                table: "ComponentTypeSlots",
                columns: new[] { "ParentComponentTypeId", "SlotCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypeSubAssemblySlotEligibilities_ChildComponentTypeId",
                schema: "dbo",
                table: "ComponentTypeSubAssemblySlotEligibilities",
                column: "ChildComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTypeSubAssemblySlotEligibilities_SlotId_ChildComponentTypeId",
                schema: "dbo",
                table: "ComponentTypeSubAssemblySlotEligibilities",
                columns: new[] { "SlotId", "ChildComponentTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentEvents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentLifeLimitStages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentLifeStatuses",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentTypePositions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentTypeSubAssemblySlotEligibilities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentLifeLimitProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Components",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentTypeSlots",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentPositions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentTypes",
                schema: "dbo");
        }
    }
}
