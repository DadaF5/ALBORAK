using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class Login_Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_CallSigns_Bases_BaseId",
                table: "CallSigns",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CallSigns_Squadrons_SquadronId",
                table: "CallSigns",
                column: "SquadronId",
                principalTable: "Squadrons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallSigns_Bases_BaseId",
                table: "CallSigns");

            migrationBuilder.DropForeignKey(
                name: "FK_CallSigns_Squadrons_SquadronId",
                table: "CallSigns");
        }
    }
}
