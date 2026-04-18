-- ============================================================
-- MRO2 — Task Counter Schema (Airbus MPD Model)
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- REPLACES: MRO2_PhasedLimitPlan_Schema.sql (never run)
-- Run after: MRO2_LookupTables_v2_Schema.sql
--            MRO2_CompRef_LimitTypeMap_Patch.sql
--            MRO2_PNLimit_SP_Patch.sql
-- ============================================================
--
-- AIRBUS MPD MODEL — CORE CONCEPTS:
--
--   A maintenance task (PNLimit) can have 1..N counter lines.
--   Each counter line (TaskCounter) races independently.
--   The task is DUE when ANY counter line reaches its NextDueAt.
--   This is pure OR logic across all counter lines.
--
--   Each TaskCounter row defines:
--     CounterDefId     : which counter (AF_FLIGHT_MIN, CAL_DAYS...)
--     CounterBasisId   : from when (SINCE_NEW, SINCE_INSTALL...)
--     FirstThreshold   : one-time initial due point from basis
--     RepeatInterval   : every subsequent interval after first done
--     Ceiling          : absolute max (NULL = no ceiling, repeats forever)
--     AlertThresholdPct: warn when X% of interval consumed
--
--   FirstThreshold can equal RepeatInterval (standard case) or
--   differ (escalation case: 3000 FH first, then 1500 FH).
--
--   EXAMPLES:
--
--   Hydraulic pump (FH line):
--     FirstThreshold=60000  (1000 FH in minutes)
--     RepeatInterval=36000  (600 FH after first done)
--     Ceiling=336000        (5600 FH hard life)
--     → First OH at 1000 FH, then every 600 FH, stop at 5600 FH
--
--   Engine borescope (FH + Calendar OR logic):
--     Line 1: FH:  First=180000(3000FH), Repeat=90000(1500FH), Ceiling=NULL
--     Line 2: CAL: First=730(24mo),      Repeat=365(12mo),     Ceiling=NULL
--     → Due at whichever comes first: 3000 FH or 24 months
--       Then every 1500 FH or 12 months, forever
--
--   O-ring shelf life (calendar, no repeat):
--     FirstThreshold=730   (24 months from cure date)
--     RepeatInterval=NULL  (discard — no repeat)
--     Ceiling=730          (same as first — one-time limit)
--
-- TABLE CREATION ORDER:
--   1. mro2.TaskCounter
--   2. mro2.SNTaskCounterState
--   3. mro2.SNTaskCounterOverride
--   4. Indexes
--   5. View: vw_SNTaskCounterStatus
--   6. SPs
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- 1. mro2.TaskCounter
--    One row per counter line per PNLimit.
--    A PNLimit can have 1..N rows (FH line, FC line, CAL line).
--    OR logic: task due when any row's SNTaskCounterState
--    reaches NextDueAt.
--
--    RepeatInterval NULL = one-time limit (discard/scrap).
--    Ceiling NULL        = no hard cap, repeats forever.
--    FirstThreshold=RepeatInterval = standard equal-interval case.
-- ============================================================
IF OBJECT_ID('mro2.TaskCounter','U') IS NULL
BEGIN
    CREATE TABLE mro2.TaskCounter (
        TaskCounterId       INT             NOT NULL IDENTITY(1,1),
        PNLimitId           INT             NOT NULL,   -- FK → mro2.PNLimit
        CounterDefId        INT             NOT NULL,   -- FK → mro2.CounterDef
        CounterBasisId      TINYINT         NOT NULL,   -- FK → mro2.CounterBasis

        -- First threshold: one-time initial due point from basis
        -- Stored in CounterDef.UnitStorage units (MINUTES or COUNT)
        FirstThreshold      INT             NOT NULL,

        -- Repeat interval after first done
        -- NULL = one-time limit (no repeat — discard/scrap)
        RepeatInterval      INT             NULL,

        -- Absolute maximum (NULL = no ceiling, repeats forever)
        -- Stored in same UnitStorage units
        Ceiling             INT             NULL,

        -- Alert fires when AccumulatedSinceLast >=
        --   CurrentInterval * AlertThresholdPct / 100
        AlertThresholdPct   TINYINT         NOT NULL
            CONSTRAINT DF_TaskCounter_AlertPct  DEFAULT (90),

        -- Display label shown on dashboard next to this counter line
        -- e.g. "3000 FH or 24 months — whichever first"
        -- Auto-generated if NULL
        DisplayLabel        NVARCHAR(200)   NULL,

        Notes               NVARCHAR(300)   NULL,
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_TaskCounter_IsActive  DEFAULT (1),
        CreatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_TaskCounter_Created   DEFAULT (GETDATE()),
        CreatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_TaskCounter PRIMARY KEY (TaskCounterId),

        -- One counter line per CounterDef per PNLimit
        -- (prevents duplicate FH lines on same task)
        CONSTRAINT UQ_TaskCounter_Limit_Counter
            UNIQUE (PNLimitId, CounterDefId),

        CONSTRAINT FK_TaskCounter_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId),

        CONSTRAINT FK_TaskCounter_CounterDef FOREIGN KEY (CounterDefId)
            REFERENCES mro2.CounterDef (CounterDefId),

        CONSTRAINT FK_TaskCounter_CounterBasis FOREIGN KEY (CounterBasisId)
            REFERENCES mro2.CounterBasis (CounterBasisId),

        CONSTRAINT CK_TaskCounter_FirstThreshold
            CHECK (FirstThreshold > 0),

        CONSTRAINT CK_TaskCounter_RepeatInterval
            CHECK (RepeatInterval IS NULL OR RepeatInterval > 0),

        CONSTRAINT CK_TaskCounter_Ceiling
            CHECK (Ceiling IS NULL OR Ceiling >= FirstThreshold),

        CONSTRAINT CK_TaskCounter_AlertPct
            CHECK (AlertThresholdPct BETWEEN 1 AND 99)
    );
