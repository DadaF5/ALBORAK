using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class AircraftJobCardStateConfiguration : IEntityTypeConfiguration<AircraftJobCardState>
    {
        public void Configure(EntityTypeBuilder<AircraftJobCardState> builder)
        {
            builder.ToTable("AircraftJobCardStates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(x => new { x.AircraftId, x.JobCardId })
                .IsUnique();

            builder.HasOne(x => x.Aircraft)
                .WithMany()
                .HasForeignKey(x => x.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.JobCard)
                .WithMany(x => x.AircraftJobCardStates)
                .HasForeignKey(x => x.JobCardId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AppliedPlanningRule)
                .WithMany(x => x.AircraftJobCardStates)
                .HasForeignKey(x => x.AppliedPlanningRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LastWorkOrder)
                .WithMany(x => x.AircraftJobCardStatesAsLastWorkOrder)
                .HasForeignKey(x => x.LastWorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}