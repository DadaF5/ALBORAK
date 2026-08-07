using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAtaCategoryAndAtaCategoryFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtaId",
                table: "JobCards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AtaCategories",
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
                    table.PrimaryKey("PK_AtaCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ATA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtaCategoryId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ATA_AtaCategories_AtaCategoryId",
                        column: x => x.AtaCategoryId,
                        principalTable: "AtaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobCards_AtaId",
                table: "JobCards",
                column: "AtaId");

            migrationBuilder.CreateIndex(
                name: "IX_ATA_AtaCategoryId",
                table: "ATA",
                column: "AtaCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AtaCategories_Code",
                table: "AtaCategories",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobCards_ATA_AtaId",
                table: "JobCards",
                column: "AtaId",
                principalTable: "ATA",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobCards_ATA_AtaId",
                table: "JobCards");

            migrationBuilder.DropTable(
                name: "ATA");

            migrationBuilder.DropTable(
                name: "AtaCategories");

            migrationBuilder.DropIndex(
                name: "IX_JobCards_AtaId",
                table: "JobCards");

            migrationBuilder.DropColumn(
                name: "AtaId",
                table: "JobCards");
        }
    }
}
