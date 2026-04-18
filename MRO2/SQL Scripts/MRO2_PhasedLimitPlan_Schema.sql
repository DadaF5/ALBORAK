-- ============================================================
-- MRO2 — Phased Limit Plan Schema
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- Run after: MRO2_LookupTables_v2_Schema.sql
--            MRO2_CompRef_LimitTypeMap_Patch.sql
--            MRO2_PNLimit_SP_Patch.sql
-- ============================================================
--
-- DESIGN DECISIONS (locked):
--
-- 1. PNLimit IS the anchor.
--    A PNLimit row is EITHER simple (no plan) OR phased (has a plan).
--    Determined by IsPhased BIT on PNLimit.
--    Simple:  PNLimit.HardLimit / AlertThresholdPct used directly.
--    Phased:  PNLimitPlan + PNLimitPhase + PNLimitPhaseTrigger
--             override the simple values. PNLimit.HardLimit still
--             stores the absolute life limit (the ceiling that no
--             phase can exceed).
--
-- 2. Phase transition is FULLY AUTOMATIC.
--    When SNPhaseState.AccumulatedTowardTrigger reaches the
--    trigger threshold (any one of them — OR logic), the system
--    SP usp_SNPhaseState_Evaluate advances the SN to the next
--    phase automatically and resets the interval counter.
--
-- 3. OR logic for triggers.
--    Each phase can have 1–3 trigger rows (FH, FC, Calendar).
--    First trigger threshold reached advances the phase.
--    Both remaining values shown on dashboard simultaneously
--    enabling smart scheduling (high-freq → FH hits first,
--    low-freq → calendar hits first).
--
-- 4. SN override.
--    SNPhasePlanOverride links a specific SN to an alternative
--    PNLimitPlan (different phases, different triggers).
--    If no override exists, SN follows the PN master plan.
--
-- 5. Last phase repeats.
--    PNLimitPhase.IsLastPhase = 1 means the interval resets
--    indefinitely at this phase's interval until PNLimit.HardLimit
--    (the absolute ceiling) is reached.
--
-- TABLE CREATION ORDER:
--   1. ALTER mro2.PNLimit    (add IsPhased column)
--   2. mro2.PNLimitPlan
--   3. mro2.PNLimitPhase
--   4. mro2.PNLimitPhaseTrigger
--   5. mro2.SNPhasePlanOverride
--   6. mro2.SNPhaseState
--   7. Indexes
--   8. Stored Procedures
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- 1. ALTER mro2.PNLimit
--    Add IsPhased flag. When 1, the phased plan tables govern
--    the interval logic. HardLimit remains the absolute ceiling.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('mro2.PNLimit')
      AND name = 'IsPhased')
BEGIN
    ALTER TABLE mro2.PNLimit
        ADD IsPhased BIT NOT NULL
            CONSTRAINT DF_PNLimit_IsPhased DEFAULT (0);
END
GO

-- ============================================================
-- 2. mro2.PNLimitPlan
--    Header record for a phased plan.
--    One plan per PNLimit row (enforced by UQ).
--    A PN can have multiple PNLimit rows (one per counter type),
--    each with its own independent plan.
--
--    PlanDescription: auditor-facing text explaining the plan.
--    e.g. "Per CMM 29-10-00 Rev 12: escalating OH intervals
--           based on accumulated FH since new"
-- ============================================================
IF OBJECT_ID('mro2.PNLimitPlan','U') IS NULL
BEGIN
    CREATE TABLE mro2.PNLimitPlan (
        PNLimitPlanId   INT             NOT NULL IDENTITY(1,1),
        PNLimitId       INT             NOT NULL,   -- FK → mro2.PNLimit
        PlanDescription NVARCHAR(500)   NULL,       -- audit-facing narrative
        IsActive        BIT             NOT NULL
            CONSTRAINT DF_PNLimitPlan_IsActive  DEFAULT (1),
        CreatedDate     DATETIME        NOT NULL
            CONSTRAINT DF_PNLimitPlan_Created   DEFAULT (GETDATE()),
        CreatedByUserId NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_PNLimitPlan PRIMARY KEY (PNLimitPlanId),

        -- One plan per PNLimit row
        CONSTRAINT UQ_PNLimitPlan_PNLimitId UNIQUE (PNLimitId),

        CONSTRAINT FK_PNLimitPlan_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId)
    );
END
GO