END
GO

-- ============================================================
-- 2. mro2.SNTaskCounterState
--    Live state per SN per TaskCounter line.
--    One row per (SerializedItemId, TaskCounterId).
--
--    IsFirstDone     : has the first threshold been reached?
--                      FALSE = comparing against FirstThreshold
--                      TRUE  = comparing against RepeatInterval
--    AccumulatedSinceLast : counter value since last task done
--                      (resets to 0 on each task accomplishment)
--    LifetimeTotal   : total since CounterBasis event (never resets)
--                      Used to check against Ceiling
--    NextDueAt       : stored computed value of next due point
--                      = LastDoneAt + CurrentInterval
--                      Recomputed on every update for dashboard speed
--    LastDoneAt      : lifetime value when task was last done
--    LastDoneDate    : calendar date of last accomplishment
--    DoneCount       : how many times this task has been done
--
--    Status values:
--      OK       : within interval, not alerting
--      ALERT    : within AlertThresholdPct of due
--      DUE      : at or past NextDueAt
--      OVERDUE  : past NextDueAt + grace (future: configurable)
--      EXPIRED  : Ceiling reached — component must be removed
--      COMPLETE : one-time limit done, RepeatInterval IS NULL
-- ============================================================
IF OBJECT_ID('mro2.SNTaskCounterState','U') IS NULL
BEGIN
    CREATE TABLE mro2.SNTaskCounterState (
        SNTaskCounterStateId    INT             NOT NULL IDENTITY(1,1),
        SerializedItemId        INT             NOT NULL,
        TaskCounterId           INT             NOT NULL,

        -- First threshold tracking
        IsFirstDone             BIT             NOT NULL
            CONSTRAINT DF_SNTCState_IsFirstDone DEFAULT (0),

        -- Current interval accumulator (resets on task done)
        AccumulatedSinceLast    INT             NOT NULL
            CONSTRAINT DF_SNTCState_Accum       DEFAULT (0),

        -- Lifetime total since CounterBasis event
        LifetimeTotal           INT             NOT NULL
            CONSTRAINT DF_SNTCState_Lifetime    DEFAULT (0),

        -- Next due point (stored, recomputed on every update)
        -- = LifetimeAtLastDone + CurrentInterval
        -- CurrentInterval = FirstThreshold (if !IsFirstDone)
        --                 = RepeatInterval  (if IsFirstDone)
        NextDueAt               INT             NOT NULL
            CONSTRAINT DF_SNTCState_NextDueAt   DEFAULT (0),

        -- Last accomplishment
        LastDoneAt              INT             NULL,   -- lifetime value when done
        LastDoneDate            DATE            NULL,
        DoneCount               SMALLINT        NOT NULL
            CONSTRAINT DF_SNTCState_DoneCount   DEFAULT (0),

        -- Stored status (fast dashboard)
        -- Recomputed dynamically in view for reports
        CounterStatus           VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNTCState_Status      DEFAULT ('OK'),

        -- Stored remaining (fast dashboard)
        RemainingToNextDue      INT             NOT NULL
            CONSTRAINT DF_SNTCState_Remaining   DEFAULT (0),

        -- Pct consumed of current interval (0-100+)
        PctConsumed             DECIMAL(5,1)    NOT NULL
            CONSTRAINT DF_SNTCState_PctConsumed DEFAULT (0),

        -- Source of last update
        ValueSource             VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNTCState_Source      DEFAULT ('MANUAL'),

        LastUpdatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_SNTCState_Updated     DEFAULT (GETDATE()),
        LastUpdatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNTaskCounterState PRIMARY KEY (SNTaskCounterStateId),

        CONSTRAINT UQ_SNTaskCounterState_SN_TC
            UNIQUE (SerializedItemId, TaskCounterId),

        CONSTRAINT FK_SNTCState_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNTCState_TC FOREIGN KEY (TaskCounterId)
            REFERENCES mro2.TaskCounter (TaskCounterId),

        CONSTRAINT CK_SNTCState_Status
            CHECK (CounterStatus IN
                ('OK','ALERT','DUE','OVERDUE','EXPIRED','COMPLETE')),

        CONSTRAINT CK_SNTCState_Source
            CHECK (ValueSource IN ('AUTO','MANUAL','PRORATE'))
    );
END
GO

