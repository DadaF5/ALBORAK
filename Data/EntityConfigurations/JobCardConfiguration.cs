using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class JobCardConfiguration : IEntityTypeConfiguration<JobCard>
    {
        public void Configure(EntityTypeBuilder<JobCard> builder)
        {
            builder.ToTable("JobCards");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AtaCode)
                .HasMaxLength(20);

            builder.Property(x => x.CardCode)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Specialty)
                .HasMaxLength(20);

            builder.Property(x => x.ToReference)
                .HasMaxLength(100);

            builder.Property(x => x.DocReference)
                .HasMaxLength(100);

            builder.Property(x => x.Edition)
                .HasMaxLength(30);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(100);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.AllocatedTimeMinutes)
                .HasDefaultValue(0);

            builder.HasIndex(x => new { x.AcTypeId, x.CardCode })
                .IsUnique();

            builder.HasOne(x => x.AcType)
                .WithMany()
                .HasForeignKey(x => x.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}