-- ============================================================
-- 3. mro2.PNLimitPhase
--    One row per phase within a plan.
--    PhaseOrder: 1 = first phase, 2 = second, etc.
--    IsLastPhase: 1 = this phase repeats until HardLimit ceiling.
--
--    IntervalValue: the maintenance interval for this phase.
--    Stored in the same UnitStorage as the parent CounterDef
--    (MINUTES for FH, COUNT for FC/Calendar).
--
--    Example for hydraulic pump:
--      PhaseOrder=1, IntervalValue=60000 (=1000 FH in minutes),
--                    IsLastPhase=0
--      PhaseOrder=2, IntervalValue=36000 (=600 FH in minutes),
--                    IsLastPhase=1
--
--    AlertThresholdPct per phase: warn at X% of the interval
--    e.g. 90% of 1000 FH = alert at 900 FH remaining in phase
-- ============================================================
IF OBJECT_ID('mro2.PNLimitPhase','U') IS NULL
BEGIN
    CREATE TABLE mro2.PNLimitPhase (
        PNLimitPhaseId      INT             NOT NULL IDENTITY(1,1),
        PNLimitPlanId       INT             NOT NULL,   -- FK → mro2.PNLimitPlan
        PhaseOrder          TINYINT         NOT NULL,   -- 1, 2, 3...
        PhaseName           NVARCHAR(100)   NULL,       -- "Initial interval",
                                                        -- "Reduced interval"...
        -- Maintenance interval for this phase
        -- stored in parent CounterDef.UnitStorage units
        IntervalValue       INT             NOT NULL,   -- minutes or count
        -- Alert fires when AccumulatedSinceReset >=
        --   IntervalValue * AlertThresholdPct / 100
        AlertThresholdPct   TINYINT         NOT NULL
            CONSTRAINT DF_PNLimitPhase_AlertPct DEFAULT (90),
        -- Last phase: interval repeats until PNLimit.HardLimit
        IsLastPhase         BIT             NOT NULL
            CONSTRAINT DF_PNLimitPhase_IsLast   DEFAULT (0),
        Notes               NVARCHAR(300)   NULL,
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_PNLimitPhase_IsActive DEFAULT (1),

        CONSTRAINT PK_PNLimitPhase PRIMARY KEY (PNLimitPhaseId),

        -- PhaseOrder must be unique within a plan
        CONSTRAINT UQ_PNLimitPhase_Order
            UNIQUE (PNLimitPlanId, PhaseOrder),

        CONSTRAINT FK_PNLimitPhase_Plan FOREIGN KEY (PNLimitPlanId)
            REFERENCES mro2.PNLimitPlan (PNLimitPlanId),

        CONSTRAINT CK_PNLimitPhase_Interval
            CHECK (IntervalValue > 0),
        CONSTRAINT CK_PNLimitPhase_AlertPct
            CHECK (AlertThresholdPct BETWEEN 1 AND 99)
    );
END
GO

-- ============================================================
-- 4. mro2.PNLimitPhaseTrigger
--    Defines the threshold(s) that end a phase and advance
--    the SN to the next one. OR logic: first hit advances.
--
--    Each phase can have 1–3 trigger rows, one per CounterDefId
--    (e.g. FH trigger + Calendar trigger for OR logic).
--
--    TriggerValue: the ABSOLUTE total accumulated value
--    (since new or since plan start) that ends this phase.
--    e.g. 5000 FH total → TriggerValue = 300000 (minutes)
--         10 years       → TriggerValue = 3650 (days)
--
--    This is ABSOLUTE (not per-interval) because the transition
--    happens based on total life position, not interval position.
--
--    WHY ABSOLUTE:
--    A component at 4800 FH total entering Phase 1 will hit the
--    5000 FH phase trigger after only 200 more FH — not after
--    a full 1000 FH interval. The system must compare
--    SNPhaseState.LifetimeTotal against TriggerValue.
-- ============================================================
IF OBJECT_ID('mro2.PNLimitPhaseTrigger','U') IS NULL
BEGIN
    CREATE TABLE mro2.PNLimitPhaseTrigger (
        PNLimitPhaseTrigId  INT             NOT NULL IDENTITY(1,1),
        PNLimitPhaseId      INT             NOT NULL,   -- FK → mro2.PNLimitPhase
        -- Which counter drives this trigger
        -- Must share UnitStorage with parent PNLimit.CounterDef
        -- OR be a compatible calendar counter
        CounterDefId        INT             NOT NULL,   -- FK → mro2.CounterDef
        -- Absolute accumulated value that ends this phase (OR logic)
        -- MINUTES for FH-type, COUNT for FC/Calendar
        TriggerValue        INT             NOT NULL,
        -- Human-readable label shown on dashboard
        -- e.g. "5000 FH since new", "10 years from manufacture"
        TriggerLabel        NVARCHAR(100)   NULL,
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_PNLPhaseTrig_IsActive DEFAULT (1),

        CONSTRAINT PK_PNLimitPhaseTrigger PRIMARY KEY (PNLimitPhaseTrigId),

        -- One trigger per counter type per phase
        CONSTRAINT UQ_PNLimitPhaseTrigger_Phase_Counter
            UNIQUE (PNLimitPhaseId, CounterDefId),

        CONSTRAINT FK_PNLPhaseTrig_Phase FOREIGN KEY (PNLimitPhaseId)
            REFERENCES mro2.PNLimitPhase (PNLimitPhaseId),

        CONSTRAINT FK_PNLPhaseTrig_CounterDef FOREIGN KEY (CounterDefId)
            REFERENCES mro2.CounterDef (CounterDefId),

        CONSTRAINT CK_PNLPhaseTrig_Value
            CHECK (TriggerValue > 0)
    );
END
GO

