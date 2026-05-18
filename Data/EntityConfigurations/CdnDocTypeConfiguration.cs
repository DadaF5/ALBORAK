using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for CdnDocType.
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new CdnDocTypeConfiguration());
    ///
    /// Seed data: 3 fixed rows — from Form 5a Step 3 (GUI-DPC-001 Art. 9–10).
    /// NEVER change Id values once this migration is applied.
    /// </summary>
    public class CdnDocTypeConfiguration : IEntityTypeConfiguration<CdnDocType>
    {
        public void Configure(EntityTypeBuilder<CdnDocType> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("CdnDocType");

            // ── Primary key ──────────────────────────────────────────────
            builder.HasKey(c => c.Id);

            // ── Code ─────────────────────────────────────────────────────
            builder.Property(c => c.Code)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("UX_CdnDocType_Code");

            // ── Name ─────────────────────────────────────────────────────
            builder.Property(c => c.Name)
                .HasColumnType("nvarchar(100)")
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("UX_CdnDocType_Name");

            // ── Description ──────────────────────────────────────────────
            builder.Property(c => c.Description)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(c => c.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // 3 rows matching Form 5a Step 3 dropdown exactly.
            // CDN = SortOrder 1 — default selection in the form.
            builder.HasData(
                new CdnDocType
                {
                    Id          = 1,
                    Code        = "CDN",
                    Name        = "Certificat de navigabilite",
                    Description = "Document de navigabilite delivre par la DAM",
                    SortOrder   = 1,
                    IsActive    = true
                },
                new CdnDocType
                {
                    Id          = 2,
                    Code        = "ADV",
                    Name        = "Autorisation de vol",
                    Description = "Autorisation temporaire delivree en l'absence de CdN",
                    SortOrder   = 2,
                    IsActive    = true
                },
                new CdnDocType
                {
                    Id          = 3,
                    Code        = "AUT",
                    Name        = "Autre",
                    Description = "Tout autre document de navigabilite reconnu par la DAM",
                    SortOrder   = 99,
                    IsActive    = true
                }
            );
        }
    }
}
