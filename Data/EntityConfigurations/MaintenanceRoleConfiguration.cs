using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class MaintenanceRoleConfiguration : IEntityTypeConfiguration<MaintenanceRole>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRole> builder)
        {
            builder.ToTable("MaintenanceRoles", schema: "dbo");

            builder.HasKey(x => x.Id);

            // LookupBase properties
            builder.Property(x => x.Code)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(250)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue((byte)99);

            // Unique Code
            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasDatabaseName("UX_MaintenanceRole_Code");

            // Seed predefined roles
            builder.HasData(
                new MaintenanceRole { Id = 1, Code = "TECH",       Name = "Technician",       Description = "Maintenance technician. Signs off own task cards.", SortOrder = 1 },
                new MaintenanceRole { Id = 2, Code = "BASE_SUP",   Name = "Base Supervisor",  Description = "Supervises maintenance within one base and aircraft group scope.", SortOrder = 2 },
                new MaintenanceRole { Id = 3, Code = "MASTER_SUP", Name = "Master Supervisor",Description = "Full read/write access across all bases and groups.", SortOrder = 3 }
            );
        }
    }
}
