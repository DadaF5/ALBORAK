using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddWing_AndSquadron : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WingLong = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AcMainGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wings_AcMainGroups_AcMainGroupId",
                        column: x => x.AcMainGroupId,
                        principalTable: "AcMainGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wings_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Squadrons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CallSign = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FrenchName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CallSignShort = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WingId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Squadrons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Squadrons_Wings_WingId",
                        column: x => x.WingId,
                        principalTable: "Wings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Squadrons_WingId",
                table: "Squadrons",
                column: "WingId");

            migrationBuilder.CreateIndex(
                name: "IX_Wings_AcMainGroupId",
                table: "Wings",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Wings_DepartmentId",
                table: "Wings",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Squadrons");

            migrationBuilder.DropTable(
                name: "Wings");
        }
    }
}
