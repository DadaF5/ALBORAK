using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItems : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItems");
        }
    }
}
