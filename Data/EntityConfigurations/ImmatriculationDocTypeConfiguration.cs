using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for ImmatriculationDocType.
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new ImmatriculationDocTypeConfiguration());
    ///
    /// Seed data: 6 rows — legally mandated by DAM regulation
    /// (Art. 15, Points 1–6, GUI-DPC-001).
    ///
    /// CRITICAL: NEVER hard-delete these rows.
    /// Use IsActive = false only.
    /// Ids are stable — do not renumber after migration.
    /// </summary>
    public class ImmatriculationDocTypeConfiguration
        : IEntityTypeConfiguration<ImmatriculationDocType>
    {
        public void Configure(EntityTypeBuilder<ImmatriculationDocType> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("ImmatriculationDocType");

            // ── Primary key ──────────────────────────────────────────────
            builder.HasKey(d => d.Id);

            // ── Code ─────────────────────────────────────────────────────
            builder.Property(d => d.Code)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(d => d.Code)
                .IsUnique()
                .HasDatabaseName("UX_ImmatriculationDocType_Code");

            // ── Name ─────────────────────────────────────────────────────
            builder.Property(d => d.Name)
                .HasColumnType("nvarchar(200)")
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(d => d.Name)
                .IsUnique()
                .HasDatabaseName("UX_ImmatriculationDocType_Name");

            // ── ArticleReference ─────────────────────────────────────────
            builder.Property(d => d.ArticleReference)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20)
                .IsRequired(false);

            // ── IsRequired ───────────────────────────────────────────────
            builder.Property(d => d.IsRequired)
                .HasDefaultValue(false);

            // ── AcceptedFormats ──────────────────────────────────────────
            builder.Property(d => d.AcceptedFormats)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);

            // ── MaxFileSizeMb ────────────────────────────────────────────
            builder.Property(d => d.MaxFileSizeMb)
                .IsRequired(false);

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(d => d.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // Source: Form 5a Step 4, GUI-DPC-001 Art. 15 Points 1–6
            // IsRequired:
            //   true  = "Obligatoire" — blocks submission if missing
            //   false = "Si applicable" or "Si disponible" — optional
            builder.HasData(

                new ImmatriculationDocType
                {
                    Id               = 1,
                    Code             = "DOC01",
                    Name             = "Justificatif de propriete ou droit d'exploitation",
                    ArticleReference = "Art. 15.1",
                    IsRequired       = true,
                    AcceptedFormats  = "PDF",
                    MaxFileSizeMb    = 10,
                    SortOrder        = 1,
                    IsActive         = true
                },

                new ImmatriculationDocType
                {
                    Id               = 2,
                    Code             = "DOC02",
                    Name             = "Photographie plaque signaletique constructeur",
                    ArticleReference = "Art. 15.2",
                    IsRequired       = true,
                    AcceptedFormats  = "JPG,PNG",
                    MaxFileSizeMb    = 5,
                    SortOrder        = 2,
                    IsActive         = true
                },

                new ImmatriculationDocType
                {
                    Id               = 3,
                    Code             = "DOC03",
                    Name             = "Certificat de radiation du registre etranger",
                    ArticleReference = "Art. 15.3",
                    IsRequired       = false,   // Si applicable
                    AcceptedFormats  = "PDF",
                    MaxFileSizeMb    = 10,
                    SortOrder        = 3,
                    IsActive         = true
                },

                new ImmatriculationDocType
                {
                    Id               = 4,
                    Code             = "DOC04",
                    Name             = "Copie du contrat d'assurance",
                    ArticleReference = "Art. 15.4",
                    IsRequired       = true,
                    AcceptedFormats  = "PDF",
                    MaxFileSizeMb    = 10,
                    SortOrder        = 4,
                    IsActive         = true
                },

                new ImmatriculationDocType
                {
                    Id               = 5,
                    Code             = "DOC05",
                    Name             = "Certificat de navigabilite ou autorisation de vol",
                    ArticleReference = "Art. 15.5",
                    IsRequired       = false,   // Si disponible
                    AcceptedFormats  = "PDF",
                    MaxFileSizeMb    = 10,
                    SortOrder        = 5,
                    IsActive         = true
                },

                new ImmatriculationDocType
                {
                    Id               = 6,
                    Code             = "DOC06",
                    Name             = "Documents de dedouanement",
                    ArticleReference = "Art. 15.6",
                    IsRequired       = false,   // Si applicable (origine etrangere)
                    AcceptedFormats  = "PDF",
                    MaxFileSizeMb    = 10,
                    SortOrder        = 6,
                    IsActive         = true
                }
            );
        }
    }
}