-- ============================================================
-- 3. mro2.SNTaskCounterOverride
--    SN-level override of a single TaskCounter line.
--    Overrides FirstThreshold, RepeatInterval, Ceiling,
--    AlertThresholdPct for this specific SN.
--    OverrideReason is mandatory (audit trail).
--
--    Example: hydraulic pump SN-XYZ requires shorter interval
--    per manufacturer SB-HYD-2024-003 after 3 accomplishments.
--    → Override RepeatInterval from 36000 to 24000 (600→400 FH)
-- ============================================================
IF OBJECT_ID('mro2.SNTaskCounterOverride','U') IS NULL
BEGIN
    CREATE TABLE mro2.SNTaskCounterOverride (
        SNTaskCounterOverrideId INT             NOT NULL IDENTITY(1,1),
        SerializedItemId        INT             NOT NULL,
        TaskCounterId           INT             NOT NULL,

        -- Override values (NULL = keep PN default for that field)
        FirstThreshold          INT             NULL,
        RepeatInterval          INT             NULL,
        Ceiling                 INT             NULL,
        AlertThresholdPct       TINYINT         NULL,

        -- Mandatory audit fields
        OverrideReason          NVARCHAR(500)   NOT NULL,
        AuthorisedBy            NVARCHAR(100)   NULL,
        AuthorisedRef           NVARCHAR(100)   NULL,   -- SB/AD/EO ref
        EffectiveDate           DATE            NOT NULL,
        ExpiryDate              DATE            NULL,   -- NULL = permanent

        IsActive                BIT             NOT NULL
            CONSTRAINT DF_SNTCOverride_IsActive DEFAULT (1),
        CreatedDate             DATETIME        NOT NULL
            CONSTRAINT DF_SNTCOverride_Created  DEFAULT (GETDATE()),
        CreatedByUserId         NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNTaskCounterOverride PRIMARY KEY (SNTaskCounterOverrideId),

        -- One active override per SN per counter line
        CONSTRAINT UQ_SNTaskCounterOverride_SN_TC
            UNIQUE (SerializedItemId, TaskCounterId),

        CONSTRAINT FK_SNTCOverride_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNTCOverride_TC FOREIGN KEY (TaskCounterId)
            REFERENCES mro2.TaskCounter (TaskCounterId),

        CONSTRAINT CK_SNTCOverride_AlertPct
            CHECK (AlertThresholdPct IS NULL
                   OR AlertThresholdPct BETWEEN 1 AND 99)
    );
END
GO

-- ============================================================
-- INDEXES
-- ============================================================

-- Fast SN lookup across all counter lines
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_SNTaskCounterState_SN'
    AND object_id=OBJECT_ID('mro2.SNTaskCounterState'))
    CREATE INDEX IX_SNTaskCounterState_SN
        ON mro2.SNTaskCounterState (SerializedItemId)
        INCLUDE (TaskCounterId, CounterStatus,
                 RemainingToNextDue, NextDueAt, PctConsumed);
GO

-- Dashboard: all non-OK SNs
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_SNTaskCounterState_Status'
    AND object_id=OBJECT_ID('mro2.SNTaskCounterState'))
    CREATE INDEX IX_SNTaskCounterState_Status
        ON mro2.SNTaskCounterState (CounterStatus)
        INCLUDE (SerializedItemId, TaskCounterId,
                 RemainingToNextDue, PctConsumed);
GO

-- Fast TaskCounter lookup by PNLimit
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_TaskCounter_PNLimitId'
    AND object_id=OBJECT_ID('mro2.TaskCounter'))
    CREATE INDEX IX_TaskCounter_PNLimitId
        ON mro2.TaskCounter (PNLimitId)
        INCLUDE (CounterDefId, CounterBasisId,
                 FirstThreshold, RepeatInterval, Ceiling);
GO

-- ============================================================
-- VIEW: mro2.vw_SNTaskCounterStatus
--    Master status view — resolves SN override vs PN default
--    for every counter line. Recomputes all derived values
--    dynamically for report accuracy.
--    Dual-storage: stored values for dashboard speed,
--    computed values for reporting accuracy.
-- ============================================================
IF OBJECT_ID('mro2.vw_SNTaskCounterStatus','V') IS NOT NULL
    DROP VIEW mro2.vw_SNTaskCounterStatus;
