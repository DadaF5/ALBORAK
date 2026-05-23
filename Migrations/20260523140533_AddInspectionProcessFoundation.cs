using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionProcessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PLANNED"),
                    IntervalHours = table.Column<int>(type: "int", nullable: true),
                    IntervalCycles = table.Column<int>(type: "int", nullable: true),
                    CalendarValue = table.Column<int>(type: "int", nullable: true),
                    CalendarUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ToleranceHours = table.Column<int>(type: "int", nullable: true),
                    ToleranceCycles = table.Column<int>(type: "int", nullable: true),
                    ToleranceCalendarValue = table.Column<int>(type: "int", nullable: true),
                    ToleranceCalendarUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NextInspectionTypeId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionTypes_InspectionTypes_NextInspectionTypeId",
                        column: x => x.NextInspectionTypeId,
                        principalTable: "InspectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    AtaCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CardCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AllocatedTimeMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ToReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChangeNo = table.Column<int>(type: "int", nullable: true),
                    ChangeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobCards_AcTypes_AcTypeId",
                        column: x => x.AcTypeId,
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenancePrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    DocReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChangeNo = table.Column<int>(type: "int", nullable: true),
                    ChangeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)99)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenancePrograms_AcTypes_AcTypeId",
                        column: x => x.AcTypeId,
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    InspectionTypeId = table.Column<int>(type: "int", nullable: false),
                    WOType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "F12"),
                    WOKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PLANNED"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    OpenHours = table.Column<int>(type: "int", nullable: false),
                    OpenCycles = table.Column<int>(type: "int", nullable: false),
                    OpenDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CloseHours = table.Column<int>(type: "int", nullable: true),
                    CloseCycles = table.Column<int>(type: "int", nullable: true),
                    CloseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OpenedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ClosedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_AspNetUsers_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_InspectionTypes_InspectionTypeId",
                        column: x => x.InspectionTypeId,
                        principalTable: "InspectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobCardAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCardAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobCardAttachments_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JobCardAttachments_JobCards_JobCardId",
                        column: x => x.JobCardId,
                        principalTable: "JobCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobCardPlanningRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ConditionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsApplicable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InitialHours = table.Column<int>(type: "int", nullable: true),
                    InitialCycles = table.Column<int>(type: "int", nullable: true),
                    InitialCalendarValue = table.Column<int>(type: "int", nullable: true),
                    InitialCalendarUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RecurringHours = table.Column<int>(type: "int", nullable: true),
                    RecurringCycles = table.Column<int>(type: "int", nullable: true),
                    RecurringCalendarValue = table.Column<int>(type: "int", nullable: true),
                    RecurringCalendarUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ManufacturerSerialFrom = table.Column<int>(type: "int", nullable: true),
                    ManufacturerSerialTo = table.Column<int>(type: "int", nullable: true),
                    RequiredComplianceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ForbiddenComplianceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCardPlanningRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobCardPlanningRules_JobCards_JobCardId",
                        column: x => x.JobCardId,
                        principalTable: "JobCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTypePrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionTypeId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceProgramId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 100)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTypePrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTypePrograms_InspectionTypes_InspectionTypeId",
                        column: x => x.InspectionTypeId,
                        principalTable: "InspectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionTypePrograms_MaintenancePrograms_MaintenanceProgramId",
                        column: x => x.MaintenanceProgramId,
                        principalTable: "MaintenancePrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramJobCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceProgramId = table.Column<int>(type: "int", nullable: false),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramJobCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramJobCards_JobCards_JobCardId",
                        column: x => x.JobCardId,
                        principalTable: "JobCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramJobCards_MaintenancePrograms_MaintenanceProgramId",
                        column: x => x.MaintenanceProgramId,
                        principalTable: "MaintenancePrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    InspectionTypeId = table.Column<int>(type: "int", nullable: false),
                    LastDoneHours = table.Column<int>(type: "int", nullable: true),
                    LastDoneCycles = table.Column<int>(type: "int", nullable: true),
                    LastDoneDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    NextDueHours = table.Column<int>(type: "int", nullable: true),
                    NextDueCycles = table.Column<int>(type: "int", nullable: true),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StatusSnapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionStates_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionStates_InspectionTypes_InspectionTypeId",
                        column: x => x.InspectionTypeId,
                        principalTable: "InspectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionStates_WorkOrders_LastWorkOrderId",
                        column: x => x.LastWorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderJobCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceProgramId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    NAJustification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderJobCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderJobCards_JobCards_JobCardId",
                        column: x => x.JobCardId,
                        principalTable: "JobCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderJobCards_MaintenancePrograms_MaintenanceProgramId",
                        column: x => x.MaintenanceProgramId,
                        principalTable: "MaintenancePrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderJobCards_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AircraftJobCardStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    JobCardId = table.Column<int>(type: "int", nullable: false),
                    AppliedPlanningRuleId = table.Column<int>(type: "int", nullable: true),
                    LastExecutedHours = table.Column<int>(type: "int", nullable: true),
                    LastExecutedCycles = table.Column<int>(type: "int", nullable: true),
                    LastExecutedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NextDueHoursBase = table.Column<int>(type: "int", nullable: true),
                    NextDueHoursExtended = table.Column<int>(type: "int", nullable: true),
                    NextDueCyclesBase = table.Column<int>(type: "int", nullable: true),
                    NextDueCyclesExtended = table.Column<int>(type: "int", nullable: true),
                    NextDueDateBase = table.Column<DateOnly>(type: "date", nullable: true),
                    NextDueDateExtended = table.Column<DateOnly>(type: "date", nullable: true),
                    RemainingHoursBase = table.Column<int>(type: "int", nullable: true),
                    RemainingHoursExtended = table.Column<int>(type: "int", nullable: true),
                    RemainingCyclesBase = table.Column<int>(type: "int", nullable: true),
                    RemainingCyclesExtended = table.Column<int>(type: "int", nullable: true),
                    RemainingDaysBase = table.Column<int>(type: "int", nullable: true),
                    RemainingDaysExtended = table.Column<int>(type: "int", nullable: true),
                    LastWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftJobCardStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AircraftJobCardStates_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftJobCardStates_JobCardPlanningRules_AppliedPlanningRuleId",
                        column: x => x.AppliedPlanningRuleId,
                        principalTable: "JobCardPlanningRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftJobCardStates_JobCards_JobCardId",
                        column: x => x.JobCardId,
                        principalTable: "JobCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftJobCardStates_WorkOrders_LastWorkOrderId",
                        column: x => x.LastWorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderJobCardSignOffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderJobCardId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Accepted = table.Column<bool>(type: "bit", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderJobCardSignOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderJobCardSignOffs_AspNetUsers_SignedByUserId",
                        column: x => x.SignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkOrderJobCardSignOffs_WorkOrderJobCards_WorkOrderJobCardId",
                        column: x => x.WorkOrderJobCardId,
                        principalTable: "WorkOrderJobCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftJobCardStates_AircraftId_JobCardId",
                table: "AircraftJobCardStates",
                columns: new[] { "AircraftId", "JobCardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AircraftJobCardStates_AppliedPlanningRuleId",
                table: "AircraftJobCardStates",
                column: "AppliedPlanningRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftJobCardStates_JobCardId",
                table: "AircraftJobCardStates",
                column: "JobCardId");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftJobCardStates_LastWorkOrderId",
                table: "AircraftJobCardStates",
                column: "LastWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionStates_AircraftId_InspectionTypeId",
                table: "InspectionStates",
                columns: new[] { "AircraftId", "InspectionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionStates_InspectionTypeId",
                table: "InspectionStates",
                column: "InspectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionStates_LastWorkOrderId",
                table: "InspectionStates",
                column: "LastWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTypePrograms_InspectionTypeId_MaintenanceProgramId",
                table: "InspectionTypePrograms",
                columns: new[] { "InspectionTypeId", "MaintenanceProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTypePrograms_MaintenanceProgramId",
                table: "InspectionTypePrograms",
                column: "MaintenanceProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTypes_AcTypeId_Code",
                table: "InspectionTypes",
                columns: new[] { "AcTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTypes_NextInspectionTypeId",
                table: "InspectionTypes",
                column: "NextInspectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardAttachments_JobCardId",
                table: "JobCardAttachments",
                column: "JobCardId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardAttachments_UploadedByUserId",
                table: "JobCardAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardPlanningRules_JobCardId_SortOrder",
                table: "JobCardPlanningRules",
                columns: new[] { "JobCardId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_JobCards_AcTypeId_CardCode",
                table: "JobCards",
                columns: new[] { "AcTypeId", "CardCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePrograms_AcTypeId_Code",
                table: "MaintenancePrograms",
                columns: new[] { "AcTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramJobCards_JobCardId",
                table: "ProgramJobCards",
                column: "JobCardId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramJobCards_MaintenanceProgramId_JobCardId",
                table: "ProgramJobCards",
                columns: new[] { "MaintenanceProgramId", "JobCardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderJobCards_JobCardId",
                table: "WorkOrderJobCards",
                column: "JobCardId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderJobCards_MaintenanceProgramId",
                table: "WorkOrderJobCards",
                column: "MaintenanceProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderJobCards_WorkOrderId_JobCardId_MaintenanceProgramId",
                table: "WorkOrderJobCards",
                columns: new[] { "WorkOrderId", "JobCardId", "MaintenanceProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderJobCardSignOffs_SignedByUserId",
                table: "WorkOrderJobCardSignOffs",
                column: "SignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderJobCardSignOffs_WorkOrderJobCardId_Level",
                table: "WorkOrderJobCardSignOffs",
                columns: new[] { "WorkOrderJobCardId", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AircraftId",
                table: "WorkOrders",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ClosedByUserId",
                table: "WorkOrders",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_InspectionTypeId",
                table: "WorkOrders",
                column: "InspectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_OpenedByUserId",
                table: "WorkOrders",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WONumber",
                table: "WorkOrders",
                column: "WONumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftJobCardStates");

            migrationBuilder.DropTable(
                name: "InspectionStates");

            migrationBuilder.DropTable(
                name: "InspectionTypePrograms");

            migrationBuilder.DropTable(
                name: "JobCardAttachments");

            migrationBuilder.DropTable(
                name: "ProgramJobCards");

            migrationBuilder.DropTable(
                name: "WorkOrderJobCardSignOffs");

            migrationBuilder.DropTable(
                name: "JobCardPlanningRules");

            migrationBuilder.DropTable(
                name: "WorkOrderJobCards");

            migrationBuilder.DropTable(
                name: "JobCards");

            migrationBuilder.DropTable(
                name: "MaintenancePrograms");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "InspectionTypes");
        }
    }
}
