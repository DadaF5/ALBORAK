using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Data.Configurations
{
    /// <summary>
    /// NEW — backs AircraftReading (see that model's own doc comment for
    /// why this table exists). Unique per (AircraftId, DimensionTypeId) —
    /// this is a CURRENT-value table, one row per counter per aircraft, not
    /// a log, so a second write for the same pair must update the existing
    /// row (see IAircraftReadingProvider.IncrementReadingAsync/
    /// SetReadingAsync — both upsert against this same index).
    /// </summary>
    public class AircraftReadingConfiguration : IEntityTypeConfiguration<AircraftReading>
    {
        public void Configure(EntityTypeBuilder<AircraftReading> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasIndex(r => new { r.AircraftId, r.DimensionTypeId })
                .IsUnique()
                .HasDatabaseName("IX_AircraftReadings_Aircraft_Dimension");

            builder.HasOne(r => r.Aircraft)
                .WithMany()
                .HasForeignKey(r => r.AircraftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict, not Cascade — a DimensionType is soft-deleted
            // (IsActive) in this module's convention, never hard-deleted;
            // Restrict is just a backstop against ever trying to hard-delete
            // one that already has real aircraft readings against it.
            builder.HasOne(r => r.DimensionType)
                .WithMany()
                .HasForeignKey(r => r.DimensionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
