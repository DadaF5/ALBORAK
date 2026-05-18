using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class ImmatDocType_MissionRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "AircraftVersions",
                schema: "dbo",
                newName: "AircraftVersions");

            migrationBuilder.RenameTable(
                name: "AircraftManufacturers",
                schema: "dbo",
                newName: "AircraftManufacturers");

            migrationBuilder.RenameTable(
                name: "AcMainGroups",
                schema: "dbo",
                newName: "AcMainGroups");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "AcMainGroups",
                newName: "IsActive");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AcMainGroups",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AcMainGroups",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AcMainGroups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "SortOrder",
                table: "AcMainGroups",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "CdnDocType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CdnDocType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImmatriculationDocType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArticleReference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AcceptedFormats = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaxFileSizeMb = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmatriculationDocType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AcCategoryId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionRole_AcCategory",
                        column: x => x.AcCategoryId,
                        principalTable: "AcCategory",
                        principalColumn: "AcCategoryId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "CdnDocType",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "CDN", "Document de navigabilite delivre par la DAM", true, "Certificat de navigabilite", 1 },
                    { 2, "ADV", "Autorisation temporaire delivree en l'absence de CdN", true, "Autorisation de vol", 2 },
                    { 3, "AUT", "Tout autre document de navigabilite reconnu par la DAM", true, "Autre", 99 }
                });

            migrationBuilder.InsertData(
                table: "ImmatriculationDocType",
                columns: new[] { "Id", "AcceptedFormats", "ArticleReference", "Code", "IsActive", "IsRequired", "MaxFileSizeMb", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "PDF", "Art. 15.1", "DOC01", true, true, 10, "Justificatif de propriete ou droit d'exploitation", 1 },
                    { 2, "JPG,PNG", "Art. 15.2", "DOC02", true, true, 5, "Photographie plaque signaletique constructeur", 2 }
                });

            migrationBuilder.InsertData(
                table: "ImmatriculationDocType",
                columns: new[] { "Id", "AcceptedFormats", "ArticleReference", "Code", "IsActive", "MaxFileSizeMb", "Name", "SortOrder" },
                values: new object[] { 3, "PDF", "Art. 15.3", "DOC03", true, 10, "Certificat de radiation du registre etranger", 3 });

            migrationBuilder.InsertData(
                table: "ImmatriculationDocType",
                columns: new[] { "Id", "AcceptedFormats", "ArticleReference", "Code", "IsActive", "IsRequired", "MaxFileSizeMb", "Name", "SortOrder" },
                values: new object[] { 4, "PDF", "Art. 15.4", "DOC04", true, true, 10, "Copie du contrat d'assurance", 4 });

            migrationBuilder.InsertData(
                table: "ImmatriculationDocType",
                columns: new[] { "Id", "AcceptedFormats", "ArticleReference", "Code", "IsActive", "MaxFileSizeMb", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 5, "PDF", "Art. 15.5", "DOC05", true, 10, "Certificat de navigabilite ou autorisation de vol", 5 },
                    { 6, "PDF", "Art. 15.6", "DOC06", true, 10, "Documents de dedouanement", 6 }
                });

            migrationBuilder.InsertData(
                table: "MissionRole",
                columns: new[] { "Id", "AcCategoryId", "Code", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, "CHASSE", true, "Chasse / Interception", 1 },
                    { 2, 1, "APPUI", true, "Appui sol", 2 },
                    { 3, 1, "RAVITO", true, "Ravitaillement en vol", 3 },
                    { 4, 1, "FORMATION", true, "Entrainement / Formation", 4 },
                    { 5, 1, "MARITIME", true, "Maritime / Patrouille", 5 },
                    { 6, 2, "ASSAULT", true, "Helicoptere d'assaut", 6 },
                    { 7, null, "TRANSPORT", true, "Transport tactique", 7 },
                    { 8, null, "SAR", true, "SAR / CSAR", 8 },
                    { 9, null, "ISR", true, "Reconnaissance / ISR", 9 },
                    { 10, 3, "UAV-ISR", true, "Drone ISR", 10 },
                    { 11, 3, "UAV-ARM", true, "Drone arme", 11 }
                });

            migrationBuilder.CreateIndex(
                name: "UX_CdnDocType_Code",
                table: "CdnDocType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CdnDocType_Name",
                table: "CdnDocType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ImmatriculationDocType_Code",
                table: "ImmatriculationDocType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ImmatriculationDocType_Name",
                table: "ImmatriculationDocType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionRole_AcCategoryId",
                table: "MissionRole",
                column: "AcCategoryId");

            migrationBuilder.CreateIndex(
                name: "UX_MissionRole_Code",
                table: "MissionRole",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MissionRole_Name",
                table: "MissionRole",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CdnDocType");

            migrationBuilder.DropTable(
                name: "ImmatriculationDocType");

            migrationBuilder.DropTable(
                name: "MissionRole");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AcMainGroups");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "AcMainGroups");

            migrationBuilder.RenameTable(
                name: "AircraftVersions",
                newName: "AircraftVersions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AircraftManufacturers",
                newName: "AircraftManufacturers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AcMainGroups",
                newName: "AcMainGroups",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "dbo",
                table: "AcMainGroups",
                newName: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "AcMainGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "AcMainGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
