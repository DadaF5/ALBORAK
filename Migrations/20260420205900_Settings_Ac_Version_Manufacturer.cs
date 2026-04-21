using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class Settings_Ac_Version_Manufacturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AircraftManufacturerId",
                schema: "dbo",
                table: "AcTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftVersionId",
                schema: "dbo",
                table: "AcTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "dbo",
                table: "AcTypes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "AcTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "SortOrder",
                schema: "dbo",
                table: "AcTypes",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "AircraftManufacturers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftManufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AircraftVersions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcTypes_AircraftManufacturerId",
                schema: "dbo",
                table: "AcTypes",
                column: "AircraftManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_AcTypes_AircraftVersionId",
                schema: "dbo",
                table: "AcTypes",
                column: "AircraftVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftManufacturers_Code",
                schema: "dbo",
                table: "AircraftManufacturers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AircraftVersions_Code",
                schema: "dbo",
                table: "AircraftVersions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcTypes_AircraftManufacturers_AircraftManufacturerId",
                schema: "dbo",
                table: "AcTypes",
                column: "AircraftManufacturerId",
                principalSchema: "dbo",
                principalTable: "AircraftManufacturers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AcTypes_AircraftVersions_AircraftVersionId",
                schema: "dbo",
                table: "AcTypes",
                column: "AircraftVersionId",
                principalSchema: "dbo",
                principalTable: "AircraftVersions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcTypes_AircraftManufacturers_AircraftManufacturerId",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_AcTypes_AircraftVersions_AircraftVersionId",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropTable(
                name: "AircraftManufacturers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AircraftVersions",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_AcTypes_AircraftManufacturerId",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropIndex(
                name: "IX_AcTypes_AircraftVersionId",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "AircraftManufacturerId",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "AircraftVersionId",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "dbo",
                table: "AcTypes");
        }
    }
}
