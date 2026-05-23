using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class InspectionTypeConfiguration : IEntityTypeConfiguration<InspectionType>
    {
        public void Configure(EntityTypeBuilder<InspectionType> builder)
        {
            builder.ToTable("InspectionTypes");

            builder.HasKey(x => x.Id);

            // LookupBase fields
            builder.Property(x => x.Code)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue((byte)99);

            // Specific fields
            builder.Property(x => x.Kind)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("PLANNED");

            builder.Property(x => x.CalendarUnit)
                .HasMaxLength(10);

            builder.Property(x => x.ToleranceCalendarUnit)
                .HasMaxLength(10);

            builder.HasIndex(x => new { x.AcTypeId, x.Code })
                .IsUnique();

            builder.HasOne(x => x.AcType)
                .WithMany()
                .HasForeignKey(x => x.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.NextInspectionType)
                .WithMany()
                .HasForeignKey(x => x.NextInspectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}