-- ============================================================
-- 5. mro2.SNPhasePlanOverride
--    Links a specific SN to an ALTERNATIVE PNLimitPlan.
--    When this row exists, the SN follows the override plan
--    instead of the PN master plan.
--    Used for: manufacturer requirement after N overhauls,
--    operator engineering order, SB-driven plan change.
--
--    OverrideReason: mandatory audit field explaining why
--    this SN deviates from the PN master plan.
--    AuthorisedBy / AuthorisedRef: document/person approving.
-- ============================================================
IF OBJECT_ID('mro2.SNPhasePlanOverride','U') IS NULL
BEGIN
    CREATE TABLE mro2.SNPhasePlanOverride (
        SNPhasePlanOverrideId   INT             NOT NULL IDENTITY(1,1),
        SerializedItemId        INT             NOT NULL,   -- FK → mro2.SerializedItem
        PNLimitId               INT             NOT NULL,   -- which limit is overridden
        -- Points to the ALTERNATIVE plan (must be for same PNLimitId)
        OverridePNLimitPlanId   INT             NOT NULL,   -- FK → mro2.PNLimitPlan
        OverrideReason          NVARCHAR(500)   NOT NULL,   -- mandatory audit field
        AuthorisedBy            NVARCHAR(100)   NULL,       -- name/role
        AuthorisedRef           NVARCHAR(100)   NULL,       -- doc ref: SB-2024-001, EO-123
        EffectiveDate           DATE            NOT NULL,
        ExpiryDate              DATE            NULL,       -- NULL = no expiry
        IsActive                BIT             NOT NULL
            CONSTRAINT DF_SNPhasePlanOverride_IsActive DEFAULT (1),
        CreatedDate             DATETIME        NOT NULL
            CONSTRAINT DF_SNPhasePlanOverride_Created  DEFAULT (GETDATE()),
        CreatedByUserId         NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNPhasePlanOverride PRIMARY KEY (SNPhasePlanOverrideId),

        -- One active override per SN per limit
        CONSTRAINT UQ_SNPhasePlanOverride_SN_Limit
            UNIQUE (SerializedItemId, PNLimitId),

        CONSTRAINT FK_SNPhasePlanOverride_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNPhasePlanOverride_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId),

        CONSTRAINT FK_SNPhasePlanOverride_Plan FOREIGN KEY (OverridePNLimitPlanId)
            REFERENCES mro2.PNLimitPlan (PNLimitPlanId)
    );
END
GO

-- ============================================================
-- 6. mro2.SNPhaseState
--    Live state per SN per PNLimitPlan.
--    One row per (SerializedItemId, PNLimitPlanId).
--    Updated automatically by usp_SNPhaseState_Evaluate.
--
--    CurrentPNLimitPhaseId: which phase the SN is currently in.
--    AccumulatedSinceReset: hours/cycles since last interval reset
--                           (compared to Phase.IntervalValue).
--    LifetimeTotal: total since new (compared to
--                   PNLimitPhaseTrigger.TriggerValue for phase
--                   transitions and PNLimit.HardLimit for life).
--    OHCount: number of overhauls completed on this SN
--             (incremented on each reset event of type SINCE_OH).
--
--    IntervalStatus: OK | ALERT | DUE (interval exceeded)
--    LifeStatus:     OK | ALERT | EXPIRED (life limit exceeded)
--    PhaseStatus:    CURRENT | TRANSITION_DUE | TRANSITIONED
--
--    Both stored for dashboard speed + recomputed in view
--    for report accuracy (same dual-storage pattern as SNCounter).
-- ============================================================
IF OBJECT_ID('mro2.SNPhaseState','U') IS NULL
BEGIN
    CREATE TABLE mro2.SNPhaseState (
        SNPhaseStateId          INT             NOT NULL IDENTITY(1,1),
        SerializedItemId        INT             NOT NULL,
        PNLimitPlanId           INT             NOT NULL,   -- master or override plan

        -- Current phase
        CurrentPNLimitPhaseId   INT             NOT NULL,   -- FK → mro2.PNLimitPhase
        PhaseEntryDate          DATE            NOT NULL,   -- when SN entered this phase
        PhaseEntryLifetime      INT             NOT NULL    -- lifetime value at phase entry
            CONSTRAINT DF_SNPhaseState_EntryLife DEFAULT (0),

        -- Interval tracking (resets at each OH within phase)
        AccumulatedSinceReset   INT             NOT NULL    -- since last interval reset
            CONSTRAINT DF_SNPhaseState_AccumReset DEFAULT (0),
        LastResetDate           DATE            NULL,
        LastResetLifetime       INT             NOT NULL    -- lifetime at last reset
            CONSTRAINT DF_SNPhaseState_LastResetLife DEFAULT (0),

        -- Lifetime tracking (since new — never resets)
        LifetimeTotal           INT             NOT NULL
            CONSTRAINT DF_SNPhaseState_Lifetime DEFAULT (0),

        -- Overhaul count for this SN on this plan
        OHCount                 SMALLINT        NOT NULL
            CONSTRAINT DF_SNPhaseState_OHCount  DEFAULT (0),

        -- Stored status fields (fast dashboard)
        IntervalStatus          VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNPhaseState_IntStatus DEFAULT ('OK'),
        LifeStatus              VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNPhaseState_LifeStatus DEFAULT ('OK'),
        PhaseStatus             VARCHAR(20)     NOT NULL
            CONSTRAINT DF_SNPhaseState_PhaseStatus DEFAULT ('CURRENT'),

        -- Stored remaining values (fast dashboard)
        -- Recomputed by SP on every update
        RemainingInInterval     INT             NOT NULL    -- to next OH
            CONSTRAINT DF_SNPhaseState_RemInterval DEFAULT (0),
        RemainingToHardLimit    INT             NOT NULL    -- to absolute ceiling
            CONSTRAINT DF_SNPhaseState_RemLife DEFAULT (0),

        -- Source and audit
        LastUpdatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_SNPhaseState_Updated  DEFAULT (GETDATE()),
        LastUpdatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNPhaseState PRIMARY KEY (SNPhaseStateId),

        CONSTRAINT UQ_SNPhaseState_SN_Plan
            UNIQUE (SerializedItemId, PNLimitPlanId),

        CONSTRAINT FK_SNPhaseState_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNPhaseState_Plan FOREIGN KEY (PNLimitPlanId)
            REFERENCES mro2.PNLimitPlan (PNLimitPlanId),

        CONSTRAINT FK_SNPhaseState_Phase FOREIGN KEY (CurrentPNLimitPhaseId)
            REFERENCES mro2.PNLimitPhase (PNLimitPhaseId),

        CONSTRAINT CK_SNPhaseState_IntervalStatus
            CHECK (IntervalStatus IN ('OK','ALERT','DUE')),
        CONSTRAINT CK_SNPhaseState_LifeStatus
            CHECK (LifeStatus IN ('OK','ALERT','EXPIRED')),
        CONSTRAINT CK_SNPhaseState_PhaseStatus
            CHECK (PhaseStatus IN ('CURRENT','TRANSITION_DUE','TRANSITIONED'))
    );
