using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class MedicaCheckRefined : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalChecks_Bases_BaseId",
                table: "MedicalChecks");

            migrationBuilder.DropIndex(
                name: "IX_MedicalChecks_BaseId",
                table: "MedicalChecks");

            

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "MedicalChecks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "OBESITE",
                table: "MedicalChecks",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LateCheckReason",
                table: "MedicalChecks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Decision",
                table: "MedicalChecks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MedicalChecks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "C_Optique",
                table: "MedicalChecks",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            
            migrationBuilder.AddColumn<int>(
                name: "DurationYears",
                table: "MedicalChecks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "MedicalChecks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "MedicalChecks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
               name: "DurationYears",
               table: "MedicalChecks");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "MedicalChecks");

            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "MedicalChecks");
                       

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "MedicalChecks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "OBESITE",
                table: "MedicalChecks",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "LateCheckReason",
                table: "MedicalChecks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Decision",
                table: "MedicalChecks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MedicalChecks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "C_Optique",
                table: "MedicalChecks",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalChecks_BaseId",
                table: "MedicalChecks",
                column: "BaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalChecks_Bases_BaseId",
                table: "MedicalChecks",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
