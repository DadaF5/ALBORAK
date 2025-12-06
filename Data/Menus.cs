using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public partial class FRAContext
    {
        // Add the DbSet so EF tracks menu items
        //public DbSet<MenuItem> MenuItems { get; set; } = null!;

        // Call this from FRAContext.OnModelCreating(modelBuilder)
        public void ConfigureMenus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuItem>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Title).HasMaxLength(200).IsRequired();
                e.Property(m => m.IconClass).HasMaxLength(200);
                e.Property(m => m.Controller).HasMaxLength(100);
                e.Property(m => m.Action).HasMaxLength(100);
                e.Property(m => m.Url).HasMaxLength(500);
                e.Property(m => m.Roles).HasMaxLength(200);

                e.HasIndex(m => new { m.ParentId, m.SortOrder });
            });
        }
    }
}
