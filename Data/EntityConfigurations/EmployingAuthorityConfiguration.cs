using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for EmployingAuthority.
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new EmployingAuthorityConfiguration());
    ///
    /// Seed data: 5 fixed rows defined by DAM regulation.
    /// These rows are fixed — do not change Id values once migrated.
    /// </summary>
    public class EmployingAuthorityConfiguration
        : IEntityTypeConfiguration<EmployingAuthority>
    {
        public void Configure(EntityTypeBuilder<EmployingAuthority> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("EmployingAuthority");

            // ── Primary key ──────────────────────────────────────────────
            builder.HasKey(e => e.Id);

            // ── Code ─────────────────────────────────────────────────────
            // nvarchar(10) — codes vary in length (FRA=3, MR=2, AUT=3...)
            // Not fixed-length like IsoCode — no IsFixedLength() here.
            // Unique index — no two authorities share the same code.
            builder.Property(e => e.Code)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("UX_EmployingAuthority_Code");

            // ── Name ─────────────────────────────────────────────────────
            builder.Property(e => e.Name)
                .HasColumnType("nvarchar(100)")
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UX_EmployingAuthority_Name");

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(e => e.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // 5 fixed rows — defined by DAM regulation.
            // FRA = SortOrder 1 — always first (primary platform user).
            // NEVER change Id values once this migration is applied.
            builder.HasData(
                new EmployingAuthority
                {
                    Id        = 1,
                    Code      = "FRA",
                    Name      = "Forces Royales Air",
                    SortOrder = 1,
                    IsActive  = true
                },
                new EmployingAuthority
                {
                    Id        = 2,
                    Code      = "MR",
                    Name      = "Marine Royale",
                    SortOrder = 2,
                    IsActive  = true
                },
                new EmployingAuthority
                {
                    Id        = 3,
                    Code      = "GR",
                    Name      = "Gendarmerie Royale",
                    SortOrder = 3,
                    IsActive  = true
                },
                new EmployingAuthority
                {
                    Id        = 4,
                    Code      = "FT",
                    Name      = "Forces Terrestres",
                    SortOrder = 4,
                    IsActive  = true
                },
                new EmployingAuthority
                {
                    Id        = 5,
                    Code      = "AUT",
                    Name      = "Autre",
                    SortOrder = 99,
                    IsActive  = true
                }
            );
        }
    }
}
