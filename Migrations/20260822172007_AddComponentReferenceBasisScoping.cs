using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentReferenceBasisScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReferenceBasisId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcMainGroupId",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Destination",
                schema: "dbo",
                table: "ComponentEvents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComponentReferenceBases",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentReferenceBases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitStageDimensions_ReferenceBasisId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions",
                column: "ReferenceBasisId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentLifeLimitDimensionTypes_AcMainGroupId",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentReferenceBases_Code",
                schema: "dbo",
                table: "ComponentReferenceBases",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentLifeLimitDimensionTypes_AcMainGroups_AcMainGroupId",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes",
                column: "AcMainGroupId",
                principalTable: "AcMainGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentLifeLimitStageDimensions_ComponentReferenceBases_ReferenceBasisId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions",
                column: "ReferenceBasisId",
                principalSchema: "dbo",
                principalTable: "ComponentReferenceBases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentLifeLimitDimensionTypes_AcMainGroups_AcMainGroupId",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentLifeLimitStageDimensions_ComponentReferenceBases_ReferenceBasisId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions");

            migrationBuilder.DropTable(
                name: "ComponentReferenceBases",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ComponentLifeLimitStageDimensions_ReferenceBasisId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions");

            migrationBuilder.DropIndex(
                name: "IX_ComponentLifeLimitDimensionTypes_AcMainGroupId",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes");

            migrationBuilder.DropColumn(
                name: "ReferenceBasisId",
                schema: "dbo",
                table: "ComponentLifeLimitStageDimensions");

            migrationBuilder.DropColumn(
                name: "AcMainGroupId",
                schema: "dbo",
                table: "ComponentLifeLimitDimensionTypes");

            migrationBuilder.DropColumn(
                name: "Destination",
                schema: "dbo",
                table: "ComponentEvents");
        }
    }
}