GO
CREATE VIEW mro2.vw_SNTaskCounterStatus
AS
SELECT
    -- Identity
    si.SerializedItemId,
    si.SerialNumber,
    pn.PartNumberId,
    pn.PN,
    pn.Nomenclature,
    pl.PNLimitId,
    tc.TaskCounterId,

    -- Counter definition
    cd.CounterDefId,
    cd.Code                                         AS CounterDefCode,
    cd.AppliesToAssetKindCode,
    ct.CounterTypeId,
    ct.Code                                         AS CounterTypeCode,
    ct.DisplayUnit,
    ct.UnitStorage,

    -- Counter basis
    cb.CounterBasisId,
    cb.Code                                         AS CounterBasisCode,
    cb.Name                                         AS CounterBasisName,

    -- Effective values: SN override wins field-by-field if exists
    -- FirstThreshold
    COALESCE(ov.FirstThreshold,   tc.FirstThreshold)    AS EffFirstThreshold,
    -- RepeatInterval (NULL = one-time)
    COALESCE(ov.RepeatInterval,   tc.RepeatInterval)     AS EffRepeatInterval,
    -- Ceiling (NULL = no ceiling)
    COALESCE(ov.Ceiling,          tc.Ceiling)            AS EffCeiling,
    -- AlertThresholdPct
    COALESCE(ov.AlertThresholdPct,tc.AlertThresholdPct)  AS EffAlertPct,

    -- Override flag
    CASE WHEN ov.SNTaskCounterOverrideId IS NOT NULL
         THEN 'OVERRIDE' ELSE 'PN_DEFAULT' END           AS ValueSource,
    ov.OverrideReason,
    ov.AuthorisedRef,

    -- State values
    st.IsFirstDone,
    st.AccumulatedSinceLast,
    st.LifetimeTotal,
    st.NextDueAt,
    st.LastDoneAt,
    st.LastDoneDate,
    st.DoneCount,

    -- Current interval in effect
    -- Before first done: FirstThreshold
    -- After first done:  RepeatInterval (or NULL if one-time)
    CASE WHEN st.IsFirstDone = 0
         THEN COALESCE(ov.FirstThreshold,  tc.FirstThreshold)
         ELSE COALESCE(ov.RepeatInterval,  tc.RepeatInterval)
    END                                                 AS CurrentInterval,

    -- Dynamically recomputed remaining (authoritative for reports)
    COALESCE(ov.Ceiling, tc.Ceiling) -
        st.LifetimeTotal                                AS RemainingToCeilingCalc,

    -- Remaining to next due
    CASE WHEN st.NextDueAt > 0
         THEN st.NextDueAt - st.LifetimeTotal
         ELSE COALESCE(ov.FirstThreshold, tc.FirstThreshold)
              - st.LifetimeTotal
    END                                                 AS RemainingToNextDueCalc,

    -- Stored remaining (fast dashboard)
    st.RemainingToNextDue                               AS RemainingToNextDueStored,

    -- Alert threshold value in current interval units
    CASE WHEN st.IsFirstDone = 0
         THEN COALESCE(ov.FirstThreshold, tc.FirstThreshold)
              * COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
              / 100
         ELSE COALESCE(ov.RepeatInterval, tc.RepeatInterval)
              * COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
              / 100
    END                                                 AS AlertAtValue,

    -- Pct consumed of current interval
    st.PctConsumed,

    -- Dynamically recomputed status (authoritative for reports)
    CASE
        -- Ceiling reached → EXPIRED
        WHEN COALESCE(ov.Ceiling, tc.Ceiling) IS NOT NULL
         AND st.LifetimeTotal >= COALESCE(ov.Ceiling, tc.Ceiling)
        THEN 'EXPIRED'
        -- One-time task already done → COMPLETE
        WHEN st.IsFirstDone = 1
         AND COALESCE(ov.RepeatInterval, tc.RepeatInterval) IS NULL
        THEN 'COMPLETE'
        -- Past NextDueAt → DUE
        WHEN st.NextDueAt > 0
         AND st.LifetimeTotal >= st.NextDueAt
        THEN 'DUE'
        -- Within alert zone → ALERT
        WHEN st.NextDueAt > 0
         AND st.LifetimeTotal >=
             st.NextDueAt -
             (CASE WHEN st.IsFirstDone = 0
                   THEN COALESCE(ov.FirstThreshold, tc.FirstThreshold)
                   ELSE COALESCE(ov.RepeatInterval,  tc.RepeatInterval)
              END
              * COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
              / 100
             ) * (1 - COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct) / 100.0)
        THEN 'ALERT'
        ELSE 'OK'
    END                                                 AS CounterStatusCalc,

    -- Stored status (fast dashboard)
    st.CounterStatus                                    AS CounterStatusStored,

    -- Display label
    ISNULL(tc.DisplayLabel,
           cd.Code + N' — ' +
           CAST(COALESCE(ov.FirstThreshold, tc.FirstThreshold) AS NVARCHAR)
           + N' ' + ct.DisplayUnit
           + CASE WHEN COALESCE(ov.RepeatInterval, tc.RepeatInterval)
                       <> COALESCE(ov.FirstThreshold, tc.FirstThreshold)
                  THEN N' then every ' +
                       CAST(COALESCE(ov.RepeatInterval,
                                     tc.RepeatInterval) AS NVARCHAR)
                       + N' ' + ct.DisplayUnit
                  ELSE N'' END
           + CASE WHEN COALESCE(ov.Ceiling, tc.Ceiling) IS NULL
                  THEN N' (no ceiling)'
                  ELSE N' until ' +
                       CAST(COALESCE(ov.Ceiling, tc.Ceiling) AS NVARCHAR)
                       + N' ' + ct.DisplayUnit
             END)                                       AS DisplayLabel,

    st.LastUpdatedDate,
    tc.Notes

FROM mro2.TaskCounter tc

INNER JOIN mro2.PNLimit            pl  ON pl.PNLimitId        = tc.PNLimitId
INNER JOIN mro2.CounterDef         cd  ON cd.CounterDefId     = tc.CounterDefId
INNER JOIN mro2.CounterType        ct  ON ct.CounterTypeId    = cd.CounterTypeId
INNER JOIN mro2.CounterBasis       cb  ON cb.CounterBasisId   = tc.CounterBasisId
INNER JOIN mro2.SerializedItem     si  ON si.PartNumberId     = pl.PartNumberId
INNER JOIN mro2.PartNumber         pn  ON pn.PartNumberId     = pl.PartNumberId

