using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex_AircraftDocumentTypes_Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AircraftDocumentTypes_Code",
                schema: "dbo",
                table: "AircraftDocumentTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AircraftDocumentTypes_Code",
                schema: "dbo",
                table: "AircraftDocumentTypes");
        }
    }
}
