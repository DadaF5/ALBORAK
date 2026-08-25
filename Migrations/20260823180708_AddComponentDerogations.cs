using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentDerogations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LotReference",
                schema: "dbo",
                table: "Components",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComponentDerogations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    DimensionTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetStageType = table.Column<int>(type: "int", nullable: false),
                    ApplicabilityRuleType = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SerialNumberPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SerialBoundary = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LotReference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    Tier = table.Column<int>(type: "int", nullable: true),
                    ApprovalAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupportingEvidence = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsConditional = table.Column<bool>(type: "bit", nullable: false),
                    ConditionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SupersedesDerogationId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentDerogations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentDerogations_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentDerogations_ComponentDerogations_SupersedesDerogationId",
                        column: x => x.SupersedesDerogationId,
                        principalSchema: "dbo",
                        principalTable: "ComponentDerogations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentDerogations_ComponentLifeLimitDimensionTypes_DimensionTypeId",
                        column: x => x.DimensionTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentLifeLimitDimensionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentDerogations_ComponentTypes_ComponentTypeId",
                        column: x => x.ComponentTypeId,
                        principalSchema: "dbo",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDerogations_ComponentTypeId_IssuedDate",
                schema: "dbo",
                table: "ComponentDerogations",
                columns: new[] { "ComponentTypeId", "IssuedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDerogations_CreatedByUserId",
                schema: "dbo",
                table: "ComponentDerogations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDerogations_DimensionTypeId",
                schema: "dbo",
                table: "ComponentDerogations",
                column: "DimensionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDerogations_SupersedesDerogationId",
                schema: "dbo",
                table: "ComponentDerogations",
                column: "SupersedesDerogationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentDerogations",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "LotReference",
                schema: "dbo",
                table: "Components");
        }
    }
}
