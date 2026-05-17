using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for AcCategory.
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new AcCategoryConfiguration());
    ///
    /// Seed data: 3 fixed rows — defined by DAM Form 5a (Step 2 radio cards).
    /// NEVER change Id values once this migration is applied.
    ///
    /// DB note: existing column is "AcCategoryId" — mapped via [Column]
    /// attribute on the entity. Remove attribute once column is renamed to Id.
    /// </summary>
    public class AcCategoryConfiguration : IEntityTypeConfiguration<AcCategory>
    {
        public void Configure(EntityTypeBuilder<AcCategory> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("AcCategory");

            // ── Primary key ──────────────────────────────────────────────
            // Property is Id, column is AcCategoryId (legacy name).
            // The [Column("AcCategoryId")] attribute on the entity handles
            // the mapping — no extra config needed here.
            builder.HasKey(c => c.Id);

            // ── Code ─────────────────────────────────────────────────────
            builder.Property(c => c.Code)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("UX_AcCategory_Code");

            // ── Name ─────────────────────────────────────────────────────
            builder.Property(c => c.Name)
                .HasColumnType("nvarchar(50)")
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("UX_AcCategory_Name");

            // ── Description ──────────────────────────────────────────────
            builder.Property(c => c.Description)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            // ── IconKey ──────────────────────────────────────────────────
            // Stores emoji or icon identifier for Form 5a radio cards.
            builder.Property(c => c.IconKey)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired(false);

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(c => c.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // 3 rows matching Form 5a Step 2 radio cards exactly.
            // IconKey values match the emoji shown in the prototype HTML.
            builder.HasData(
                new AcCategory
                {
                    Id          = 1,
                    Code        = "AVION",
                    Name        = "Avion",
                    Description = "Aeronef a voilure fixe",
                    IconKey     = "✈",
                    SortOrder   = 1,
                    IsActive    = true
                },
                new AcCategory
                {
                    Id          = 2,
                    Code        = "HELI",
                    Name        = "Helicoptere",
                    Description = "Aeronef a voilure tournante",
                    IconKey     = "🚁",
                    SortOrder   = 2,
                    IsActive    = true
                },
                new AcCategory
                {
                    Id          = 3,
                    Code        = "UAS",
                    Name        = "UAS / Drone",
                    Description = "Aeronef sans equipage a bord",
                    IconKey     = "◈",
                    SortOrder   = 3,
                    IsActive    = true
                }
            );
        }
    }
}
