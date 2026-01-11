using FRAProject.Areas.SquadronOps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class MissionConfiguration: IEntityTypeConfiguration<Mission>
    {
        public void Configure(EntityTypeBuilder<Mission> builder)
        { 
            // Table name
            builder.ToTable("Missions");
            // Primary key
            builder.HasKey(m => m.Id);
            // Properties
            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Code)
                .HasMaxLength(50);

            builder.Property(m => m.Description)
                .HasMaxLength(1000);
            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(m => m.Phase)
                .WithMany(p => p.Missions)
                .HasForeignKey(m => m.PhaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Squadron)
                .WithMany(s => s.Missions)
                .HasForeignKey(m => m.SquadronId)
                .OnDelete(DeleteBehavior.SetNull);
           

        }
    }
}
