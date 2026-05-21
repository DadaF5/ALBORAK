using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for AircraftCertificate.
    /// Applied in FRAContext via:
    ///   modelBuilder.ApplyConfiguration(new AircraftCertificateConfiguration());
    ///
    /// Key constraint:
    ///   Filtered unique index on (AircraftId, CertType) WHERE IsActive = 1
    ///   → one active certificate per type per aircraft
    ///   → allows replacing a certificate (soft-delete old, insert new)
    /// </summary>
    public class AircraftCertificateConfiguration
        : IEntityTypeConfiguration<AircraftCertificate>
    {
        public void Configure(EntityTypeBuilder<AircraftCertificate> builder)
        {
            builder.ToTable("AircraftCertificates");
            builder.HasKey(c => c.Id);

            // ── FK → Aircraft ────────────────────────────────────────────
            builder.HasOne(c => c.Aircraft)
                .WithMany()
                .HasForeignKey(c => c.AircraftId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AircraftCertificates_Aircraft");

            // ── CertType ─────────────────────────────────────────────────
            builder.Property(c => c.CertType)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired();

            // ── Unique: one ACTIVE cert per type per aircraft ─────────────
            // Filtered index allows multiple historical (inactive) rows.
            builder.HasIndex(c => new { c.AircraftId, c.CertType })
                .IsUnique()
                .HasFilter("[IsActive] = 1")
                .HasDatabaseName("UX_AircraftCertificates_Aircraft_Type_Active");

            // ── Reference ────────────────────────────────────────────────
            builder.Property(c => c.Reference)
                .HasColumnType("nvarchar(80)")
                .HasMaxLength(80)
                .IsRequired();

            // ── IssuingAuthority ─────────────────────────────────────────
            builder.Property(c => c.IssuingAuthority)
                .HasColumnType("nvarchar(80)")
                .HasMaxLength(80)
                .IsRequired(false);

            // ── Document ─────────────────────────────────────────────────
            builder.Property(c => c.DocumentPath)
                .HasColumnType("nvarchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(c => c.DocumentName)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            // ── Notes ────────────────────────────────────────────────────
            builder.Property(c => c.Notes)
                .HasColumnType("nvarchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            // ── Audit ────────────────────────────────────────────────────
            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.CreatedByUserId)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // ── Index for dashboard queries ───────────────────────────────
            // DAM Dashboard loads certs by AircraftId — this index
            // makes that query instant across a large fleet.
            builder.HasIndex(c => new { c.AircraftId, c.IsActive })
                .HasDatabaseName("IX_AircraftCertificates_Aircraft_Active");
        }
    }
}
