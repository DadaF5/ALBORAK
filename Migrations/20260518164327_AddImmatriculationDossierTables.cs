using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddImmatriculationDossierTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                table: "CrewMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CrewMembers_Squadrons_SquadronId",
                table: "CrewMembers");

            migrationBuilder.CreateTable(
                name: "ImmatriculationDossier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DossierNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Brouillon"),
                    CurrentStep = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AttestationCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttestationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SignatoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttestationConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmatriculationDossier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DossierAircraft",
                columns: table => new
                {
                    DossierId = table.Column<int>(type: "int", nullable: false),
                    AircraftCategoryId = table.Column<int>(type: "int", nullable: true),
                    AcTypeId = table.Column<int>(type: "int", nullable: true),
                    AircraftSerie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AircraftVersionId = table.Column<int>(type: "int", nullable: true),
                    MissionRoleId = table.Column<int>(type: "int", nullable: true),
                    ManufacturerId = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManufactureDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ServiceEntryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PortAttacheId = table.Column<int>(type: "int", nullable: true),
                    OriginCountryId = table.Column<int>(type: "int", nullable: true),
                    ImmatriculationSuffix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierAircraft", x => x.DossierId);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_AcCategory",
                        column: x => x.AircraftCategoryId,
                        principalTable: "AcCategory",
                        principalColumn: "AcCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_AcType",
                        column: x => x.AcTypeId,
                        principalSchema: "dbo",
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_AircraftVersion",
                        column: x => x.AircraftVersionId,
                        principalTable: "AircraftVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_Dossier",
                        column: x => x.DossierId,
                        principalTable: "ImmatriculationDossier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_Manufacturer",
                        column: x => x.ManufacturerId,
                        principalTable: "AircraftManufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_MissionRole",
                        column: x => x.MissionRoleId,
                        principalTable: "MissionRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_OriginCountry",
                        column: x => x.OriginCountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAircraft_PortAttache",
                        column: x => x.PortAttacheId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DossierAirworthiness",
                columns: table => new
                {
                    DossierId = table.Column<int>(type: "int", nullable: false),
                    HasAirworthinessDoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CdnDocTypeId = table.Column<int>(type: "int", nullable: true),
                    CdnReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CdnDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CdnExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CdnRenewalRequested = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WasForeignRegistered = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ForeignCountryId = table.Column<int>(type: "int", nullable: true),
                    FormerImmatriculation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ForeignRadiationDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierAirworthiness", x => x.DossierId);
                    table.ForeignKey(
                        name: "FK_DossierAirworthiness_CdnDocType",
                        column: x => x.CdnDocTypeId,
                        principalTable: "CdnDocType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAirworthiness_Dossier",
                        column: x => x.DossierId,
                        principalTable: "ImmatriculationDossier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DossierAirworthiness_ForeignCountry",
                        column: x => x.ForeignCountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DossierAuthority",
                columns: table => new
                {
                    DossierId = table.Column<int>(type: "int", nullable: false),
                    EmployingAuthorityId = table.Column<int>(type: "int", nullable: true),
                    BaseAerienneId = table.Column<int>(type: "int", nullable: true),
                    OgmnNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OgmnAggrementDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OgmnSousPartie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OgmnResponsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AeAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AePhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AeEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierAuthority", x => x.DossierId);
                    table.ForeignKey(
                        name: "FK_DossierAuthority_Base",
                        column: x => x.BaseAerienneId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DossierAuthority_Dossier",
                        column: x => x.DossierId,
                        principalTable: "ImmatriculationDossier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DossierAuthority_EmployingAuthority",
                        column: x => x.EmployingAuthorityId,
                        principalTable: "EmployingAuthority",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImmatriculationDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DossierId = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmatriculationDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImmatriculationDocument_DocType",
                        column: x => x.DocumentTypeId,
                        principalTable: "ImmatriculationDocType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImmatriculationDocument_Dossier",
                        column: x => x.DossierId,
                        principalTable: "ImmatriculationDossier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_AcTypeId",
                table: "DossierAircraft",
                column: "AcTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_AircraftCategoryId",
                table: "DossierAircraft",
                column: "AircraftCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_AircraftVersionId",
                table: "DossierAircraft",
                column: "AircraftVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_ManufacturerId",
                table: "DossierAircraft",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_MissionRoleId",
                table: "DossierAircraft",
                column: "MissionRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_OriginCountryId",
                table: "DossierAircraft",
                column: "OriginCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAircraft_PortAttacheId",
                table: "DossierAircraft",
                column: "PortAttacheId");

            migrationBuilder.CreateIndex(
                name: "UX_DossierAircraft_ImmatriculationSuffix",
                table: "DossierAircraft",
                column: "ImmatriculationSuffix",
                unique: true,
                filter: "[ImmatriculationSuffix] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAirworthiness_CdnDocTypeId",
                table: "DossierAirworthiness",
                column: "CdnDocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAirworthiness_ForeignCountryId",
                table: "DossierAirworthiness",
                column: "ForeignCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAuthority_BaseAerienneId",
                table: "DossierAuthority",
                column: "BaseAerienneId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierAuthority_EmployingAuthorityId",
                table: "DossierAuthority",
                column: "EmployingAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_ImmatriculationDocument_DocumentTypeId",
                table: "ImmatriculationDocument",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ImmatriculationDocument_Dossier_Type",
                table: "ImmatriculationDocument",
                columns: new[] { "DossierId", "DocumentTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UX_ImmatriculationDossier_Number",
                table: "ImmatriculationDossier",
                column: "DossierNumber",
                unique: true,
                filter: "[DossierNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                table: "CrewMembers",
                column: "PrimaryQualificationId",
                principalTable: "Qualifications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrewMembers_Squadrons_SquadronId",
                table: "CrewMembers",
                column: "SquadronId",
                principalTable: "Squadrons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                table: "CrewMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CrewMembers_Squadrons_SquadronId",
                table: "CrewMembers");

            migrationBuilder.DropTable(
                name: "DossierAircraft");

            migrationBuilder.DropTable(
                name: "DossierAirworthiness");

            migrationBuilder.DropTable(
                name: "DossierAuthority");

            migrationBuilder.DropTable(
                name: "ImmatriculationDocument");

            migrationBuilder.DropTable(
                name: "ImmatriculationDossier");

            migrationBuilder.AddForeignKey(
                name: "FK_CrewMembers_Qualifications_PrimaryQualificationId",
                table: "CrewMembers",
                column: "PrimaryQualificationId",
                principalTable: "Qualifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrewMembers_Squadrons_SquadronId",
                table: "CrewMembers",
                column: "SquadronId",
                principalTable: "Squadrons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