END
GO

-- ============================================================
-- INDEXES
-- ============================================================

-- Fast lookup by SN across all plans
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_SNPhaseState_SerializedItemId'
    AND object_id=OBJECT_ID('mro2.SNPhaseState'))
    CREATE INDEX IX_SNPhaseState_SerializedItemId
        ON mro2.SNPhaseState (SerializedItemId)
        INCLUDE (PNLimitPlanId, CurrentPNLimitPhaseId,
                 IntervalStatus, LifeStatus, PhaseStatus,
                 RemainingInInterval, RemainingToHardLimit);
GO

-- Dashboard: all SNs with non-OK status
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_SNPhaseState_Status'
    AND object_id=OBJECT_ID('mro2.SNPhaseState'))
    CREATE INDEX IX_SNPhaseState_Status
        ON mro2.SNPhaseState (IntervalStatus, LifeStatus, PhaseStatus)
        INCLUDE (SerializedItemId, RemainingInInterval, RemainingToHardLimit);
GO

-- Fast phase trigger lookup
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_PNLimitPhaseTrigger_PhaseId'
    AND object_id=OBJECT_ID('mro2.PNLimitPhaseTrigger'))
    CREATE INDEX IX_PNLimitPhaseTrigger_PhaseId
        ON mro2.PNLimitPhaseTrigger (PNLimitPhaseId)
        INCLUDE (CounterDefId, TriggerValue, TriggerLabel);
GO

-- ============================================================
-- VIEW: mro2.vw_SNPhaseStatus
--    Master dashboard view for phased limits.
--    Resolves: SN override plan vs PN master plan.
--    Dynamically recomputes remaining + status for reports.
--    Joins trigger rows to show all active triggers per phase
--    with their individual remaining values (smart scheduling).
-- ============================================================
IF OBJECT_ID('mro2.vw_SNPhaseStatus','V') IS NOT NULL
    DROP VIEW mro2.vw_SNPhaseStatus;
GO
CREATE VIEW mro2.vw_SNPhaseStatus
AS
SELECT
    -- Identity
    si.SerializedItemId,
    si.SerialNumber,
    pn.PartNumberId,
    pn.PN,
    pn.Nomenclature,

    -- Plan in effect (SN override wins if exists and active)
    CASE WHEN ov.SNPhasePlanOverrideId IS NOT NULL
         THEN 'OVERRIDE' ELSE 'PN_MASTER' END          AS PlanSource,
    ps.PNLimitPlanId,
    plan.PlanDescription,

    -- Current phase
    ps.CurrentPNLimitPhaseId,
    phase.PhaseOrder,
    phase.PhaseName,
    phase.IntervalValue,
    phase.AlertThresholdPct,
    phase.IsLastPhase,

    -- Interval tracking
    ps.AccumulatedSinceReset,
    ps.LastResetDate,
    ps.OHCount,

    -- Dynamically recomputed interval remaining
    phase.IntervalValue - ps.AccumulatedSinceReset       AS RemainingInIntervalCalc,
    -- Stored remaining (fast)
    ps.RemainingInInterval                               AS RemainingInIntervalStored,

    -- Alert threshold value for this phase
    phase.IntervalValue * phase.AlertThresholdPct / 100  AS AlertAtValue,

    -- Lifetime
    ps.LifetimeTotal,

    -- Hard life limit (absolute ceiling from PNLimit)
    pl.HardLimit                                         AS HardLifeLimit,
    -- Dynamically recomputed life remaining
    pl.HardLimit - ps.LifetimeTotal                      AS RemainingToHardLimitCalc,
    -- Stored
    ps.RemainingToHardLimit                              AS RemainingToHardLimitStored,

    -- Dynamically recomputed statuses
    CASE
        WHEN ps.LifetimeTotal >= pl.HardLimit
        THEN 'EXPIRED'
        WHEN ps.LifetimeTotal >= pl.HardLimit * phase.AlertThresholdPct / 100
        THEN 'ALERT'
        ELSE 'OK'
    END                                                  AS LifeStatusCalc,

    CASE
        WHEN ps.AccumulatedSinceReset >= phase.IntervalValue
        THEN 'DUE'
        WHEN ps.AccumulatedSinceReset >=
             phase.IntervalValue * phase.AlertThresholdPct / 100
        THEN 'ALERT'
        ELSE 'OK'
    END                                                  AS IntervalStatusCalc,

    -- Stored statuses (fast dashboard)
    ps.IntervalStatus                                    AS IntervalStatusStored,
    ps.LifeStatus                                        AS LifeStatusStored,
    ps.PhaseStatus,

    -- Phase entry context
    ps.PhaseEntryDate,
    ps.PhaseEntryLifetime,
    ps.LastUpdatedDate,

    -- Counter type context (for unit display)
    ct.Code                                              AS CounterTypeCode,
    ct.DisplayUnit,
    ct.UnitStorage,

    -- Override audit info
    ov.OverrideReason,
    ov.AuthorisedRef

