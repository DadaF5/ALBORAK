using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class InspectionTypeProgramConfiguration : IEntityTypeConfiguration<InspectionTypeProgram>
    {
        public void Configure(EntityTypeBuilder<InspectionTypeProgram> builder)
        {
            builder.ToTable("InspectionTypePrograms");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(100);

            builder.HasIndex(x => new { x.InspectionTypeId, x.MaintenanceProgramId })
                .IsUnique();

            builder.HasOne(x => x.InspectionType)
                .WithMany(x => x.InspectionTypePrograms)
                .HasForeignKey(x => x.InspectionTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MaintenanceProgram)
                .WithMany(x => x.InspectionTypePrograms)
                .HasForeignKey(x => x.MaintenanceProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}