using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class InspectionStateConfiguration : IEntityTypeConfiguration<InspectionState>
    {
        public void Configure(EntityTypeBuilder<InspectionState> builder)
        {
            builder.ToTable("InspectionStates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StatusSnapshot)
                .HasMaxLength(20);

            builder.HasIndex(x => new { x.AircraftId, x.InspectionTypeId })
                .IsUnique();

            builder.HasOne(x => x.Aircraft)
                .WithMany()
                .HasForeignKey(x => x.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InspectionType)
                .WithMany(x => x.InspectionStates)
                .HasForeignKey(x => x.InspectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LastWorkOrder)
                .WithMany(x => x.InspectionStatesAsLastWorkOrder)
                .HasForeignKey(x => x.LastWorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}