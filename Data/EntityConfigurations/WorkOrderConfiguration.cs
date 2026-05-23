using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.WONumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.WOType)
                .HasMaxLength(5)
                .IsRequired()
                .HasDefaultValue("F12");

            builder.Property(x => x.WOKind)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("PLANNED");

            builder.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("DRAFT");

            builder.Property(x => x.OpenedByUserId)
                .HasMaxLength(450);

            builder.Property(x => x.ClosedByUserId)
                .HasMaxLength(450);

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.WONumber)
                .IsUnique();

            builder.HasOne(x => x.Aircraft)
                .WithMany()
                .HasForeignKey(x => x.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InspectionType)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.InspectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OpenedByUser)
                .WithMany()
                .HasForeignKey(x => x.OpenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}