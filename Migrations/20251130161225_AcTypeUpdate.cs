using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AcTypeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WingLong",
                table: "Wings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Wings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "BaseId",
                table: "Wings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxEngines",
                table: "AcTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MaxGrossweight",
                table: "AcTypes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MaxPassengers",
                table: "AcTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Wings_BaseId",
                table: "Wings",
                column: "BaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wings_Bases_BaseId",
                table: "Wings",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wings_Bases_BaseId",
                table: "Wings");

            migrationBuilder.DropIndex(
                name: "IX_Wings_BaseId",
                table: "Wings");

            migrationBuilder.DropColumn(
                name: "BaseId",
                table: "Wings");

            migrationBuilder.DropColumn(
                name: "MaxEngines",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "MaxGrossweight",
                table: "AcTypes");

            migrationBuilder.DropColumn(
                name: "MaxPassengers",
                table: "AcTypes");

            migrationBuilder.AlterColumn<string>(
                name: "WingLong",
                table: "Wings",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Wings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
