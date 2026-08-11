using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkSections_AcTypes_AcTypeId",
                        column: x => x.AcTypeId,
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkSectionId = table.Column<int>(type: "int", nullable: false),
                    FormNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganismeResponsable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeTravail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: true),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true),
                    TempsAlloueMinutes = table.Column<int>(type: "int", nullable: true),
                    TempsPasseSystematiqueMinutes = table.Column<int>(type: "int", nullable: true),
                    TempsPasseRetoucheMinutes = table.Column<int>(type: "int", nullable: true),
                    VieillissementHours = table.Column<int>(type: "int", nullable: true),
                    Directives = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicalOrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectiveIssuedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectiveIssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderSections_AspNetUsers_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderSections_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderSections_WorkSections_WorkSectionId",
                        column: x => x.WorkSectionId,
                        principalTable: "WorkSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSections_OpenedByUserId",
                table: "WorkOrderSections",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSections_WorkOrderId",
                table: "WorkOrderSections",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSections_WorkSectionId",
                table: "WorkOrderSections",
                column: "WorkSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSections_AcTypeId",
                table: "WorkSections",
                column: "AcTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderSections");

            migrationBuilder.DropTable(
                name: "WorkSections");
        }
    }
}
