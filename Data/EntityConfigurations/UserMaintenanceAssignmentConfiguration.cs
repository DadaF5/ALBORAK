using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class UserMaintenanceAssignmentConfiguration : IEntityTypeConfiguration<UserMaintenanceAssignment>
    {
        public void Configure(EntityTypeBuilder<UserMaintenanceAssignment> builder)
        {
            builder.ToTable("UserMaintenanceAssignments", schema: "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x => x.EffectiveFrom)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.EffectiveTo)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // Index for active-assignment lookup (most common query)
            builder.HasIndex(x => new { x.UserId, x.IsActive })
                .HasDatabaseName("IX_UserMaintenanceAssignment_UserId_IsActive");

            // FK: ApplicationUser (restrict delete — user should be deactivated, not deleted)
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Base (restrict delete)
            builder.HasOne(x => x.Base)
                .WithMany()
                .HasForeignKey(x => x.BaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: AcMainGroup (restrict delete)
            builder.HasOne(x => x.AcMainGroup)
                .WithMany()
                .HasForeignKey(x => x.AcMainGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: MaintenanceRole (restrict delete)
            builder.HasOne(x => x.MaintenanceRole)
                .WithMany(r => r.UserAssignments)
                .HasForeignKey(x => x.MaintenanceRoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
