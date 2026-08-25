using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// How a ComponentType's remaining life is tracked.
    /// ON_CONDITION = no fixed retirement limit, monitored by inspection/condition only.
    /// HARD_TIME    = fixed FH/Cycles/Calendar retirement limit (life-limited part).
    /// </summary>
    public enum ComponentTrackingMethod
    {
        OnCondition = 0,
        HardTime = 1
    }

    /// <summary>
    /// For HARD_TIME components: whether the life limit counts from new,
    /// or resets at each overhaul.
    /// NEW — [Display(Name)] added: this enum had no French labels at all,
    /// so both the "Base du calcul" dropdown on the profile form AND the
    /// ManageLifeLimits list (which prints @p.LifeBasis raw) showed the bare
    /// English member name ("SinceNew"/"SinceOverhaul") — confusing on its
    /// own and inconsistent with ComponentReferenceBasis's real seeded
    /// French Names shown right next to it. Html.GetEnumSelectList and
    /// Html.DisplayFor both pick these up automatically, no other code
    /// change needed.
    /// </summary>
    public enum ComponentLifeBasis
    {
        [Display(Name = "Depuis neuf")]
        SinceNew = 0,
        [Display(Name = "Depuis révision")]
        SinceOverhaul = 1
    }

    /// <summary>
    /// Current physical/administrative state of a serialized Component.
    /// </summary>
    public enum ComponentStatus
    {
        InStock = 0,
        Installed = 1,
        UnderRepair = 2,
        Removed = 3,   // pulled, not yet back in stock nor scrapped (transitional)
        Scrapped = 4
    }

    /// <summary>
    /// One entry in a Component's append-only genealogy log.
    /// Never edited after creation — corrections are made by adding a new event,
    /// same convention as UserAssignment revoke-and-recreate.
    /// </summary>
    public enum ComponentEventType
    {
        Receipt = 0,          // new part received into stock (or into service records)
        Install = 1,
        Remove = 2,
        TransferToStock = 3,  // administrative move back to stock without an Aircraft link (rare; normally Remove already returns it to stock)
        Overhaul = 4,          // resets SinceOverhaul counters
        Scrap = 5,
        /// <summary>NEW — sub-assembly attached to a parent Component (e.g. a DEEC bolted onto an Engine). RelatedParentComponentId is set.</summary>
        AttachToParent = 6,
        /// <summary>NEW — sub-assembly detached from its parent Component. RelatedParentComponentId is set (records which parent it came off of).</summary>
        DetachFromParent = 7
    }

    public enum ComponentRemovalReason
    {
        Scheduled = 0,
        LlpExpiry = 1,
        UnscheduledFailure = 2,
        Precautionary = 3,
        /// <summary>NEW — tactical Cannibalization ("Canni"): part pulled from one tail to make another mission-capable, not because the part itself failed.</summary>
        Cannibalization = 4
    }

    /// <summary>
    /// Which Component(s) a ComponentLifeLimitProfile applies to. Deliberately
    /// mirrors JobCardApplicability's RuleType (RANGE_FROM|RANGE_TO|SPECIFIC|PN_BASED)
    /// — same real problem ("same PN, different rule per S/N"), same resolution
    /// priority: Specific > Range > PnBased default.
    ///
    /// NEW — Lot (Derogation discussion): a legacy-data-confirmed scope
    /// (tblMeca_ItemSerialNo.LotReference) a life-limit profile never needed
    /// but a Derogation does — Dadda confirmed derogations can be issued
    /// "PN-wide, Lot-wise, or S/N-series-wise". Only ComponentDerogation uses
    /// this value today; ComponentLifeLimitProfile's own resolution order
    /// (ComponentLifeStatusCalculator.ResolveProfile) does not handle it and
    /// should keep rejecting/ignoring it if ever posted there by mistake.
    /// See ComponentDerogation.LotReference / Component.LotReference.
    /// </summary>
    public enum ApplicabilityRuleType
    {
        [Display(Name = "PN générique")]
        PnBased = 0,
        [Display(Name = "Spécifique")]
        Specific = 1,
        [Display(Name = "Plage (à partir de)")]
        RangeFrom = 2,
        [Display(Name = "Plage (jusqu'à)")]
        RangeTo = 3,
        [Display(Name = "Lot")]
        Lot = 4
    }

    /// <summary>
    /// NEW — Derogation urgency tiering, modeled on the USAF Time Compliance
    /// Technical Order (TCTO) categories (T.O. 00-5-15: Immediate/Urgent/
    /// Routine Action, each with its own mandated compliance window) —
    /// pulled in at Dadda's request after the DoD/USAF standards comparison.
    /// Nullable on ComponentDerogation (Tier) — most of BAFRA's real
    /// historical data (both sample tblMeca_ItemDerogation rows) predates
    /// this concept entirely and simply won't have a tier; leave it unset
    /// rather than guessing on migration.
    /// </summary>
    public enum DerogationTier
    {
        [Display(Name = "Immédiate")]
        Immediate = 0,
        [Display(Name = "Urgente")]
        Urgent = 1,
        [Display(Name = "Routine")]
        Routine = 2
    }

    /// <summary>
    /// NEW — explicit Extension-vs-Reduction flag for ComponentDerogation.
    /// Resolves an open question from the Derogation design discussion
    /// ("confirm 'reduced' is real... if so, no UI wording should assume
    /// extension-only"): rather than relying on the sign of Value (fragile —
    /// one Math.Abs() slip anywhere silently flips the meaning), Direction is
    /// a first-class, explicit field. Value is always entered/stored as a
    /// positive magnitude; Direction says whether it's added to or
    /// subtracted from OriginalBaseValue. Reduction is the rarer real case
    /// Dadda confirmed (e.g. a known-defect AD that tightens a limit).
    /// </summary>
    public enum DerogationDirection
    {
        [Display(Name = "Extension")]
        Extension = 0,
        [Display(Name = "Réduction")]
        Reduction = 1
    }

    /// <summary>A band in a staged overhaul/retirement schedule.</summary>
    public enum ComponentLifeLimitStageType
    {
        [Display(Name = "Révision")]
        Overhaul = 0,
        [Display(Name = "Réforme")]
        Retirement = 1
    }

    /// <summary>
    /// Whether a stage's tolerance values are absolute (same unit as the
    /// interval) or a whole-number percentage of that stage's interval.
    /// </summary>
    public enum ComponentToleranceType
    {
        Absolute = 0,
        PercentOfInterval = 1
    }

    /// <summary>
    /// Computed due/overdue status — mirrors the OK/ALERTE/DÉPASSÉ/INCONNU
    /// vocabulary already used by InspectionStatusCalculator/DueList,
    /// plus NotLifeLimited for ON_CONDITION components.
    /// </summary>
    public enum ComponentLifeStatusValue
    {
        NotLifeLimited = 0,
        Ok = 1,
        Alert = 2,
        Overdue = 3,
        Unknown = 4
    }
}
