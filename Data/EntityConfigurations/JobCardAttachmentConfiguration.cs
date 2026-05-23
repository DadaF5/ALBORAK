using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class JobCardAttachmentConfiguration : IEntityTypeConfiguration<JobCardAttachment>
    {
        public void Configure(EntityTypeBuilder<JobCardAttachment> builder)
        {
            builder.ToTable("JobCardAttachments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .HasMaxLength(100);

            builder.Property(x => x.UploadedByUserId)
                .HasMaxLength(450);

            builder.HasOne(x => x.JobCard)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.JobCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}