-- Live state (may not exist yet if SN never updated)
LEFT JOIN mro2.SNTaskCounterState  st  ON st.SerializedItemId = si.SerializedItemId
                                      AND st.TaskCounterId    = tc.TaskCounterId

-- SN override (field-by-field — active and non-expired)
LEFT JOIN mro2.SNTaskCounterOverride ov ON ov.SerializedItemId = si.SerializedItemId
                                       AND ov.TaskCounterId   = tc.TaskCounterId
                                       AND ov.IsActive        = 1
                                       AND (ov.ExpiryDate IS NULL
                                            OR ov.ExpiryDate >= CAST(GETDATE() AS DATE))

WHERE tc.IsActive = 1
  AND si.IsActive = 1
  AND pn.IsActive = 1;
GO

-- ============================================================
-- SP: mro2.usp_SNTaskCounterState_Update
--    Single entry point for all counter updates.
--    Called after every counter feed (TechLog auto, manual,
--    prorate). Resolves override, recomputes all fields,
--    upserts SNTaskCounterState.
--
--    @NewLifetimeTotal : updated absolute total since basis
--    @ValueSource      : AUTO | MANUAL | PRORATE
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounterState_Update','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounterState_Update;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounterState_Update
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @NewLifetimeTotal   INT,
    @ValueSource        VARCHAR(10),
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- ── 1. Resolve effective values (override wins per field) ─
    DECLARE @EffFirst    INT;
    DECLARE @EffRepeat   INT;
    DECLARE @EffCeiling  INT;
    DECLARE @EffAlertPct TINYINT;

    SELECT
        @EffFirst    = COALESCE(ov.FirstThreshold,    tc.FirstThreshold),
        @EffRepeat   = COALESCE(ov.RepeatInterval,    tc.RepeatInterval),
        @EffCeiling  = COALESCE(ov.Ceiling,           tc.Ceiling),
        @EffAlertPct = COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
    FROM mro2.TaskCounter tc
    LEFT JOIN mro2.SNTaskCounterOverride ov
           ON ov.TaskCounterId    = tc.TaskCounterId
          AND ov.SerializedItemId = @SerializedItemId
          AND ov.IsActive         = 1
          AND (ov.ExpiryDate IS NULL
               OR ov.ExpiryDate >= CAST(GETDATE() AS DATE))
    WHERE tc.TaskCounterId = @TaskCounterId;

    -- ── 2. Get current state ──────────────────────────────────
    DECLARE @IsFirstDone    BIT       = 0;
    DECLARE @LastDoneAt     INT       = NULL;
    DECLARE @LastDoneDate   DATE      = NULL;
    DECLARE @DoneCount      SMALLINT  = 0;
    DECLARE @OldLifetime    INT       = 0;

    SELECT
        @IsFirstDone  = IsFirstDone,
        @LastDoneAt   = LastDoneAt,
        @LastDoneDate = LastDoneDate,
        @DoneCount    = DoneCount,
        @OldLifetime  = LifetimeTotal
    FROM mro2.SNTaskCounterState
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId;

    -- ── 3. Determine current interval and NextDueAt ───────────
    DECLARE @CurrentInterval INT;
    DECLARE @NextDueAt       INT;

    IF @IsFirstDone = 0
    BEGIN
        -- Still working toward first threshold
        SET @CurrentInterval = @EffFirst;
        SET @NextDueAt       = @EffFirst;   -- from basis origin (0)
    END
    ELSE
    BEGIN
        -- First done — using repeat interval
        SET @CurrentInterval = @EffRepeat;
        -- NextDueAt = last done point + repeat interval
        SET @NextDueAt = ISNULL(@LastDoneAt, 0) + @EffRepeat;
    END

    -- ── 4. Accumulated since last done ────────────────────────
    DECLARE @AccumulatedSinceLast INT =
        @NewLifetimeTotal - ISNULL(@LastDoneAt, 0);

    -- ── 5. Compute status ─────────────────────────────────────
    DECLARE @AlertAtValue INT =
        @NextDueAt - (@CurrentInterval * @EffAlertPct / 100);

    DECLARE @Status VARCHAR(10);

    IF @EffCeiling IS NOT NULL AND @NewLifetimeTotal >= @EffCeiling
        SET @Status = 'EXPIRED'
    ELSE IF @IsFirstDone = 1 AND @EffRepeat IS NULL
        SET @Status = 'COMPLETE'
    ELSE IF @NewLifetimeTotal >= @NextDueAt
        SET @Status = 'DUE'
    ELSE IF @NewLifetimeTotal >= @AlertAtValue
        SET @Status = 'ALERT'
    ELSE
        SET @Status = 'OK'

    -- ── 6. Remaining and pct consumed ─────────────────────────
    DECLARE @Remaining   INT           = @NextDueAt - @NewLifetimeTotal;
    DECLARE @PctConsumed DECIMAL(5,1)  = 0;

    IF @CurrentInterval > 0
        SET @PctConsumed = CAST(
            @AccumulatedSinceLast * 100.0 / @CurrentInterval
            AS DECIMAL(5,1));

    -- Cap pct at 100+ (over-run shown as >100%)
    IF @PctConsumed > 999.9 SET @PctConsumed = 999.9;

    -- ── 7. Upsert SNTaskCounterState ──────────────────────────
    IF EXISTS (
        SELECT 1 FROM mro2.SNTaskCounterState
        WHERE SerializedItemId = @SerializedItemId
          AND TaskCounterId    = @TaskCounterId)
    BEGIN
        UPDATE mro2.SNTaskCounterState SET
            AccumulatedSinceLast  = @AccumulatedSinceLast,
            LifetimeTotal         = @NewLifetimeTotal,
            NextDueAt             = @NextDueAt,
            CounterStatus         = @Status,
            RemainingToNextDue    = @Remaining,
            PctConsumed           = @PctConsumed,
            ValueSource           = @ValueSource,
            LastUpdatedDate       = GETDATE(),
            LastUpdatedByUserId   = @UserId
        WHERE SerializedItemId = @SerializedItemId
          AND TaskCounterId    = @TaskCounterId;
    END
    ELSE
    BEGIN
        INSERT INTO mro2.SNTaskCounterState (
            SerializedItemId, TaskCounterId,
            IsFirstDone, AccumulatedSinceLast, LifetimeTotal,
            NextDueAt, LastDoneAt, LastDoneDate, DoneCount,
            CounterStatus, RemainingToNextDue, PctConsumed,
            ValueSource, LastUpdatedDate, LastUpdatedByUserId)
        VALUES (
            @SerializedItemId, @TaskCounterId,
            0, @AccumulatedSinceLast, @NewLifetimeTotal,
            @NextDueAt, NULL, NULL, 0,
            @Status, @Remaining, @PctConsumed,
            @ValueSource, GETDATE(), @UserId);
    END

    -- ── 8. Return state to caller ─────────────────────────────
    SELECT
        @TaskCounterId          AS TaskCounterId,
        @IsFirstDone            AS IsFirstDone,
        @CurrentInterval        AS CurrentInterval,
        @AccumulatedSinceLast   AS AccumulatedSinceLast,
        @NewLifetimeTotal       AS LifetimeTotal,
        @NextDueAt              AS NextDueAt,
        @Remaining              AS RemainingToNextDue,
        @PctConsumed            AS PctConsumed,
        @Status                 AS CounterStatus;
