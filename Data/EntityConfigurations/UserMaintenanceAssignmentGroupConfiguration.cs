using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class UserMaintenanceAssignmentGroupConfiguration : IEntityTypeConfiguration<UserMaintenanceAssignmentGroup>
    {
        public void Configure(EntityTypeBuilder<UserMaintenanceAssignmentGroup> builder)
        {
            builder.ToTable("UserMaintenanceAssignmentGroups", schema: "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EffectiveFrom)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.EffectiveTo)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(x => x.Reason)
                .HasMaxLength(300)
                .IsRequired(false);

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            // Prevent duplicate active extra-scope rows for same assignment + group
            builder.HasIndex(x => new { x.AssignmentId, x.AcMainGroupId })
                .HasDatabaseName("IX_UserMaintenanceAssignmentGroup_Assignment_Group");

            // FK: UserMaintenanceAssignment (cascade: delete rows when assignment is deleted)
            builder.HasOne(x => x.Assignment)
                .WithMany(a => a.AdditionalGroups)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK: AcMainGroup (restrict delete)
            builder.HasOne(x => x.AcMainGroup)
                .WithMany()
                .HasForeignKey(x => x.AcMainGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
