using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class MaintenanceProgramConfiguration : IEntityTypeConfiguration<MaintenanceProgram>
    {
        public void Configure(EntityTypeBuilder<MaintenanceProgram> builder)
        {
            builder.ToTable("MaintenancePrograms");

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
            builder.Property(x => x.DocReference)
                .HasMaxLength(100);

            builder.Property(x => x.Edition)
                .HasMaxLength(30);

            builder.HasIndex(x => new { x.AcTypeId, x.Code })
                .IsUnique();

            builder.HasOne(x => x.AcType)
                .WithMany()
                .HasForeignKey(x => x.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}