END
GO

-- ============================================================
-- SP: mro2.usp_SNTaskCounter_RecordAccomplishment
--    Called when a maintenance task is signed off.
--    Marks IsFirstDone=1 (if not already), resets
--    AccumulatedSinceLast to 0, advances NextDueAt,
--    increments DoneCount. Writes the accomplishment
--    and then calls Update to recompute status.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounter_RecordAccomplishment','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounter_RecordAccomplishment;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounter_RecordAccomplishment
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @AccomplishedAt     INT,        -- lifetime value at accomplishment
    @AccomplishedDate   DATE,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Resolve effective repeat interval
    DECLARE @EffRepeat INT;
    SELECT @EffRepeat = COALESCE(ov.RepeatInterval, tc.RepeatInterval)
    FROM mro2.TaskCounter tc
    LEFT JOIN mro2.SNTaskCounterOverride ov
           ON ov.TaskCounterId    = tc.TaskCounterId
          AND ov.SerializedItemId = @SerializedItemId
          AND ov.IsActive         = 1
          AND (ov.ExpiryDate IS NULL
               OR ov.ExpiryDate >= CAST(GETDATE() AS DATE))
    WHERE tc.TaskCounterId = @TaskCounterId;

    -- Update state: mark done, reset accumulator, advance due
    UPDATE mro2.SNTaskCounterState SET
        IsFirstDone          = 1,
        AccumulatedSinceLast = 0,
        LastDoneAt           = @AccomplishedAt,
        LastDoneDate         = @AccomplishedDate,
        DoneCount            = DoneCount + 1,
        -- Next due = accomplished point + repeat interval
        NextDueAt            = CASE
                                   WHEN @EffRepeat IS NOT NULL
                                   THEN @AccomplishedAt + @EffRepeat
                                   ELSE NextDueAt   -- one-time, no advance
                               END,
        LastUpdatedDate      = GETDATE(),
        LastUpdatedByUserId  = @UserId
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId;

    -- If no row yet (first accomplishment ever), insert
    IF @@ROWCOUNT = 0
    BEGIN
        DECLARE @EffFirst    INT;
        DECLARE @EffCeiling  INT;
        DECLARE @EffAlertPct TINYINT;

        SELECT
            @EffFirst    = COALESCE(ov.FirstThreshold,    tc.FirstThreshold),
            @EffCeiling  = COALESCE(ov.Ceiling,           tc.Ceiling),
            @EffAlertPct = COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
        FROM mro2.TaskCounter tc
        LEFT JOIN mro2.SNTaskCounterOverride ov
               ON ov.TaskCounterId    = tc.TaskCounterId
              AND ov.SerializedItemId = @SerializedItemId
              AND ov.IsActive         = 1
        WHERE tc.TaskCounterId = @TaskCounterId;

        INSERT INTO mro2.SNTaskCounterState (
            SerializedItemId, TaskCounterId,
            IsFirstDone, AccumulatedSinceLast, LifetimeTotal,
            NextDueAt, LastDoneAt, LastDoneDate, DoneCount,
            CounterStatus, RemainingToNextDue, PctConsumed,
            ValueSource, LastUpdatedDate, LastUpdatedByUserId)
        VALUES (
            @SerializedItemId, @TaskCounterId,
            1, 0, @AccomplishedAt,
            @AccomplishedAt + ISNULL(@EffRepeat, 0),
            @AccomplishedAt, @AccomplishedDate, 1,
            'OK', ISNULL(@EffRepeat, 0), 0,
            'MANUAL', GETDATE(), @UserId);
    END

    -- Recompute status after accomplishment
    EXEC mro2.usp_SNTaskCounterState_Update
        @SerializedItemId = @SerializedItemId,
        @TaskCounterId    = @TaskCounterId,
        @NewLifetimeTotal = @AccomplishedAt,
        @ValueSource      = 'MANUAL',
        @UserId           = @UserId;
