using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class SortieCrewConfiguration : IEntityTypeConfiguration<SortieCrew>
    {
        public void Configure(EntityTypeBuilder<SortieCrew> entity)
        {
            entity.HasKey(sc => sc.Id);

            entity.HasOne(sc => sc.Sortie)
                  .WithMany(s => s.SortieCrews)
                  .HasForeignKey(sc => sc.SortieId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sc => sc.CrewMember)
                  .WithMany()
                  .HasForeignKey(sc => sc.CrewMemberId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(c => c.Role).HasMaxLength(100);
            entity.Property(c => c.IsPrimary).HasDefaultValue(false);
            entity.Property(c => c.Remarks).HasMaxLength(1000);

            entity.HasIndex(sc => sc.SortieId);
        }
    }

}
