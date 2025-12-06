using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class CreateMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    BaseId = table.Column<int>(type: "int", nullable: true),
                    Roles = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId_SortOrder",
                table: "MenuItems",
                columns: new[] { "ParentId", "SortOrder" });

            // optional seed using explicit IDs (keeps ParentId links stable)
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT dbo.MenuItems ON;

INSERT INTO dbo.MenuItems (Id, Title, IconClass, Controller, Action, Url, ParentId, SortOrder, DepartmentId, BaseId, Roles)
VALUES
    (1000, N'Squadron', N'fa fa-fighter-jet', NULL, NULL, NULL, NULL, 100, NULL, NULL, NULL),
    (2000, N'CrewChief', N'fas fa-user-cog', NULL, NULL, NULL, NULL, 200, NULL, NULL, NULL),
    (3000, N'Aircraft', N'fa fa-plane', NULL, NULL, NULL, NULL, 300, NULL, NULL, NULL);

INSERT INTO dbo.MenuItems (Id, Title, IconClass, Controller, Action, Url, ParentId, SortOrder, DepartmentId, BaseId, Roles)
VALUES
    (1001, N'Create ODV', NULL, N'Odv', N'Create', NULL, 1000, 10, NULL, NULL, NULL),
    (1002, N'Pilot Logbook', NULL, N'PilotLog', N'Index', NULL, 1000, 20, NULL, NULL, NULL),
    (1003, N'Update Sortie', NULL, N'Sortie', N'Edit', NULL, 1000, 30, NULL, NULL, NULL),

    (2001, N'Assign Aircraft', NULL, N'CrewChief', N'AssignAircraft', NULL, 2000, 10, NULL, NULL, NULL),
    (2002, N'Report Malfunction', NULL, N'CrewChief', N'ReportMalfunction', NULL, 2000, 20, NULL, NULL, NULL),
    (2003, N'Maintenance Log', NULL, N'CrewChief', N'MaintenanceLog', NULL, 2000, 30, NULL, NULL, NULL),

    (3001, N'List', NULL, N'Aircraft', N'Index', NULL, 3000, 10, NULL, NULL, NULL),
    (3002, N'Create', NULL, N'Aircraft', N'Create', NULL, 3000, 20, NULL, NULL, NULL);

SET IDENTITY_INSERT dbo.MenuItems OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
           name: "MenuItems");
        }
    }
}
