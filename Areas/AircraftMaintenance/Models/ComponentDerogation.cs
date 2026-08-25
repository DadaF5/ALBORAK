using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Models; // ApplicationUser — same confirmed namespace ComponentEvent.cs uses

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW — a formal, documented, engineering/manufacturer-approved
    /// extension (or, rarer, reduction) of a life-limit dimension for a
    /// ComponentType, real-world-driven by Dadda's own example: a safety-
    /// critical part (ejection-seat main cartridge initiator) reaches its
    /// life limit, no spare exists worldwide, and a deep analysis with the
    /// manufacturer extends the limit under a reference directive.
    ///
    /// APPEND-ONLY, same convention as ComponentEvent — never edited after
    /// creation. A correction is a NEW ComponentDerogation row, optionally
    /// pointing back at the one it corrects/voids via SupersedesDerogationId
    /// — never an in-place edit of the original (audit trail must survive).
    ///
    /// CONFIRMED CALCULATION MODEL (see claude/ALBORAK_Legacy_Migration_
    /// Discussion.md, "Derogation" section, for the full worked example):
    ///
    ///   effectiveLimit = OriginalBaseValue + Σ(extensionAmount_i)
    ///
    ///   extensionAmount_i =
    ///       (Direction == Extension ? +1 : -1) * Value_i                          (Mode = Absolute)
    ///       (Direction == Extension ? +1 : -1) * OriginalBaseValue * Value_i/100   (Mode = PercentOfInterval)
    ///
    /// Percentage mode is ALWAYS anchored to the immutable ORIGINAL
    /// manufacturer-issued value (the profile stage's own Interval/BandEnd
    /// at the time this derogation targets it), never to a running/already-
    /// derogated value — confirmed by Dadda's own worked example (two
    /// successive absolute extensions: +24 months, then +12 months, both
    /// added on top, order-independent for the math even though
    /// chronological order must still be preserved for the audit trail).
    ///
    /// WIRED (see ComponentLifeStatusCalculator) — every active (IsActive),
    /// non-expired (EffectiveUntil), applicability-matched derogation on a
    /// dimension now shifts that dimension's TargetStageType checkpoint when
    /// ComponentLifeStatus is recomputed; ComponentLifeStatus.HasActiveDerogation
    /// surfaces this in the UI (Index/DueList/Details) so a "Restant" figure
    /// that already reflects a derogation is never silently indistinguishable
    /// from the raw manufacturer schedule. KNOWN LIMITATION: IsConditional
    /// derogations are applied unconditionally — there is no automated check
    /// of whether the follow-up condition (e.g. a repeat inspection) is still
    /// being honored; ConditionDescription remains informational only.
    /// </summary>
    [Table("ComponentDerogations", Schema = "dbo")]
    public class ComponentDerogation
    {
        public int Id { get; set; }

        [Required]
        public int ComponentTypeId { get; set; }
        [ForeignKey(nameof(ComponentTypeId))]
        public virtual ComponentType? ComponentType { get; set; }

        /// <summary>Which tracked parameter this derogation extends/reduces — FH, Cycles, a calendar dimension (incl. the new CALENDAR_MONTHS/CALENDAR_YEARS — see ComponentLifeLimitDimensionUnit), etc. Reuses the Revision 13 generic dimension catalog, same "seeded row, not a migration" philosophy — no new lookup invented for this.</summary>
        [Required]
        public int DimensionTypeId { get; set; }
        [ForeignKey(nameof(DimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DimensionType { get; set; }

        /// <summary>Which stage/aspect of the life-limit schedule this derogation targets — the recurring Overhaul interval, or the final Retirement limit. A derogation always targets exactly one.</summary>
        [Required]
        public ComponentLifeLimitStageType TargetStageType { get; set; } = ComponentLifeLimitStageType.Retirement;

        /// <summary>Scope this derogation applies to. Reuses ApplicabilityRuleType (same PnBased/Specific/RangeFrom/RangeTo as ComponentLifeLimitProfile) PLUS the new Lot option — Dadda confirmed derogations can be PN-wide, Lot-wise, or S/N-series-wise.</summary>
        [Required]
        public ApplicabilityRuleType ApplicabilityRuleType { get; set; } = ApplicabilityRuleType.PnBased;

        /// <summary>Required when ApplicabilityRuleType = Specific.</summary>
        [StringLength(60)]
        public string? SerialNumber { get; set; }

        /// <summary>Required when ApplicabilityRuleType = RangeFrom/RangeTo.</summary>
        [StringLength(20)]
        public string? SerialNumberPrefix { get; set; }

        /// <summary>Numeric part of the serial, as text — same convention as ComponentLifeLimitProfile.SerialBoundary.</summary>
        [StringLength(30)]
        public string? SerialBoundary { get; set; }

        /// <summary>Required when ApplicabilityRuleType = Lot — matched against Component.LotReference.</summary>
        [StringLength(60)]
        public string? LotReference { get; set; }

        /// <summary>Absolute (same unit as the targeted stage's Interval/BandEnd) or PercentOfInterval (whole-number percent of the ORIGINAL value — see class doc comment). Reuses ComponentToleranceType for architectural consistency with the stage/tolerance model rather than inventing a new enum.</summary>
        [Required]
        public ComponentToleranceType Mode { get; set; } = ComponentToleranceType.Absolute;

        /// <summary>NEW — explicit Extension-vs-Reduction (see DerogationDirection doc comment for why this isn't just the sign of Value). Value below is always a positive magnitude.</summary>
        [Required]
        public DerogationDirection Direction { get; set; } = DerogationDirection.Extension;

        /// <summary>Positive magnitude — months/hours/percent points/etc. per Mode. Never negative; use Direction for reductions.</summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        /// <summary>Free-text directive/document reference — e.g. "TO N 4517/INSP FRA/STQ DU 12/08/2023" or "1F-5E/F-6". Deliberately kept as free text, not a constrained lookup: real BAFRA/RMAF reference numbers don't follow one fixed external format, and forcing one would either reject real data or force a fake normalization.</summary>
        [Required, StringLength(300)]
        public string Reference { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        public DateOnly IssuedDate { get; set; }

        /// <summary>NEW — optional expiry (a derogation that is time-bound rather than permanent). Null = no expiry, the common case for BAFRA's real data seen so far.</summary>
        public DateOnly? EffectiveUntil { get; set; }

        /// <summary>NEW — USAF TCTO-style urgency tiering (see DerogationTier doc comment). Nullable: most real/historical derogations won't have this classified.</summary>
        public DerogationTier? Tier { get; set; }

        /// <summary>NEW — who/what authority approved this derogation (e.g. a rank+name, an engineering board, "STQ/DTA"). Free text, not a FK to a user/role table — the approving authority is very often not an AL-BORAK application user at all (manufacturer engineering, an external board).</summary>
        [StringLength(200)]
        public string? ApprovalAuthority { get; set; }

        /// <summary>NEW — what justified the extension (visual inspection / certified lab test / restorative action / manufacturer analysis — mirrors the DoD 4140.27 Shelf-Life Extension Program's basis for extending a Type II item). Free text.</summary>
        [StringLength(500)]
        public string? SupportingEvidence { get; set; }

        /// <summary>NEW — true if this derogation's validity depends on a follow-up condition (e.g. a repeat inspection at a set interval) rather than being an unconditional flat extension. See ConditionDescription.</summary>
        public bool IsConditional { get; set; } = false;

        /// <summary>Required (service-level, not DB-level) when IsConditional = true — describes the condition (e.g. "revalidated by visual inspection every 6 months until next overhaul").</summary>
        [StringLength(500)]
        public string? ConditionDescription { get; set; }

        /// <summary>
        /// NEW — optional explicit link to the derogation this one corrects
        /// or voids, preserving the append-only convention (see class doc
        /// comment) while giving an explicit chain instead of relying only
        /// on chronology. Self-FK, Restrict delete, no reverse-nav
        /// collection needed (queried by SupersedesDerogationId directly
        /// when displaying a derogation's history, same convention as
        /// ComponentEvent.RelatedParentComponentId).
        /// </summary>
        public int? SupersedesDerogationId { get; set; }
        [ForeignKey(nameof(SupersedesDerogationId))]
        public virtual ComponentDerogation? SupersedesDerogation { get; set; }

        /// <summary>
        /// NEW — Void action now ships (VoidDerogation). Distinct from
        /// SupersedesDerogationId: a superseding derogation is a NEW real
        /// fact (the authority granted a further extension), so the original
        /// stays IsActive AND is preserved verbatim. Voiding is for a pure
        /// data-entry mistake — nothing like it ever really happened — so
        /// the row is kept (never hard-deleted, same "never erase history"
        /// discipline as everything else in this table) but flagged
        /// IsActive = false, greyed out in the list, and excluded from any
        /// future calculator consideration once that integration exists.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Required (service-level) when voiding — why this entry was a mistake, e.g. "mauvaise dimension sélectionnée, voir dérogation #7 pour la bonne valeur".</summary>
        [StringLength(500)]
        public string? VoidReason { get; set; }

        public DateTime? VoidedAtUtc { get; set; }

        public string? VoidedByUserId { get; set; }
        [ForeignKey(nameof(VoidedByUserId))]
        public virtual ApplicationUser? VoidedByUser { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Who entered this record in AL-BORAK — NOT necessarily the same as ApprovalAuthority (the person typing this in is rarely the approving engineering authority).</summary>
        public string? CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
