// Data/EntityConfigurations/SquadronConfiguration.cs
//
// NEW (2026-08-29). Squadron had NO IEntityTypeConfiguration<Squadron> at
// all before this — confirmed by reading the real FRAContext.cs: the only
// Squadron-side relationship configured anywhere is Wing -> Squadron, done
// inline from the Wing side in OnModelCreating
// (modelBuilder.Entity<Wing>().HasMany(w => w.Squadrons)...). Everything
// else about Squadron has always been configured by EF Core convention.
//
// This file is scoped to exactly the one thing added this session:
// Squadron.CurrentBaseId -> Base. It does not touch, override, or
// duplicate anything already configured elsewhere.
using FRAProject.Areas.SquadronOps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class SquadronConfiguration : IEntityTypeConfiguration<Squadron>
    {
        public void Configure(EntityTypeBuilder<Squadron> builder)
        {
            // Squadron's CURRENT operating base (may differ from its Wing's
            // administrative/scope base via Wing -> Department -> Base —
            // see the long comment on Squadron.CurrentBaseId itself for the
            // full "F16 home base is 6th AFB, but Squadron 312 operates
            // from 2nd AFB" example).
            //
            // Explicit Restrict here matches this codebase's own house
            // style: nearly every optional FK in FRAContext.cs sets
            // OnDelete(DeleteBehavior.Restrict) explicitly rather than
            // relying on EF's convention default for optional
            // relationships (see Odv -> Base in OdvConfiguration.cs for
            // the closest analog — same "optional FK to Base" shape).
            // Without this line, EF's convention default for an optional
            // relationship behaves differently (effectively SET NULL) if a
            // Base row is ever deleted — Restrict is safer and consistent
            // with every other FK into Base in this codebase.
            builder.HasOne(s => s.CurrentBase)
                .WithMany()
                .HasForeignKey(s => s.CurrentBaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
