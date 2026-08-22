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
    /// </summary>
    public enum ComponentLifeBasis
    {
        SinceNew = 0,
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
    /// </summary>
    public enum ApplicabilityRuleType
    {
        PnBased = 0,
        Specific = 1,
        RangeFrom = 2,
        RangeTo = 3
    }

    /// <summary>A band in a staged overhaul/retirement schedule.</summary>
    public enum ComponentLifeLimitStageType
    {
        Overhaul = 0,
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