FROM mro2.SNPhaseState ps

INNER JOIN mro2.PNLimitPlan        plan ON plan.PNLimitPlanId       = ps.PNLimitPlanId
INNER JOIN mro2.PNLimit            pl   ON pl.PNLimitId             = plan.PNLimitId
INNER JOIN mro2.PNLimitPhase       phase ON phase.PNLimitPhaseId    = ps.CurrentPNLimitPhaseId
INNER JOIN mro2.SerializedItem     si   ON si.SerializedItemId      = ps.SerializedItemId
INNER JOIN mro2.PartNumber         pn   ON pn.PartNumberId          = si.PartNumberId
INNER JOIN mro2.CounterDef         cd   ON cd.CounterDefId          =
    (SELECT TOP 1 t.CounterDefId
     FROM mro2.PNLimitPhaseTrigger t
     INNER JOIN mro2.PNLimitPhase  p2 ON p2.PNLimitPhaseId = t.PNLimitPhaseId
     WHERE p2.PNLimitPlanId = ps.PNLimitPlanId
     ORDER BY p2.PhaseOrder, t.PNLimitPhaseTrigId)
INNER JOIN mro2.CounterType        ct   ON ct.CounterTypeId         = cd.CounterTypeId
-- Override: does this SN have a non-expired active plan override?
LEFT JOIN mro2.SNPhasePlanOverride ov   ON ov.SerializedItemId      = ps.SerializedItemId
                                       AND ov.PNLimitId             = pl.PNLimitId
                                       AND ov.IsActive              = 1
                                       AND ov.OverridePNLimitPlanId = ps.PNLimitPlanId
                                       AND (ov.ExpiryDate IS NULL
                                            OR ov.ExpiryDate >= CAST(GETDATE() AS DATE))
WHERE ps.PNLimitPlanId = plan.PNLimitPlanId;
GO

