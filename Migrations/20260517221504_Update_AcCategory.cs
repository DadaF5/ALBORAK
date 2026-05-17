using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class Update_AcCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcMainGroups_AcCategories_AcCategoryId",
                schema: "dbo",
                table: "AcMainGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircrafts_AcStatusTypes_AcStatusTypeId",
                table: "Aircrafts");

            migrationBuilder.DropTable(
                name: "AcCategories",
                schema: "dbo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcStatusTypes",
                schema: "dbo",
                table: "AcStatusTypes");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                schema: "dbo",
                table: "AcStatusTypes");

            migrationBuilder.DropColumn(
                name: "StatusName",
                schema: "dbo",
                table: "AcStatusTypes");

            migrationBuilder.RenameTable(
                name: "AcStatusTypes",
                schema: "dbo",
                newName: "AcStatusType");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "AcStatusType",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AcStatusType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AcStatusType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AcStatusType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcStatusType",
                table: "AcStatusType",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AcCategory",
                columns: table => new
                {
                    AcCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconKey = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcCategory", x => x.AcCategoryId);
                });

            migrationBuilder.InsertData(
                table: "AcCategory",
                columns: new[] { "AcCategoryId", "Code", "Description", "IconKey", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "AVION", "Aeronef a voilure fixe", "✈", true, "Avion", 1 },
                    { 2, "HELI", "Aeronef a voilure tournante", "🚁", true, "Helicoptere", 2 },
                    { 3, "UAS", "Aeronef sans equipage a bord", "◈", true, "UAS / Drone", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "UX_AcCategory_Code",
                table: "AcCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AcCategory_Name",
                table: "AcCategory",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcMainGroups_AcCategory_AcCategoryId",
                schema: "dbo",
                table: "AcMainGroups",
                column: "AcCategoryId",
                principalTable: "AcCategory",
                principalColumn: "AcCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircrafts_AcStatusType_AcStatusTypeId",
                table: "Aircrafts",
                column: "AcStatusTypeId",
                principalTable: "AcStatusType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcMainGroups_AcCategory_AcCategoryId",
                schema: "dbo",
                table: "AcMainGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Aircrafts_AcStatusType_AcStatusTypeId",
                table: "Aircrafts");

            migrationBuilder.DropTable(
                name: "AcCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcStatusType",
                table: "AcStatusType");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AcStatusType");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AcStatusType");

            migrationBuilder.RenameTable(
                name: "AcStatusType",
                newName: "AcStatusTypes",
                newSchema: "dbo");

            migrationBuilder.AlterColumn<byte>(
                name: "SortOrder",
                schema: "dbo",
                table: "AcStatusTypes",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "AcStatusTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusCode",
                schema: "dbo",
                table: "AcStatusTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                schema: "dbo",
                table: "AcStatusTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcStatusTypes",
                schema: "dbo",
                table: "AcStatusTypes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AcCategories",
                schema: "dbo",
                columns: table => new
                {
                    AcCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcCategories", x => x.AcCategoryId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AcMainGroups_AcCategories_AcCategoryId",
                schema: "dbo",
                table: "AcMainGroups",
                column: "AcCategoryId",
                principalSchema: "dbo",
                principalTable: "AcCategories",
                principalColumn: "AcCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Aircrafts_AcStatusTypes_AcStatusTypeId",
                table: "Aircrafts",
                column: "AcStatusTypeId",
                principalSchema: "dbo",
                principalTable: "AcStatusTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
