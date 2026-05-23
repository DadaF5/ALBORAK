using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class ProgramJobCardConfiguration : IEntityTypeConfiguration<ProgramJobCard>
    {
        public void Configure(EntityTypeBuilder<ProgramJobCard> builder)
        {
            builder.ToTable("ProgramJobCards");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(100);

            builder.Property(x => x.IsMandatory)
                .HasDefaultValue(true);

            builder.HasIndex(x => new { x.MaintenanceProgramId, x.JobCardId })
                .IsUnique();

            builder.HasOne(x => x.MaintenanceProgram)
                .WithMany(x => x.ProgramJobCards)
                .HasForeignKey(x => x.MaintenanceProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.JobCard)
                .WithMany(x => x.ProgramJobCards)
                .HasForeignKey(x => x.JobCardId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}