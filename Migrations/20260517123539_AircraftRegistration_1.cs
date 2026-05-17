using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AircraftRegistration_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsoCode = table.Column<string>(type: "char(2)", fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Continent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "Continent", "IsActive", "IsoCode", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Afrique", true, "MA", "Maroc", 1 },
                    { 2, "Amerique du Nord", true, "US", "Etats-Unis", 2 },
                    { 3, "Europe", true, "FR", "France", 3 },
                    { 4, "Europe", true, "GB", "Royaume-Uni", 4 },
                    { 5, "Europe", true, "DE", "Allemagne", 5 },
                    { 6, "Europe", true, "IT", "Italie", 6 },
                    { 7, "Europe", true, "ES", "Espagne", 7 },
                    { 8, "Europe / Asie", true, "RU", "Russie", 8 },
                    { 9, "Asie", true, "CN", "Chine", 9 },
                    { 10, "Amerique du Sud", true, "BR", "Bresil", 10 },
                    { 11, "Afrique", true, "DZ", "Algerie", 99 },
                    { 12, "Afrique", true, "TN", "Tunisie", 99 },
                    { 13, "Afrique", true, "LY", "Libye", 99 },
                    { 14, "Afrique", true, "EG", "Egypte", 99 },
                    { 15, "Afrique", true, "SN", "Senegal", 99 },
                    { 16, "Afrique", true, "NG", "Nigeria", 99 },
                    { 17, "Afrique", true, "ZA", "Afrique du Sud", 99 },
                    { 18, "Moyen-Orient", true, "SA", "Arabie Saoudite", 99 },
                    { 19, "Moyen-Orient", true, "AE", "Emirats Arabes Unis", 99 },
                    { 20, "Europe / Asie", true, "TR", "Turquie", 99 },
                    { 21, "Moyen-Orient", true, "IL", "Israel", 99 },
                    { 22, "Moyen-Orient", true, "JO", "Jordanie", 99 },
                    { 23, "Europe", true, "NL", "Pays-Bas", 99 },
                    { 24, "Europe", true, "BE", "Belgique", 99 },
                    { 25, "Europe", true, "CH", "Suisse", 99 },
                    { 26, "Europe", true, "SE", "Suede", 99 },
                    { 27, "Europe", true, "PT", "Portugal", 99 },
                    { 28, "Europe", true, "PL", "Pologne", 99 },
                    { 29, "Europe", true, "CZ", "Republique tcheque", 99 },
                    { 30, "Europe", true, "UA", "Ukraine", 99 },
                    { 31, "Amerique du Nord", true, "CA", "Canada", 99 },
                    { 32, "Amerique du Nord", true, "MX", "Mexique", 99 },
                    { 33, "Asie", true, "JP", "Japon", 99 },
                    { 34, "Asie", true, "KR", "Coree du Sud", 99 },
                    { 35, "Asie", true, "IN", "Inde", 99 },
                    { 36, "Asie", true, "PK", "Pakistan", 99 },
                    { 37, "Oceanie", true, "AU", "Australie", 99 }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Country_IsoCode",
                table: "Country",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Country_Name",
                table: "Country",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
