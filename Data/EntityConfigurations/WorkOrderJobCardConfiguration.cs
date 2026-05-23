using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class WorkOrderJobCardConfiguration : IEntityTypeConfiguration<WorkOrderJobCard>
    {
        public void Configure(EntityTypeBuilder<WorkOrderJobCard> builder)
        {
            builder.ToTable("WorkOrderJobCards");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("PENDING");

            builder.Property(x => x.NAJustification)
                .HasMaxLength(500);

            builder.Property(x => x.Observations)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IsMandatory)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(100);

            builder.HasIndex(x => new { x.WorkOrderId, x.JobCardId, x.MaintenanceProgramId })
                .IsUnique();

            builder.HasOne(x => x.WorkOrder)
                .WithMany(x => x.WorkOrderJobCards)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.JobCard)
                .WithMany(x => x.WorkOrderJobCards)
                .HasForeignKey(x => x.JobCardId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MaintenanceProgram)
                .WithMany(x => x.WorkOrderJobCards)
                .HasForeignKey(x => x.MaintenanceProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}