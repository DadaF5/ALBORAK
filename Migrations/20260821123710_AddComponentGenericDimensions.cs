using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentGenericDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CumulativeCalendarDays",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "CumulativeCycles",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "CumulativeFHMinutes",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "CumulativeFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "CumulativeTgoLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "RemainingCalendarDays",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "RemainingCycles",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "SinceOverhaulCalendarDays",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "SinceOverhaulCycles",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "SinceOverhaulFHMinutes",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "SinceOverhaulFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "SinceOverhaulTgoLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(
                name: "BandEndCalendarDays",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "BandEndCycles",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "BandEndFHMinutes",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "BandEndFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "BandEndTgoLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "IntervalCalendarDays",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "IntervalCycles",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "IntervalFHMinutes",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "IntervalFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "IntervalTgoLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "ToleranceCalendarDays",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "ToleranceCycles",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "ToleranceFHMinutes",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "ToleranceFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "ToleranceTgoLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages");

            migrationBuilder.DropColumn(
                name: "AircraftCyclesAtEvent",
                schema: "dbo",
                table: "ComponentEvents");

            migrationBuilder.DropColumn(
                name: "AircraftFHAtEventMinutes",
                schema: "dbo",
                table: "ComponentEvents");

            migrationBuilder.DropColumn(
                name: "AircraftFullStopLandingsAtEvent",
                schema: "dbo",
                table: "ComponentEvents");

            migrationBuilder.DropColumn(
                name: "AircraftTgoLandingsAtEvent",
                schema: "dbo",
                table: "ComponentEvents");

            migrationBuilder.DropColumn(name: "RemainingTgoLandings", schema: "dbo", table: "ComponentLifeStatuses");
            migrationBuilder.DropColumn(name: "RemainingFullStopLandings", schema: "dbo", table: "ComponentLifeStatuses");
            migrationBuilder.DropColumn(name: "RemainingFHMinutes", schema: "dbo", table: "ComponentLifeStatuses");

            migrationBuilder.AddColumn<int>(name: "DrivingDimensionTypeId", schema: "dbo", table: "ComponentLifeStatuses", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "DrivingDimensionTolerance", schema: "dbo", table: "ComponentLifeStatuses", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "DrivingDimensionRemaining", schema: "dbo", table: "ComponentLifeStatuses", type: "int", nullable: true);

            migrationBuilder.CreateTable(
                name: "ComponentInitialReadings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    PriorOverhaulCount = table.Column<int>(type: "int", nullable: false),
                    PriorLastOverhaulDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentInitialReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentInitialReadings_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentInitialReadings_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "dbo",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponentLifeLimitDimensionTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    IsCalendarBased = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLifeLimitDimensionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComponentEventReadings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentEventId = table.Column<int>(type: "int", nullable: false),
                    DimensionTypeId = table.Column<int>(type: "int", nullable: false),
                    ValueAtEvent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentEventReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentEventReadings_ComponentEvents_ComponentEventId",
                        column: x => x.ComponentEventId,
                        principalSchema: "dbo",
                        principalTable: "ComponentEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentEventReadings_ComponentLifeLimitDimensionTypes_DimensionTypeId",
                        column: x => x.DimensionTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitDimensionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentInitialReadingValues",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentInitialReadingId = table.Column<int>(type: "int", nullable: false),
                    DimensionTypeId = table.Column<int>(type: "int", nullable: false),
                    InitialValue = table.Column<int>(type: "int", nullable: false),
                    PriorSinceOverhaulValue = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentInitialReadingValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentInitialReadingValues_ComponentInitialReadings_ComponentInitialReadingId",
                        column: x => x.ComponentInitialReadingId,
                        principalSchema: "dbo",
                        principalTable: "ComponentInitialReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentInitialReadingValues_ComponentLifeLimitDimensionTypes_DimensionTypeId",
                        column: x => x.DimensionTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitDimensionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentLifeLimitStageDimensions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentLifeLimitStageId = table.Column<int>(type: "int", nullable: false),
                    DimensionTypeId = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: true),
                    BandEnd = table.Column<int>(type: "int", nullable: true),
                    Tolerance = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLifeLimitStageDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentLifeLimitStageDimensions_ComponentLifeLimitDimensionTypes_DimensionTypeId",
                        column: x => x.DimensionTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitDimensionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentLifeLimitStageDimensions_ComponentLifeLimitStages_ComponentLifeLimitStageId",
                        column: x => x.ComponentLifeLimitStageId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponentLifeStatusDimensions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentLifeStatusId = table.Column<int>(type: "int", nullable: false),
                    DimensionTypeId = table.Column<int>(type: "int", nullable: false),
                    Cumulative = table.Column<int>(type: "int", nullable: false),
                    SinceOverhaul = table.Column<int>(type: "int", nullable: false),
                    Remaining = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLifeStatusDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentLifeStatusDimensions_ComponentLifeLimitDimensionTypes_DimensionTypeId",
                        column: x => x.DimensionTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitDimensionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentLifeStatusDimensions_ComponentLifeStatuses_ComponentLifeStatusId",
                        column: x => x.ComponentLifeStatusId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeStatuses_DrivingDimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                column: "DrivingDimensionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEventReadings_ComponentEventId_DimensionTypeId",
                schema: "dbo",
                table: "ComponentEventReadings",
                columns: new[] { "ComponentEventId", "DimensionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentEventReadings_DimensionTypeId",
                schema: "dbo",
                table: "ComponentEventReadings",
                column: "DimensionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentInitialReadings_ComponentId",
                schema: "dbo",
                table: "ComponentInitialReadings",
                column: "ComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentInitialReadings_RecordedByUserId",
                schema: "dbo",
                table: "ComponentInitialReadings",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentInitialReadingValues_ComponentInitialReadingId_DimensionTypeId",
                schema: "dbo",
                table: "ComponentInitialReadingValues",
                columns: new[] { "ComponentInitialReadingId", "DimensionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentInitialReadingValues_DimensionTypeId",
                schema: "dbo",
                table: "ComponentInitialReadingValues",
                column: "DimensionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitDimensionTypes_Code",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitStageDimensions_ComponentLifeLimitStageId_DimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions",
                columns: new[] { "ComponentLifeLimitStageId", "DimensionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitStageDimensions_DimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions",
                column: "DimensionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeStatusDimensions_ComponentLifeStatusId_DimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeStatusDimensions",
                columns: new[] { "ComponentLifeStatusId", "DimensionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeStatusDimensions_DimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeStatusDimensions",
                column: "DimensionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentLifeStatuses_ComponentLifeLimitDimensionTypes_DrivingDimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                column: "DrivingDimensionTypeId",
                principalSchema: "dbo",
                principalTable: "ComponentLifeLimitDimensionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentLifeStatuses_ComponentLifeLimitDimensionTypes_DrivingDimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropTable(
                name: "ComponentEventReadings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentInitialReadingValues",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentLifeLimitStageDimensions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentLifeStatusDimensions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentInitialReadings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComponentLifeLimitDimensionTypes",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ComponentLifeStatuses_DrivingDimensionTypeId",
                schema: "dbo",
                table: "ComponentLifeStatuses");

            migrationBuilder.DropColumn(name: "DrivingDimensionTypeId", schema: "dbo", table: "ComponentLifeStatuses");
            migrationBuilder.DropColumn(name: "DrivingDimensionTolerance", schema: "dbo", table: "ComponentLifeStatuses");
            migrationBuilder.DropColumn(name: "DrivingDimensionRemaining", schema: "dbo", table: "ComponentLifeStatuses");

            migrationBuilder.AddColumn<int>(name: "RemainingTgoLandings", schema: "dbo", table: "ComponentLifeStatuses", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "RemainingFullStopLandings", schema: "dbo", table: "ComponentLifeStatuses", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "RemainingFHMinutes", schema: "dbo", table: "ComponentLifeStatuses", type: "int", nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CumulativeCalendarDays",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CumulativeCycles",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CumulativeFHMinutes",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CumulativeFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CumulativeTgoLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemainingCalendarDays",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingCycles",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SinceOverhaulCalendarDays",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SinceOverhaulCycles",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SinceOverhaulFHMinutes",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SinceOverhaulFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SinceOverhaulTgoLandings",
                schema: "dbo",
                table: "ComponentLifeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BandEndCalendarDays",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BandEndCycles",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BandEndFHMinutes",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BandEndFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BandEndTgoLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalCalendarDays",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalCycles",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalFHMinutes",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalTgoLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToleranceCalendarDays",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToleranceCycles",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToleranceFHMinutes",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToleranceFullStopLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToleranceTgoLandings",
                schema: "dbo",
                table: "ComponentLifeLimitStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftCyclesAtEvent",
                schema: "dbo",
                table: "ComponentEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftFHAtEventMinutes",
                schema: "dbo",
                table: "ComponentEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftFullStopLandingsAtEvent",
                schema: "dbo",
                table: "ComponentEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftTgoLandingsAtEvent",
                schema: "dbo",
                table: "ComponentEvents",
                type: "int",
                nullable: true);
        }
    }
}
