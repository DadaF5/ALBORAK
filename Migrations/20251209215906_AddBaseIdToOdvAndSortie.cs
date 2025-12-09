using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseIdToOdvAndSortie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Odvs_SquadronId",
                table: "Odvs");

            migrationBuilder.AddColumn<int>(
                name: "BaseId",
                table: "Sorties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaseId",
                table: "Odvs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sorties_Base_StartTime",
                table: "Sorties",
                columns: new[] { "BaseId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Sorties_Odv_Completed",
                table: "Sorties",
                columns: new[] { "OdvId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_BaseId_OdvDate",
                table: "Odvs",
                columns: new[] { "BaseId", "OdvDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_SquadronId_OdvDate",
                table: "Odvs",
                columns: new[] { "SquadronId", "OdvDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Odvs_Bases_BaseId",
                table: "Odvs",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Odvs_Bases_BaseId",
                table: "Odvs");

            migrationBuilder.DropIndex(
                name: "IX_Sorties_Base_StartTime",
                table: "Sorties");

            migrationBuilder.DropIndex(
                name: "IX_Sorties_Odv_Completed",
                table: "Sorties");

            migrationBuilder.DropIndex(
                name: "IX_Odvs_BaseId_OdvDate",
                table: "Odvs");

            migrationBuilder.DropIndex(
                name: "IX_Odvs_SquadronId_OdvDate",
                table: "Odvs");

            migrationBuilder.DropColumn(
                name: "BaseId",
                table: "Sorties");

            migrationBuilder.DropColumn(
                name: "BaseId",
                table: "Odvs");

            migrationBuilder.CreateIndex(
                name: "IX_Odvs_SquadronId",
                table: "Odvs",
                column: "SquadronId");
        }
    }
}
