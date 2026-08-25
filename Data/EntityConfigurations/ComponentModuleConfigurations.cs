using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Data.Configurations
{
    // Register each of these in FRAContext.OnModelCreating, same as the
    // existing ApplyConfiguration(new XConfiguration()) block, e.g.:
    //
    //   modelBuilder.ApplyConfiguration(new ComponentPositionConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentTypeConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentLifeLimitProfileConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentLifeLimitStageConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentTypePositionConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentEventConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentLifeStatusConfiguration());
    //   modelBuilder.ApplyConfiguration(new ComponentTypeSlotConfiguration());              // NEW — hierarchy (slot definitions)
    //   modelBuilder.ApplyConfiguration(new ComponentTypeSubAssemblySlotConfiguration());  // NEW — hierarchy (per-PN eligibility)
    //   modelBuilder.ApplyConfiguration(new ComponentInitialReadingConfiguration());       // NEW (Revision 12) — opening reading, DO NOT forget this line (see Revision 8's note: a missed ApplyConfiguration line here silently falls back to EF default conventions instead of the Restrict/unique-index rules below)
    //   modelBuilder.ApplyConfiguration(new ComponentLifeLimitDimensionTypeConfiguration());   // NEW (Revision 13) — generic dimension model, DO NOT forget any of these 5 lines
    //   modelBuilder.ApplyConfiguration(new ComponentLifeLimitStageDimensionConfiguration());  // NEW (Revision 13)
    //   modelBuilder.ApplyConfiguration(new ComponentLifeStatusDimensionConfiguration());      // NEW (Revision 13)
    //   modelBuilder.ApplyConfiguration(new ComponentEventReadingConfiguration());             // NEW (Revision 13)
    //   modelBuilder.ApplyConfiguration(new ComponentInitialReadingValueConfiguration());      // NEW (Revision 13)
    //   modelBuilder.ApplyConfiguration(new ComponentReferenceBasisConfiguration());            // NEW — reference-basis lookup, DO NOT forget (same failure mode as the Revision 13 lines above: a missed line here silently falls back to EF default conventions instead of the unique-Code index below)
    //   modelBuilder.ApplyConfiguration(new ComponentDerogationConfiguration());                // NEW — Derogation implementation pass, DO NOT forget (same failure mode as above — a missed line here silently falls back to EF default conventions instead of the Restrict-delete rules below)

    public class ComponentPositionConfiguration : IEntityTypeConfiguration<ComponentPosition>
    {
        public void Configure(EntityTypeBuilder<ComponentPosition> builder)
        {
            builder.HasIndex(p => new { p.AcTypeId, p.Code }).IsUnique();

            builder.HasOne(p => p.AcType)
                .WithMany() // add an AcType.ComponentPositions collection if you want the reverse nav; not required
                .HasForeignKey(p => p.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Ata)
                .WithMany()
                .HasForeignKey(p => p.AtaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ComponentTypeConfiguration : IEntityTypeConfiguration<ComponentType>
    {
        public void Configure(EntityTypeBuilder<ComponentType> builder)
        {
            builder.HasIndex(t => t.PartNumber).IsUnique();

            builder.HasOne(t => t.Ata)
                .WithMany()
                .HasForeignKey(t => t.AtaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AircraftManufacturer)
                .WithMany()
                .HasForeignKey(t => t.AircraftManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ComponentLifeLimitProfileConfiguration : IEntityTypeConfiguration<ComponentLifeLimitProfile>
    {
        public void Configure(EntityTypeBuilder<ComponentLifeLimitProfile> builder)
        {
            builder.HasOne(p => p.ComponentType)
                .WithMany(t => t.LifeLimitProfiles)
                .HasForeignKey(p => p.ComponentTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // At most one active PN_BASED default per ComponentType. Filtered
            // index (SQL Server syntax) — drop the .HasFilter if your EF/DB
            // version doesn't support it and enforce this in the service layer
            // instead (ComponentLifeLimitProfileService should already do this
            // as a belt-and-suspenders check).
            builder.HasIndex(p => p.ComponentTypeId)
                .HasFilter("[ApplicabilityRuleType] = 0 AND [IsActive] = 1")
                .IsUnique();
        }
    }

    public class ComponentLifeLimitStageConfiguration : IEntityTypeConfiguration<ComponentLifeLimitStage>
    {
        public void Configure(EntityTypeBuilder<ComponentLifeLimitStage> builder)
        {
            builder.HasOne(s => s.ComponentLifeLimitProfile)
                .WithMany(p => p.Stages)
                .HasForeignKey(s => s.ComponentLifeLimitProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new { s.ComponentLifeLimitProfileId, s.SequenceOrder }).IsUnique();
        }
    }

    public class ComponentTypePositionConfiguration : IEntityTypeConfiguration<ComponentTypePosition>
    {
        public void Configure(EntityTypeBuilder<ComponentTypePosition> builder)
        {
            builder.HasIndex(x => new { x.ComponentTypeId, x.ComponentPositionId }).IsUnique();

            builder.HasOne(x => x.ComponentType)
                .WithMany(t => t.ComponentTypePositions)
                .HasForeignKey(x => x.ComponentTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ComponentPosition)
                .WithMany(p => p.ComponentTypePositions)
                .HasForeignKey(x => x.ComponentPositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ComponentConfiguration : IEntityTypeConfiguration<Component>
    {
        public void Configure(EntityTypeBuilder<Component> builder)
        {
            builder.HasIndex(c => new { c.ComponentTypeId, c.SerialNumber }).IsUnique();

            builder.HasOne(c => c.ComponentType)
                .WithMany(t => t.Components)
                .HasForeignKey(c => c.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.StockBase)
                .WithMany()
                .HasForeignKey(c => c.StockBaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CurrentAircraft)
                .WithMany() // add Aircraft.CurrentComponents collection if wanted
                .HasForeignKey(c => c.CurrentAircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CurrentPosition)
                .WithMany(p => p.Components)
                .HasForeignKey(c => c.CurrentPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ComponentLifeStatus)
                .WithOne(s => s!.Component)
                .HasForeignKey<ComponentLifeStatus>(s => s.ComponentId)
                .OnDelete(DeleteBehavior.Cascade);

            // NEW — recursive parent-child assembly tree. Restrict, not Cascade:
            // deleting a parent Component must not silently delete its attached
            // children (SQL Server would also reject a self-referencing cascade
            // path here as a multiple-cascade-paths error if this were Cascade).
            // Application flow: DetachFromParentAsync every child first.
            builder.HasOne(c => c.ParentComponent)
                .WithMany(c => c.ChildComponents)
                .HasForeignKey(c => c.ParentComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ComponentEventConfiguration : IEntityTypeConfiguration<ComponentEvent>
    {
        public void Configure(EntityTypeBuilder<ComponentEvent> builder)
        {
            builder.HasOne(e => e.Component)
                .WithMany(c => c.ComponentEvents)
                .HasForeignKey(e => e.ComponentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Aircraft)
                .WithMany()
                .HasForeignKey(e => e.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Position)
                .WithMany(p => p.ComponentEvents)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.LinkedWorkOrder)
                .WithMany() // add WorkOrder.ComponentEvents collection if wanted
                .HasForeignKey(e => e.LinkedWorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.PerformedByUser)
                .WithMany()
                .HasForeignKey(e => e.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // NEW — RelatedParentComponentId (AttachToParent/DetachFromParent
            // events). Restrict, and deliberately WithMany() with no reverse
            // nav collection on Component — this FK is queried the other
            // direction (by RelatedParentComponentId, from the parent's own
            // Details/history view), a second collection here isn't needed
            // and would just be one more nav to keep in sync.
            builder.HasOne(e => e.RelatedParentComponent)
                .WithMany()
                .HasForeignKey(e => e.RelatedParentComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Never edited/deleted post-creation by app code — no concurrency
            // token needed, but do not expose an Edit/Delete action on this
            // entity in the controller/UI.
            builder.HasIndex(e => new { e.ComponentId, e.EventDate });
        }
    }

    /// <summary>NEW — hierarchy slot DEFINITIONS (design doc §2). Capacity (MaxCount) lives here, once per physical slot — see ComponentTypeSlot doc comment for why this was split out of the eligibility table.</summary>
    public class ComponentTypeSlotConfiguration : IEntityTypeConfiguration<ComponentTypeSlot>
    {
        public void Configure(EntityTypeBuilder<ComponentTypeSlot> builder)
        {
            // One SlotCode per parent ComponentType — "DEEC" means one specific
            // physical slot on a given engine PN, not a free-text duplicate.
            builder.HasIndex(x => new { x.ParentComponentTypeId, x.SlotCode }).IsUnique();

            builder.HasOne(x => x.ParentComponentType)
                .WithMany(t => t.ChildSlots)
                .HasForeignKey(x => x.ParentComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict); // matches every other ComponentType reference in this module — catalog rows aren't cascade-deleted
        }
    }

    /// <summary>NEW — hierarchy per-PN eligibility rows, pointing back at a ComponentTypeSlot (design doc §2).</summary>
    public class ComponentTypeSubAssemblySlotConfiguration : IEntityTypeConfiguration<ComponentTypeSubAssemblySlot>
    {
        public void Configure(EntityTypeBuilder<ComponentTypeSubAssemblySlot> builder)
        {
            // One (slot, eligible child PN) combination only once — several
            // DIFFERENT child PNs may still share the same SlotId to express
            // "either of these two PNs fits this slot" (interchangeable parts
            // from different manufacturers), same shape as ComponentTypePosition.
            builder.HasIndex(x => new { x.SlotId, x.ChildComponentTypeId }).IsUnique();

            // Cascade from the slot: an eligibility row has no meaning once its
            // slot definition is gone — deleting a ComponentTypeSlot should take
            // its eligible-PN list with it, no orphaned rows. Single cascade
            // path (ComponentTypeSlot -> this table only), so this doesn't
            // collide with the Restrict below (ComponentType -> ComponentTypeSlot
            // is also Restrict, not Cascade, so there's no multi-path ambiguity
            // for SQL Server to reject).
            builder.HasOne(x => x.Slot)
                .WithMany(s => s.EligibleChildren)
                .HasForeignKey(x => x.SlotId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChildComponentType)
                .WithMany(t => t.EligibleAsChildIn)
                .HasForeignKey(x => x.ChildComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ComponentLifeStatusConfiguration : IEntityTypeConfiguration<ComponentLifeStatus>
    {
        public void Configure(EntityTypeBuilder<ComponentLifeStatus> builder)
        {
            builder.HasIndex(s => s.ComponentId).IsUnique();

            builder.HasOne(s => s.MatchedLifeLimitProfile)
                .WithMany()
                .HasForeignKey(s => s.MatchedLifeLimitProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            // NEW (Revision 13) — denormalized "headline" dimension for list
            // views. SetNull, not Restrict: a DimensionType being deactivated/
            // removed should never block deleting a component's life status.
            builder.HasOne(s => s.DrivingDimensionType)
                .WithMany()
                .HasForeignKey(s => s.DrivingDimensionTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    /// <summary>NEW (Revision 13) — see ComponentLifeLimitDimensionType.cs for the "why".</summary>
    public class ComponentLifeLimitDimensionTypeConfiguration : IEntityTypeConfiguration<ComponentLifeLimitDimensionType>
    {
        public void Configure(EntityTypeBuilder<ComponentLifeLimitDimensionType> builder)
        {
            builder.HasIndex(d => d.Code).IsUnique();

            // NEW — AcMainGroup scoping (null = universal). Restrict, same
            // convention as every other lookup FK in this module: deleting an
            // AcMainGroup that a dimension type still points at should fail
            // loudly, not silently null out the scoping and make the
            // dimension universal again.
            builder.HasOne(d => d.AcMainGroup)
                .WithMany()
                .HasForeignKey(d => d.AcMainGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>NEW — reference-basis lookup, see ComponentReferenceBasis.cs for the "why".</summary>
    public class ComponentReferenceBasisConfiguration : IEntityTypeConfiguration<ComponentReferenceBasis>
    {
        public void Configure(EntityTypeBuilder<ComponentReferenceBasis> builder)
        {
            builder.HasIndex(b => b.Code).IsUnique();
        }
    }

    /// <summary>NEW (Revision 13) — see ComponentLifeLimitStageDimension.cs.</summary>
    public class ComponentLifeLimitStageDimensionConfiguration : IEntityTypeConfiguration<ComponentLifeLimitStageDimension>
    {
        public void Configure(EntityTypeBuilder<ComponentLifeLimitStageDimension> builder)
        {
            builder.HasIndex(x => new { x.ComponentLifeLimitStageId, x.DimensionTypeId }).IsUnique();

            builder.HasOne(x => x.ComponentLifeLimitStage)
                .WithMany(s => s.Dimensions)
                .HasForeignKey(x => x.ComponentLifeLimitStageId)
                .OnDelete(DeleteBehavior.Cascade); // deleted alongside its stage — ReplaceStagesAsync already deletes/reinserts the whole stage set on every save.

            builder.HasOne(x => x.DimensionType)
                .WithMany()
                .HasForeignKey(x => x.DimensionTypeId)
                .OnDelete(DeleteBehavior.Restrict); // catalog/lookup row — never cascade-deleted, same convention as every other lookup FK in this module.

            // NEW — reference-basis pick per (stage, dimension) row. Restrict
            // + nullable: a basis being deactivated/removed should be
            // prevented while any stage row still points at it, not silently
            // null the pick back to profile-level default (SetNull would
            // mask a real "this basis is still in use" problem).
            builder.HasOne(x => x.ReferenceBasis)
                .WithMany()
                .HasForeignKey(x => x.ReferenceBasisId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>NEW (Revision 13) — see ComponentLifeStatusDimension.cs.</summary>
    public class ComponentLifeStatusDimensionConfiguration : IEntityTypeConfiguration<ComponentLifeStatusDimension>
    {
        public void Configure(EntityTypeBuilder<ComponentLifeStatusDimension> builder)
        {
            builder.HasIndex(x => new { x.ComponentLifeStatusId, x.DimensionTypeId }).IsUnique();

            builder.HasOne(x => x.ComponentLifeStatus)
                .WithMany(s => s.Dimensions)
                .HasForeignKey(x => x.ComponentLifeStatusId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.DimensionType)
                .WithMany()
                .HasForeignKey(x => x.DimensionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>NEW (Revision 13) — see ComponentEventReading.cs.</summary>
    public class ComponentEventReadingConfiguration : IEntityTypeConfiguration<ComponentEventReading>
    {
        public void Configure(EntityTypeBuilder<ComponentEventReading> builder)
        {
            builder.HasIndex(x => new { x.ComponentEventId, x.DimensionTypeId }).IsUnique();

            builder.HasOne(x => x.ComponentEvent)
                .WithMany(e => e.Readings)
                .HasForeignKey(x => x.ComponentEventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.DimensionType)
                .WithMany()
                .HasForeignKey(x => x.DimensionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>NEW (Revision 13) — see ComponentInitialReadingValue.cs.</summary>
    public class ComponentInitialReadingValueConfiguration : IEntityTypeConfiguration<ComponentInitialReadingValue>
    {
        public void Configure(EntityTypeBuilder<ComponentInitialReadingValue> builder)
        {
            builder.HasIndex(x => new { x.ComponentInitialReadingId, x.DimensionTypeId }).IsUnique();

            builder.HasOne(x => x.ComponentInitialReading)
                .WithMany(r => r.Values)
                .HasForeignKey(x => x.ComponentInitialReadingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.DimensionType)
                .WithMany()
                .HasForeignKey(x => x.DimensionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>NEW (Revision 12) — see ComponentInitialReading.cs for the "why".</summary>
    public class ComponentInitialReadingConfiguration : IEntityTypeConfiguration<ComponentInitialReading>
    {
        public void Configure(EntityTypeBuilder<ComponentInitialReading> builder)
        {
            builder.HasIndex(r => r.ComponentId).IsUnique();

            builder.HasOne(r => r.Component)
                .WithOne(c => c!.InitialReading)
                .HasForeignKey<ComponentInitialReading>(r => r.ComponentId)
                .OnDelete(DeleteBehavior.Cascade); // pure 1:1 detail row, same as ComponentLifeStatus — no independent lifecycle from its Component.

            builder.HasOne(r => r.RecordedByUser)
                .WithMany()
                .HasForeignKey(r => r.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>NEW (Derogation implementation pass) — see ComponentDerogation.cs for the full design.</summary>
    public class ComponentDerogationConfiguration : IEntityTypeConfiguration<ComponentDerogation>
    {
        public void Configure(EntityTypeBuilder<ComponentDerogation> builder)
        {
            // Catalog/lookup FKs — Restrict, same convention as every other
            // lookup reference in this module (ComponentType/DimensionType
            // rows are never cascade-deleted out from under a derogation
            // that still cites them).
            builder.HasOne(d => d.ComponentType)
                .WithMany(t => t.Derogations)
                .HasForeignKey(d => d.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.DimensionType)
                .WithMany()
                .HasForeignKey(d => d.DimensionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-FK correction chain (see ComponentDerogation.SupersedesDerogationId
            // doc comment) — Restrict, no reverse-nav collection, same
            // pattern as ComponentEvent.RelatedParentComponentId.
            builder.HasOne(d => d.SupersedesDerogation)
                .WithMany()
                .HasForeignKey(d => d.SupersedesDerogationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // NEW — Void action's audit trail (who voided, not who created).
            builder.HasOne(d => d.VoidedByUser)
                .WithMany()
                .HasForeignKey(d => d.VoidedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Append-only log — rows are never edited/deleted through the UI
            // (Void flips IsActive + stamps VoidedAt/VoidedBy/VoidReason,
            // it does not touch any other field) — no concurrency token
            // needed, same discipline as ComponentEvent.
            builder.HasIndex(d => new { d.ComponentTypeId, d.IssuedDate });

            // decimal(9,2) is generous enough for both a whole-number percent
            // (e.g. 20) and an absolute months/hours value (e.g. 19, 120)
            // while leaving room for a fractional value if one ever shows up.
            builder.Property(d => d.Value).HasColumnType("decimal(9,2)");
        }
    }
}
