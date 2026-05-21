using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    // ════════════════════════════════════════════════════════════════════
    //  AircraftRestriction
    // ════════════════════════════════════════════════════════════════════
    public class AircraftRestrictionConfiguration
        : IEntityTypeConfiguration<AircraftRestriction>
    {
        public void Configure(EntityTypeBuilder<AircraftRestriction> builder)
        {
            builder.ToTable("AircraftRestrictions");
            builder.HasKey(r => r.Id);

            // ── String columns ────────────────────────────────────────────
            builder.Property(r => r.RestrictionType)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(r => r.Severity)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired()
                .HasDefaultValue("HIGH");

            builder.Property(r => r.Reference)
                .HasColumnType("nvarchar(80)")
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasColumnType("nvarchar(500)")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(r => r.IssuedBy)
                .HasColumnType("nvarchar(80)")
                .HasMaxLength(80)
                .IsRequired(false);

            builder.Property(r => r.Notes)
                .HasColumnType("nvarchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            // ── Audit ─────────────────────────────────────────────────────
            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.CreatedByUserId)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(r => r.IsActive)
                .HasDefaultValue(true);

            // ── FK → Aircraft ─────────────────────────────────────────────
            builder.HasOne(r => r.Aircraft)
                .WithMany()
                .HasForeignKey(r => r.AircraftId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AircraftRestrictions_Aircraft");

            // ── FK → AircraftCertificate (optional) ───────────────────────
            builder.HasOne(r => r.Certificate)
                .WithMany()
                .HasForeignKey(r => r.CertificateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AircraftRestrictions_Certificate");

            // ── Index for DAM Dashboard query ─────────────────────────────
            builder.HasIndex(r => new { r.AircraftId, r.IsActive })
                .HasDatabaseName("IX_AircraftRestrictions_Aircraft_Active");
        }
    }
    
}
