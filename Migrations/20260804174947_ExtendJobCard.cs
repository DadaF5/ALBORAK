using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class ExtendJobCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ElectricalPowerRequired",
                table: "JobCards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FigureRef",
                table: "JobCards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MechNo",
                table: "JobCards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkAreas",
                table: "JobCards",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricalPowerRequired",
                table: "JobCards");

            migrationBuilder.DropColumn(
                name: "FigureRef",
                table: "JobCards");

            migrationBuilder.DropColumn(
                name: "MechNo",
                table: "JobCards");

            migrationBuilder.DropColumn(
                name: "WorkAreas",
                table: "JobCards");
        }
    }
}
