using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class WorkOrderJobCardSignOffConfiguration : IEntityTypeConfiguration<WorkOrderJobCardSignOff>
    {
        public void Configure(EntityTypeBuilder<WorkOrderJobCardSignOff> builder)
        {
            builder.ToTable("WorkOrderJobCardSignOffs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Level)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.IsMandatory)
                .HasDefaultValue(true);

            builder.Property(x => x.SignedByUserId)
                .HasMaxLength(450);

            builder.Property(x => x.Remarks)
                .HasMaxLength(500);

            builder.HasIndex(x => new { x.WorkOrderJobCardId, x.Level })
                .IsUnique();

            builder.HasOne(x => x.WorkOrderJobCard)
                .WithMany(x => x.SignOffs)
                .HasForeignKey(x => x.WorkOrderJobCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SignedByUser)
                .WithMany()
                .HasForeignKey(x => x.SignedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}