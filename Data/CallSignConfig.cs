using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.SquadronOps.Models;

namespace FRAProject.Data
{
    public partial class FRAContext
    {
       

        private void ConfigureCallSign(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CallSign>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Code).HasMaxLength(20).IsRequired();
                e.HasIndex(c => c.Code).IsUnique(false); // keep false — app enforces scope uniqueness
                e.Property(c => c.Description).HasMaxLength(250);
                e.Property(c => c.IsActive).HasDefaultValue(true);
                e.HasIndex(c => c.BaseId);
                e.HasIndex(c => c.SquadronId);
            });
        }
    }
}