END
GO

-- ============================================================
-- SP: mro2.usp_TaskCounter_GetBySN
--    All counter lines for a given SN with full status.
--    Used by SN detail page. Returns one row per counter line
--    showing both stored (fast) and computed (accurate) values.
-- ============================================================
IF OBJECT_ID('mro2.usp_TaskCounter_GetBySN','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_TaskCounter_GetBySN;
GO
CREATE PROCEDURE mro2.usp_TaskCounter_GetBySN
    @SerializedItemId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_SNTaskCounterStatus
    WHERE SerializedItemId = @SerializedItemId
    ORDER BY PNLimitId, CounterTypeId;
END
GO

-- ============================================================
-- SP: mro2.usp_TaskCounter_GetDashboard
--    All SNs with non-OK status across all counter lines.
--    Smart scheduling: shows RemainingToNextDueCalc for every
--    counter line so scheduler sees both FH remaining AND
--    calendar remaining simultaneously.
--    Ordered: EXPIRED → DUE → ALERT, then by remaining ASC.
-- ============================================================
IF OBJECT_ID('mro2.usp_TaskCounter_GetDashboard','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_TaskCounter_GetDashboard;
GO
CREATE PROCEDURE mro2.usp_TaskCounter_GetDashboard
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        SerializedItemId,
        SerialNumber,
        PN,
        Nomenclature,
        PNLimitId,
        TaskCounterId,
        CounterDefCode,
        CounterTypeCode,
        DisplayUnit,
        UnitStorage,
        CounterBasisCode,
        IsFirstDone,
        EffFirstThreshold,
        EffRepeatInterval,
        EffCeiling,
        CurrentInterval,
        AccumulatedSinceLast,
        LifetimeTotal,
        NextDueAt,
        RemainingToNextDueCalc,
        RemainingToCeilingCalc,
        AlertAtValue,
        PctConsumed,
        CounterStatusCalc,
        LastDoneDate,
        DoneCount,
        DisplayLabel,
        ValueSource,
        OverrideReason,
        AuthorisedRef
    FROM mro2.vw_SNTaskCounterStatus
    WHERE CounterStatusCalc IN ('EXPIRED','DUE','ALERT')
    ORDER BY
        CASE CounterStatusCalc
            WHEN 'EXPIRED' THEN 0
            WHEN 'DUE'     THEN 1
            WHEN 'ALERT'   THEN 2
            ELSE 3
        END,
        RemainingToNextDueCalc ASC;
END
GO

-- ============================================================
-- SP: mro2.usp_TaskCounter_List (for PNLimitList modal grid)
--    Returns all TaskCounter rows for a given PNLimit.
--    Used by the "Limits" modal in PartNumberList.aspx.
-- ============================================================
IF OBJECT_ID('mro2.usp_TaskCounter_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_TaskCounter_List;
GO
CREATE PROCEDURE mro2.usp_TaskCounter_List
    @PNLimitId          INT,
    @IncludeInactive    BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        tc.TaskCounterId,
        tc.PNLimitId,
        tc.CounterDefId,
        cd.Code             AS CounterDefCode,
        cd.Name             AS CounterDefName,
        cd.AppliesToAssetKindCode,
        ct.Code             AS CounterTypeCode,
        ct.DisplayUnit,
        ct.UnitStorage,
        tc.CounterBasisId,
        cb.Code             AS CounterBasisCode,
        cb.Name             AS CounterBasisName,
        tc.FirstThreshold,
        tc.RepeatInterval,
        tc.Ceiling,
        tc.AlertThresholdPct,
        tc.DisplayLabel,
        tc.Notes,
        tc.IsActive,
        tc.CreatedDate,
        -- SN count: how many SNs of this PN have state records
        ISNULL(snc.SNCount, 0) AS SNCount
    FROM mro2.TaskCounter tc
    INNER JOIN mro2.CounterDef   cd ON cd.CounterDefId  = tc.CounterDefId
    INNER JOIN mro2.CounterType  ct ON ct.CounterTypeId = cd.CounterTypeId
    INNER JOIN mro2.CounterBasis cb ON cb.CounterBasisId= tc.CounterBasisId
    LEFT  JOIN (
        SELECT TaskCounterId, COUNT(*) AS SNCount
        FROM mro2.SNTaskCounterState
        GROUP BY TaskCounterId
    ) snc ON snc.TaskCounterId = tc.TaskCounterId
    WHERE tc.PNLimitId = @PNLimitId
      AND (@IncludeInactive = 1 OR tc.IsActive = 1)
    ORDER BY ct.SortOrder, cd.Code;
END
GO

-- ============================================================
-- SP: mro2.usp_TaskCounter_Save
-- ============================================================
IF OBJECT_ID('mro2.usp_TaskCounter_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_TaskCounter_Save;
GO
CREATE PROCEDURE mro2.usp_TaskCounter_Save
    @TaskCounterId      INT             = NULL,
    @PNLimitId          INT,
    @CounterDefId       INT,
    @CounterBasisId     TINYINT,
    @FirstThreshold     INT,
    @RepeatInterval     INT             = NULL,
    @Ceiling            INT             = NULL,
    @AlertThresholdPct  TINYINT,
    @DisplayLabel       NVARCHAR(200)   = NULL,
    @Notes              NVARCHAR(300)   = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @TaskCounterId IS NULL
    BEGIN
        INSERT INTO mro2.TaskCounter (
            PNLimitId, CounterDefId, CounterBasisId,
            FirstThreshold, RepeatInterval, Ceiling,
            AlertThresholdPct, DisplayLabel, Notes,
            IsActive, CreatedDate, CreatedByUserId)
        VALUES (
            @PNLimitId, @CounterDefId, @CounterBasisId,
            @FirstThreshold, @RepeatInterval, @Ceiling,
            @AlertThresholdPct, @DisplayLabel, @Notes,
            1, GETDATE(), @UserId);
        SELECT SCOPE_IDENTITY() AS TaskCounterId;
    END
    ELSE
    BEGIN
        UPDATE mro2.TaskCounter SET
            CounterDefId       = @CounterDefId,
            CounterBasisId     = @CounterBasisId,
            FirstThreshold     = @FirstThreshold,
            RepeatInterval     = @RepeatInterval,
            Ceiling            = @Ceiling,
            AlertThresholdPct  = @AlertThresholdPct,
            DisplayLabel       = @DisplayLabel,
            Notes              = @Notes
        WHERE TaskCounterId = @TaskCounterId;
        SELECT @TaskCounterId AS TaskCounterId;
    END
END
GO

-- ============================================================
-- SP: mro2.usp_TaskCounter_SetActive
-- ============================================================
IF OBJECT_ID('mro2.usp_TaskCounter_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_TaskCounter_SetActive;
GO
CREATE PROCEDURE mro2.usp_TaskCounter_SetActive
    @TaskCounterId INT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.TaskCounter SET IsActive = @IsActive
    WHERE TaskCounterId = @TaskCounterId;
END
GO

-- ============================================================
-- VERIFICATION
-- ============================================================
/*
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'mro2'
  AND TABLE_NAME IN ('TaskCounter','SNTaskCounterState',
                     'SNTaskCounterOverride')
ORDER BY TABLE_NAME;

SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'mro2'
  AND ROUTINE_NAME IN (
      'usp_SNTaskCounterState_Update',
      'usp_SNTaskCounter_RecordAccomplishment',
      'usp_TaskCounter_GetBySN',
      'usp_TaskCounter_GetDashboard',
      'usp_TaskCounter_List',
      'usp_TaskCounter_Save',
      'usp_TaskCounter_SetActive')
ORDER BY ROUTINE_NAME;

-- ── SAMPLE DATA — hydraulic pump + engine borescope ──────

-- Pump: FH counter (FirstThreshold ≠ RepeatInterval)
-- Assumes PNLimitId=1, CounterDefId for AF_FLIGHT_MIN, BasisId=2 (SINCE_NEW)
-- INSERT INTO mro2.TaskCounter
--   (PNLimitId,CounterDefId,CounterBasisId,
--    FirstThreshold,RepeatInterval,Ceiling,AlertThresholdPct,
--    DisplayLabel,CreatedByUserId)
-- VALUES
--   (1, 2, 2,
--    60000, 36000, 336000, 90,
--    N'1000 FH first, then 600 FH until 5600 FH', 'admin');

-- Borescope: FH line (no ceiling)
-- INSERT INTO mro2.TaskCounter
--   (PNLimitId,CounterDefId,CounterBasisId,
--    FirstThreshold,RepeatInterval,Ceiling,AlertThresholdPct,
--    DisplayLabel,CreatedByUserId)
-- VALUES
--   (2, 2, 2,
--    180000, 90000, NULL, 90,
--    N'3000 FH first, then 1500 FH — no ceiling', 'admin');

-- Borescope: Calendar line (same PNLimitId, OR logic)
-- INSERT INTO mro2.TaskCounter
--   (PNLimitId,CounterDefId,CounterBasisId,
--    FirstThreshold,RepeatInterval,Ceiling,AlertThresholdPct,
--    DisplayLabel,CreatedByUserId)
-- VALUES
--   (2, 10, 4,
--    730, 365, NULL, 90,
--    N'24 months first, then 12 months — no ceiling', 'admin');
*/
