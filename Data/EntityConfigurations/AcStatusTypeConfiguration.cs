
using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for AcStatusType.
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new AcStatusTypeConfiguration());
    ///
    /// Seed data: 5 rows — standard operational aircraft statuses.
    /// NEVER change Id values once this migration is applied.
    /// </summary>
    public class AcStatusTypeConfiguration : IEntityTypeConfiguration<AcStatusType>
    {
        public void Configure(EntityTypeBuilder<AcStatusType> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("AcStatusType");

            // ── Primary key ──────────────────────────────────────────────
            builder.HasKey(s => s.Id);

            // ── Code ─────────────────────────────────────────────────────
            builder.Property(s => s.Code)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(s => s.Code)
                .IsUnique()
                .HasDatabaseName("UX_AcStatusType_Code");

            // ── Name ─────────────────────────────────────────────────────
            builder.Property(s => s.Name)
                .HasColumnType("nvarchar(100)")
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(s => s.Name)
                .IsUnique()
                .HasDatabaseName("UX_AcStatusType_Name");

            // ── Description ──────────────────────────────────────────────
            builder.Property(s => s.Description)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(s => s.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(s => s.IsActive)
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // 5 standard aircraft operational statuses.
            // OPR = SortOrder 1 — most common status first in DDLs.
            // SortOrder reflects operational frequency, not severity.
            builder.HasData(
                new AcStatusType
                {
                    Id          = 1,
                    Code        = "OPR",
                    Name        = "Operationnel",
                    Description = "Aeronef en etat de vol et disponible pour mission",
                    SortOrder   = 1,
                    IsActive    = true
                },
                new AcStatusType
                {
                    Id          = 2,
                    Code        = "MNT",
                    Name        = "En maintenance",
                    Description = "Aeronef immobilise pour maintenance programmee ou corrective",
                    SortOrder   = 2,
                    IsActive    = true
                },
                new AcStatusType
                {
                    Id          = 3,
                    Code        = "AOG",
                    Name        = "Aircraft on Ground",
                    Description = "Aeronef immobilise suite a panne — priorite de remise en etat",
                    SortOrder   = 3,
                    IsActive    = true
                },
                new AcStatusType
                {
                    Id          = 4,
                    Code        = "STK",
                    Name        = "En stockage",
                    Description = "Aeronef mis en conservation — non disponible pour operations",
                    SortOrder   = 4,
                    IsActive    = true
                },
                new AcStatusType
                {
                    Id          = 5,
                    Code        = "RAD",
                    Name        = "Radie",
                    Description = "Aeronef retire du service — radiation du registre DAM",
                    SortOrder   = 5,
                    IsActive    = true
                }
            );
        }
    }
}