-- ============================================================
-- SP: mro2.usp_SNPhaseState_Evaluate
--    Core engine SP. Called after every counter update.
--    1. Resolves which plan the SN follows (master or override)
--    2. Checks all active triggers for current phase (OR logic)
--    3. If any trigger threshold reached → advances phase
--       automatically, resets interval counter, increments OHCount
--    4. Recomputes and stores remaining + status values
--    5. Writes to SNPhaseState
--
--    @NewLifetimeTotal: updated lifetime value for this SN
--    @NewAccumulated:   updated since-reset value
--    @UserId:           Session("UserId")
-- ============================================================
IF OBJECT_ID('mro2.usp_SNPhaseState_Evaluate','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNPhaseState_Evaluate;
GO
CREATE PROCEDURE mro2.usp_SNPhaseState_Evaluate
    @SerializedItemId   INT,
    @PNLimitId          INT,
    @NewLifetimeTotal   INT,        -- absolute total since new (MINUTES or COUNT)
    @NewAccumulated     INT,        -- since last reset
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- ── 1. Resolve which plan this SN follows ────────────────
    DECLARE @PlanId INT;

    -- Check for active, non-expired SN override first
    SELECT TOP 1 @PlanId = ov.OverridePNLimitPlanId
    FROM mro2.SNPhasePlanOverride ov
    WHERE ov.SerializedItemId = @SerializedItemId
      AND ov.PNLimitId        = @PNLimitId
      AND ov.IsActive         = 1
      AND (ov.ExpiryDate IS NULL OR ov.ExpiryDate >= CAST(GETDATE() AS DATE));

    -- Fall back to PN master plan
    IF @PlanId IS NULL
    BEGIN
        SELECT @PlanId = PNLimitPlanId
        FROM mro2.PNLimitPlan
        WHERE PNLimitId = @PNLimitId
          AND IsActive  = 1;
    END

    IF @PlanId IS NULL
    BEGIN
        -- No plan exists — nothing to evaluate
        RETURN;
    END

    -- ── 2. Get current phase state ───────────────────────────
    DECLARE @CurrentPhaseId     INT;
    DECLARE @CurrentPhaseOrder  TINYINT;
    DECLARE @IsLastPhase        BIT;
    DECLARE @IntervalValue      INT;
    DECLARE @AlertPct           TINYINT;
    DECLARE @OHCount            SMALLINT;
    DECLARE @PhaseEntryLifetime INT;
    DECLARE @PhaseEntryDate     DATE;
    DECLARE @LastResetLifetime  INT;

    SELECT
        @CurrentPhaseId     = ps.CurrentPNLimitPhaseId,
        @OHCount            = ps.OHCount,
        @PhaseEntryLifetime = ps.PhaseEntryLifetime,
        @PhaseEntryDate     = ps.PhaseEntryDate,
        @LastResetLifetime  = ps.LastResetLifetime
    FROM mro2.SNPhaseState ps
    WHERE ps.SerializedItemId = @SerializedItemId
      AND ps.PNLimitPlanId   = @PlanId;

    -- First time: initialise with Phase 1
    IF @CurrentPhaseId IS NULL
    BEGIN
        SELECT TOP 1 @CurrentPhaseId   = PNLimitPhaseId,
                     @CurrentPhaseOrder = PhaseOrder,
                     @IsLastPhase       = IsLastPhase,
                     @IntervalValue     = IntervalValue,
                     @AlertPct          = AlertThresholdPct
        FROM mro2.PNLimitPhase
        WHERE PNLimitPlanId = @PlanId
          AND IsActive      = 1
        ORDER BY PhaseOrder;

        SET @OHCount            = 0;
        SET @PhaseEntryLifetime = @NewLifetimeTotal;
        SET @PhaseEntryDate     = CAST(GETDATE() AS DATE);
        SET @LastResetLifetime  = @NewLifetimeTotal;
    END
    ELSE
    BEGIN
        SELECT @CurrentPhaseOrder = PhaseOrder,
               @IsLastPhase       = IsLastPhase,
               @IntervalValue     = IntervalValue,
               @AlertPct          = AlertThresholdPct
        FROM mro2.PNLimitPhase
        WHERE PNLimitPhaseId = @CurrentPhaseId;
    END

    -- ── 3. Check triggers — OR logic ────────────────────────
    -- A trigger fires when NewLifetimeTotal >= TriggerValue
    -- for any active trigger on the current phase.
    DECLARE @TriggerFired BIT = 0;

    IF @IsLastPhase = 0
    BEGIN
        SELECT TOP 1 @TriggerFired = 1
        FROM mro2.PNLimitPhaseTrigger t
        WHERE t.PNLimitPhaseId = @CurrentPhaseId
          AND t.IsActive       = 1
          AND @NewLifetimeTotal >= t.TriggerValue;
    END

    -- ── 4. If trigger fired → advance to next phase ──────────
    DECLARE @AdvancedPhase BIT = 0;

    IF @TriggerFired = 1
    BEGIN
        -- Get next phase
        DECLARE @NextPhaseId    INT;
        DECLARE @NextPhaseOrder TINYINT;

        SELECT TOP 1
            @NextPhaseId    = PNLimitPhaseId,
            @NextPhaseOrder = PhaseOrder,
            @IsLastPhase    = IsLastPhase,
            @IntervalValue  = IntervalValue,
            @AlertPct       = AlertThresholdPct
        FROM mro2.PNLimitPhase
        WHERE PNLimitPlanId = @PlanId
          AND PhaseOrder    > @CurrentPhaseOrder
          AND IsActive      = 1
        ORDER BY PhaseOrder;

        IF @NextPhaseId IS NOT NULL
        BEGIN
            -- Move to next phase
            SET @CurrentPhaseId     = @NextPhaseId;
            SET @CurrentPhaseOrder  = @NextPhaseOrder;
            SET @PhaseEntryLifetime = @NewLifetimeTotal;
            SET @PhaseEntryDate     = CAST(GETDATE() AS DATE);
            -- Reset interval counter (automatic OH reset)
            SET @NewAccumulated     = 0;
            SET @LastResetLifetime  = @NewLifetimeTotal;
            SET @OHCount            = @OHCount + 1;
            SET @AdvancedPhase      = 1;
        END
        -- If no next phase exists, stay on current (IsLastPhase=1)
    END

    -- ── 5. Check interval reset within current phase ─────────
    -- If accumulated >= interval and not already reset by phase advance
    IF @AdvancedPhase = 0 AND @NewAccumulated >= @IntervalValue
    BEGIN
        -- Interval complete: reset counter, increment OH
        SET @NewAccumulated    = 0;
        SET @LastResetLifetime = @NewLifetimeTotal;
        SET @OHCount           = @OHCount + 1;
    END

    -- ── 6. Resolve hard life limit ───────────────────────────
    DECLARE @HardLimit INT;
    SELECT @HardLimit = CAST(HardLimit AS INT)
    FROM mro2.PNLimit
    WHERE PNLimitId = @PNLimitId;

    -- ── 7. Compute stored remaining + status ─────────────────
    DECLARE @RemInterval    INT = @IntervalValue - @NewAccumulated;
    DECLARE @RemLife        INT = @HardLimit - @NewLifetimeTotal;
    DECLARE @AlertAtValue   INT = @IntervalValue * @AlertPct / 100;

    DECLARE @IntervalStatus VARCHAR(10) =
        CASE WHEN @NewAccumulated >= @IntervalValue       THEN 'DUE'
             WHEN @NewAccumulated >= @AlertAtValue        THEN 'ALERT'
             ELSE 'OK' END;

    DECLARE @LifeStatus VARCHAR(10) =
        CASE WHEN @NewLifetimeTotal >= @HardLimit         THEN 'EXPIRED'
             WHEN @NewLifetimeTotal >= @HardLimit * @AlertPct / 100
             THEN 'ALERT'
             ELSE 'OK' END;

    DECLARE @PhaseStatus VARCHAR(20) =
        CASE WHEN @AdvancedPhase = 1                      THEN 'TRANSITIONED'
             WHEN @TriggerFired  = 1                      THEN 'TRANSITION_DUE'
             ELSE 'CURRENT' END;

    -- ── 8. Upsert SNPhaseState ───────────────────────────────
    IF EXISTS (
        SELECT 1 FROM mro2.SNPhaseState
        WHERE SerializedItemId = @SerializedItemId
          AND PNLimitPlanId    = @PlanId)
    BEGIN
        UPDATE mro2.SNPhaseState SET
            CurrentPNLimitPhaseId = @CurrentPhaseId,
            PhaseEntryDate        = @PhaseEntryDate,
            PhaseEntryLifetime    = @PhaseEntryLifetime,
            AccumulatedSinceReset = @NewAccumulated,
            LastResetDate         = CASE WHEN @OHCount >
                                         (SELECT OHCount FROM mro2.SNPhaseState
                                          WHERE SerializedItemId = @SerializedItemId
                                            AND PNLimitPlanId    = @PlanId)
                                         THEN CAST(GETDATE() AS DATE)
                                         ELSE LastResetDate END,
            LastResetLifetime     = @LastResetLifetime,
            LifetimeTotal         = @NewLifetimeTotal,
            OHCount               = @OHCount,
            IntervalStatus        = @IntervalStatus,
            LifeStatus            = @LifeStatus,
            PhaseStatus           = @PhaseStatus,
            RemainingInInterval   = @RemInterval,
            RemainingToHardLimit  = @RemLife,
            LastUpdatedDate       = GETDATE(),
            LastUpdatedByUserId   = @UserId
        WHERE SerializedItemId = @SerializedItemId
          AND PNLimitPlanId    = @PlanId;
    END
    ELSE
    BEGIN
        INSERT INTO mro2.SNPhaseState (
            SerializedItemId, PNLimitPlanId,
            CurrentPNLimitPhaseId,
            PhaseEntryDate, PhaseEntryLifetime,
            AccumulatedSinceReset,
            LastResetDate, LastResetLifetime,
            LifetimeTotal, OHCount,
            IntervalStatus, LifeStatus, PhaseStatus,
            RemainingInInterval, RemainingToHardLimit,
            LastUpdatedDate, LastUpdatedByUserId)
        VALUES (
            @SerializedItemId, @PlanId,
            @CurrentPhaseId,
            @PhaseEntryDate, @PhaseEntryLifetime,
            @NewAccumulated,
            CAST(GETDATE() AS DATE), @LastResetLifetime,
            @NewLifetimeTotal, @OHCount,
            @IntervalStatus, @LifeStatus, @PhaseStatus,
            @RemInterval, @RemLife,
            GETDATE(), @UserId);
    END

    -- ── 9. Return evaluation result for caller ───────────────
    SELECT
        @PlanId                 AS PNLimitPlanId,
        @CurrentPhaseId         AS CurrentPNLimitPhaseId,
        @CurrentPhaseOrder      AS PhaseOrder,
        @AdvancedPhase          AS PhaseAdvanced,
        @OHCount                AS OHCount,
        @NewAccumulated         AS AccumulatedSinceReset,
        @NewLifetimeTotal       AS LifetimeTotal,
        @RemInterval            AS RemainingInInterval,
        @RemLife                AS RemainingToHardLimit,
        @IntervalStatus         AS IntervalStatus,
        @LifeStatus             AS LifeStatus,
        @PhaseStatus            AS PhaseStatus;
END
GO

-- ============================================================
-- SP: mro2.usp_SNPhaseState_GetBySN
--    Returns full phase state + all trigger remaining values
--    for a single SN. Used by the SN detail page.
--    Trigger remaining values enable the smart scheduling
--    dashboard (shows FH remaining AND calendar remaining).
-- ============================================================
IF OBJECT_ID('mro2.usp_SNPhaseState_GetBySN','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNPhaseState_GetBySN;
GO
CREATE PROCEDURE mro2.usp_SNPhaseState_GetBySN
    @SerializedItemId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Phase state summary
    SELECT * FROM mro2.vw_SNPhaseStatus
    WHERE SerializedItemId = @SerializedItemId;

    -- All active triggers for current phase with remaining values
    -- (the smart scheduling data — shown side by side on dashboard)
    SELECT
        t.PNLimitPhaseTrigId,
        t.PNLimitPhaseId,
        t.CounterDefId,
        cd.Code                             AS CounterDefCode,
        ct.DisplayUnit,
        ct.UnitStorage,
        t.TriggerValue,
        t.TriggerLabel,
        -- Remaining to this trigger for this SN
        t.TriggerValue - ps.LifetimeTotal   AS RemainingToTrigger,
        -- Pct consumed toward this trigger
        CASE WHEN t.TriggerValue > 0
             THEN CAST(ps.LifetimeTotal * 100.0
                       / t.TriggerValue AS DECIMAL(5,1))
             ELSE 0 END                     AS PctConsumedToTrigger
    FROM mro2.SNPhaseState ps
    INNER JOIN mro2.PNLimitPlan          plan ON plan.PNLimitPlanId   = ps.PNLimitPlanId
    INNER JOIN mro2.PNLimitPhase         phase ON phase.PNLimitPhaseId = ps.CurrentPNLimitPhaseId
    INNER JOIN mro2.PNLimitPhaseTrigger  t     ON t.PNLimitPhaseId    = phase.PNLimitPhaseId
                                              AND t.IsActive          = 1
    INNER JOIN mro2.CounterDef           cd    ON cd.CounterDefId     = t.CounterDefId
    INNER JOIN mro2.CounterType          ct    ON ct.CounterTypeId    = cd.CounterTypeId
    WHERE ps.SerializedItemId = @SerializedItemId
    ORDER BY ct.SortOrder;
END
GO

-- ============================================================
-- SP: mro2.usp_SNPhaseState_GetAlertDashboard
--    Returns all SNs with non-OK interval or life status.
--    Ordered: EXPIRED/DUE first, then ALERT, then by remaining.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNPhaseState_GetAlertDashboard','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNPhaseState_GetAlertDashboard;
GO
CREATE PROCEDURE mro2.usp_SNPhaseState_GetAlertDashboard
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        SerializedItemId,
        SerialNumber,
        PN,
        Nomenclature,
        CounterTypeCode,
        DisplayUnit,
        UnitStorage,
        PhaseOrder,
        PhaseName,
        IsLastPhase,
        IntervalValue,
        AccumulatedSinceReset,
        RemainingInIntervalCalc,
        AlertAtValue,
        LifetimeTotal,
        HardLifeLimit,
        RemainingToHardLimitCalc,
        IntervalStatusCalc,
        LifeStatusCalc,
        PhaseStatus,
        OHCount,
        PlanSource,
        OverrideReason
    FROM mro2.vw_SNPhaseStatus
    WHERE IntervalStatusCalc IN ('DUE','ALERT')
       OR LifeStatusCalc     IN ('EXPIRED','ALERT')
    ORDER BY
        -- Worst first
        CASE WHEN LifeStatusCalc    = 'EXPIRED' THEN 0
             WHEN IntervalStatusCalc = 'DUE'    THEN 1
             WHEN LifeStatusCalc    = 'ALERT'   THEN 2
             WHEN IntervalStatusCalc = 'ALERT'  THEN 3
             ELSE 4 END,
        RemainingInIntervalCalc ASC;
END
GO

-- ============================================================
-- VERIFICATION
-- ============================================================
/*
-- Check all tables created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='mro2'
  AND TABLE_NAME IN ('PNLimitPlan','PNLimitPhase',
                     'PNLimitPhaseTrigger','SNPhasePlanOverride',
                     'SNPhaseState')
ORDER BY TABLE_NAME;

-- Check IsPhased column added to PNLimit
SELECT COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='mro2' AND TABLE_NAME='PNLimit'
  AND COLUMN_NAME='IsPhased';

-- Check SPs
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA='mro2'
  AND ROUTINE_NAME IN ('usp_SNPhaseState_Evaluate',
                       'usp_SNPhaseState_GetBySN',
                       'usp_SNPhaseState_GetAlertDashboard');

-- ── SAMPLE DATA — hydraulic pump example ─────────────────
-- Assumes PartNumberId=1, PNLimitId=1 already exists with
-- HardLimit=300000 (=5000 FH in minutes), IsPhased=1

-- Step 1: Create the plan
-- INSERT INTO mro2.PNLimitPlan (PNLimitId, PlanDescription, CreatedByUserId)
-- VALUES (1, 'Hydraulic pump: 1000 FH x5 then 600 FH per CMM 29-10-00', 'admin');

-- Step 2: Phase 1 (1000 FH interval, ends at 5000 FH)
-- INSERT INTO mro2.PNLimitPhase
--   (PNLimitPlanId, PhaseOrder, PhaseName, IntervalValue, AlertThresholdPct, IsLastPhase)
-- VALUES (1, 1, 'Initial interval (1000 FH)', 60000, 90, 0);  -- 60000 min = 1000 FH

-- Step 3: Phase 2 (600 FH interval, repeats until life limit)
-- INSERT INTO mro2.PNLimitPhase
--   (PNLimitPlanId, PhaseOrder, PhaseName, IntervalValue, AlertThresholdPct, IsLastPhase)
-- VALUES (1, 2, 'Reduced interval (600 FH)', 36000, 90, 1);   -- 36000 min = 600 FH

-- Step 4: Phase 1 triggers (OR logic: 5000 FH OR 10 years)
-- INSERT INTO mro2.PNLimitPhaseTrigger
--   (PNLimitPhaseId, CounterDefId, TriggerValue, TriggerLabel)
-- VALUES (1, 2, 300000, '5000 FH since new');  -- CounterDefId=2: AF_FLIGHT_MIN

-- INSERT INTO mro2.PNLimitPhaseTrigger
--   (PNLimitPhaseId, CounterDefId, TriggerValue, TriggerLabel)
-- VALUES (1, 10, 3650, '10 years (calendar)');  -- CounterDefId=10: CAL_DAYS

-- Step 5: Mark PNLimit as phased
-- UPDATE mro2.PNLimit SET IsPhased=1 WHERE PNLimitId=1;
*/
