-- ============================================================
-- MRO2 — PN/SN Life Limits Schema
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- Author  : Generated for RMAF 2BAFRA
-- ============================================================
-- TABLE CREATION ORDER (respects FK dependencies):
--   1. mro2.LimitType
--   2. mro2.PNLimit
--   3. mro2.SNLimitOverride
--   4. mro2.SNResetEvent
--   5. mro2.SNCounter
--   6. mro2.SNCounterLog
--   7. Views
--   8. Stored Procedures
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- 1. mro2.LimitType
--    Master lookup: the 5 counter types the system tracks.
--    Seeded once, never user-edited.
--    CounterUnit drives display formatting across all pages.
-- ============================================================
IF OBJECT_ID('mro2.LimitType', 'U') IS NULL
BEGIN
    CREATE TABLE mro2.LimitType (
        LimitTypeId     TINYINT         NOT NULL,   -- PK: 1=FH,2=FC,3=DAYS,4=TGO,5=FULLSTOP
        Code            VARCHAR(10)     NOT NULL,   -- 'FH','FC','DAYS','TGO','FULLSTOP'
        Name            NVARCHAR(50)    NOT NULL,   -- 'Flight Hours', 'Flight Cycles', etc.
        CounterUnit     VARCHAR(20)     NOT NULL,   -- 'hrs', 'cycles', 'days', 'ldg', 'ldg'
        IsDecimal       BIT             NOT NULL    -- 1=FH (can be 1234.5h), 0=integer counters
            CONSTRAINT DF_LimitType_IsDecimal DEFAULT (0),
        IsActive        BIT             NOT NULL
            CONSTRAINT DF_LimitType_IsActive  DEFAULT (1),

        CONSTRAINT PK_LimitType PRIMARY KEY (LimitTypeId),
        CONSTRAINT UQ_LimitType_Code UNIQUE (Code)
    );

    -- Seed the 5 counter types — fixed, never changes
    INSERT INTO mro2.LimitType (LimitTypeId, Code, Name, CounterUnit, IsDecimal)
    VALUES
        (1, 'FH',       'Flight Hours',          'hrs',    1),
        (2, 'FC',       'Flight Cycles',         'cycles', 0),
        (3, 'DAYS',     'Calendar Days',         'days',   0),
        (4, 'TGO',      'Touch-and-Go Landings', 'ldg',    0),
        (5, 'FULLSTOP', 'Full Stop Landings',    'ldg',    0);
END
GO

-- ============================================================
-- 2. mro2.PNLimit
--    Default limits defined at PN level.
--    One row per (PartNumberId, LimitTypeId) combination.
--    A PN can have multiple rows = multiple independent limits
--    (e.g. tire row 1: FULLSTOP limit, tire row 2: DAYS limit).
--
--    ResetTrigger:
--      'INSTALL'   = counter resets when SN is installed on a/c
--      'OVERHAUL'  = counter resets only on explicit overhaul event
--
--    AlertThresholdPct: soft alert fires when
--      AccumulatedSinceReset >= HardLimit * (AlertThresholdPct/100)
--    Example: HardLimit=500 FH, AlertThresholdPct=90 → alert at 450 FH
-- ============================================================
IF OBJECT_ID('mro2.PNLimit', 'U') IS NULL
BEGIN
    CREATE TABLE mro2.PNLimit (
        PNLimitId           INT             NOT NULL IDENTITY(1,1),
        PartNumberId        INT             NOT NULL,   -- FK → mro2.PartNumber
        LimitTypeId         TINYINT         NOT NULL,   -- FK → mro2.LimitType
        HardLimit           DECIMAL(10,1)   NOT NULL,   -- e.g. 500.0 FH, 300 FC, 365 days
        AlertThresholdPct   TINYINT         NOT NULL    -- 0–99, typically 85–95
            CONSTRAINT DF_PNLimit_AlertPct DEFAULT (90),
        ResetTrigger        VARCHAR(10)     NOT NULL    -- 'INSTALL' | 'OVERHAUL'
            CONSTRAINT DF_PNLimit_ResetTrigger DEFAULT ('INSTALL'),
        Notes               NVARCHAR(300)   NULL,       -- e.g. 'AMM 05-10-10 ref'
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_PNLimit_IsActive DEFAULT (1),
        CreatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_PNLimit_CreatedDate DEFAULT (GETDATE()),
        CreatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_PNLimit PRIMARY KEY (PNLimitId),

        -- One limit type per PN (prevents duplicate FH limits on same PN)
        CONSTRAINT UQ_PNLimit_PN_Type UNIQUE (PartNumberId, LimitTypeId),

        CONSTRAINT FK_PNLimit_PartNumber FOREIGN KEY (PartNumberId)
            REFERENCES mro2.PartNumber (PartNumberId),

        CONSTRAINT FK_PNLimit_LimitType FOREIGN KEY (LimitTypeId)
            REFERENCES mro2.LimitType (LimitTypeId),

        CONSTRAINT CK_PNLimit_HardLimit
            CHECK (HardLimit > 0),

        CONSTRAINT CK_PNLimit_AlertPct
            CHECK (AlertThresholdPct BETWEEN 1 AND 99),

        CONSTRAINT CK_PNLimit_ResetTrigger
            CHECK (ResetTrigger IN ('INSTALL', 'OVERHAUL'))
    );
END
GO

-- ============================================================
-- 3. mro2.SNLimitOverride
--    SN-level override of a PNLimit row.
--    If a row exists here for (SerializedItemId, PNLimitId),
--    its values take precedence over PNLimit for that SN.
--    If no row exists, the SN inherits from PNLimit (default).
--    Allows one SN to have a tighter limit (e.g. damaged item).
-- ============================================================
IF OBJECT_ID('mro2.SNLimitOverride', 'U') IS NULL
BEGIN
    CREATE TABLE mro2.SNLimitOverride (
        SNLimitOverrideId   INT             NOT NULL IDENTITY(1,1),
        SerializedItemId    INT             NOT NULL,   -- FK → mro2.SerializedItem
        PNLimitId           INT             NOT NULL,   -- FK → mro2.PNLimit (identifies PN+LimitType)
        HardLimit           DECIMAL(10,1)   NOT NULL,   -- overridden hard limit for this SN
        AlertThresholdPct   TINYINT         NOT NULL
            CONSTRAINT DF_SNLimitOverride_AlertPct DEFAULT (90),
        ResetTrigger        VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNLimitOverride_ResetTrigger DEFAULT ('INSTALL'),
        OverrideReason      NVARCHAR(300)   NULL,       -- why this SN gets different limit
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_SNLimitOverride_IsActive DEFAULT (1),
        CreatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_SNLimitOverride_CreatedDate DEFAULT (GETDATE()),
        CreatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNLimitOverride PRIMARY KEY (SNLimitOverrideId),

        -- One override per SN per PNLimit row
        CONSTRAINT UQ_SNLimitOverride_SN_Limit UNIQUE (SerializedItemId, PNLimitId),

        CONSTRAINT FK_SNLimitOverride_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNLimitOverride_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId),

        CONSTRAINT CK_SNLimitOverride_HardLimit
            CHECK (HardLimit > 0),

        CONSTRAINT CK_SNLimitOverride_AlertPct
            CHECK (AlertThresholdPct BETWEEN 1 AND 99),

        CONSTRAINT CK_SNLimitOverride_ResetTrigger
            CHECK (ResetTrigger IN ('INSTALL', 'OVERHAUL'))
    );
END
GO

-- ============================================================
-- 4. mro2.SNResetEvent
--    Records every time a limit counter was reset to zero.
--    ResetType mirrors the trigger: 'INSTALL' or 'OVERHAUL'.
--    InstallEventId links to the install record (future RecordEvent
--    table) — nullable for now, populated when RecordEvent is built.
--    At overhaul, shop order reference is stored in ShopOrderRef.
-- ============================================================
IF OBJECT_ID('mro2.SNResetEvent', 'U') IS NULL
BEGIN
    CREATE TABLE mro2.SNResetEvent (
        SNResetEventId      INT             NOT NULL IDENTITY(1,1),
        SerializedItemId    INT             NOT NULL,   -- FK → mro2.SerializedItem
        PNLimitId           INT             NOT NULL,   -- which limit was reset
        ResetType           VARCHAR(10)     NOT NULL,   -- 'INSTALL' | 'OVERHAUL'
        ResetDate           DATE            NOT NULL,   -- calendar date of the reset
        -- Counter values AT TIME OF RESET (snapshot for audit)
        FH_AtReset          DECIMAL(10,1)   NULL,       -- aircraft FH when reset occurred
        FC_AtReset          INT             NULL,       -- aircraft FC when reset occurred
        -- Link to events (populated as modules are built)
        InstallEventId      INT             NULL,       -- FK → mro2.RecordEvent (future)
        ShopOrderRef        NVARCHAR(50)    NULL,       -- overhaul work order number
        Notes               NVARCHAR(300)   NULL,
        CreatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_SNResetEvent_CreatedDate DEFAULT (GETDATE()),
        CreatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNResetEvent PRIMARY KEY (SNResetEventId),

        CONSTRAINT FK_SNResetEvent_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNResetEvent_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId),

        CONSTRAINT CK_SNResetEvent_Type
            CHECK (ResetType IN ('INSTALL', 'OVERHAUL'))
    );
END
GO

-- ============================================================
-- 5. mro2.SNCounter
--    Current accumulated counter values per SN per PNLimit.
--    One row per (SerializedItemId, PNLimitId).
--
--    AccumulatedSinceReset : running total since last SNResetEvent
--    LifetimeTotal         : never resets — total since SN was new
--    Remaining             : STORED = HardLimit - AccumulatedSinceReset
--                            (recalculated on every update for perf,
--                             views also compute it dynamically)
--
--    ValueSource:
--      'AUTO'   = derived from aircraft TechLog/counters automatically
--      'MANUAL' = manually entered (with ManualCorrectionNote)
--      'PRORATE'= prorated from aircraft counters over install period
--
--    LastSNResetEventId: FK to the reset event that started the
--    current interval — NULL means counter never been reset (since new).
-- ============================================================
IF OBJECT_ID('mro2.SNCounter', 'U') IS NULL
BEGIN
    CREATE TABLE mro2.SNCounter (
        SNCounterId             INT             NOT NULL IDENTITY(1,1),
        SerializedItemId        INT             NOT NULL,
        PNLimitId               INT             NOT NULL,

        -- Effective limit for this SN (PN default or SN override)
        -- Denormalized here for fast remaining calculation
        EffectiveHardLimit      DECIMAL(10,1)   NOT NULL,
        EffectiveAlertPct       TINYINT         NOT NULL,

        -- Accumulated since last reset
        AccumulatedSinceReset   DECIMAL(10,1)   NOT NULL
            CONSTRAINT DF_SNCounter_Accumulated DEFAULT (0),

        -- Lifetime total (never resets)
        LifetimeTotal           DECIMAL(10,1)   NOT NULL
            CONSTRAINT DF_SNCounter_Lifetime DEFAULT (0),

        -- Stored remaining (= EffectiveHardLimit - AccumulatedSinceReset)
        -- Recomputed on every upsert via SP, also recalculated in views
        Remaining               DECIMAL(10,1)   NOT NULL
            CONSTRAINT DF_SNCounter_Remaining DEFAULT (0),

        -- Status derived from Remaining vs AlertThreshold
        -- 'OK' | 'ALERT' | 'EXPIRED'
        -- Stored for fast dashboard queries, recomputed on every update
        LimitStatus             VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNCounter_Status DEFAULT ('OK'),

        -- Source of last update
        ValueSource             VARCHAR(10)     NOT NULL
            CONSTRAINT DF_SNCounter_Source DEFAULT ('MANUAL'),
        ManualCorrectionNote    NVARCHAR(200)   NULL,

        -- Link to reset event that started current interval
        LastSNResetEventId      INT             NULL,

        -- Timestamps
        LastUpdatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_SNCounter_LastUpdated DEFAULT (GETDATE()),
        LastUpdatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNCounter PRIMARY KEY (SNCounterId),

        CONSTRAINT UQ_SNCounter_SN_Limit UNIQUE (SerializedItemId, PNLimitId),

        CONSTRAINT FK_SNCounter_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNCounter_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId),

        CONSTRAINT FK_SNCounter_ResetEvent FOREIGN KEY (LastSNResetEventId)
            REFERENCES mro2.SNResetEvent (SNResetEventId),

        CONSTRAINT CK_SNCounter_Accumulated
            CHECK (AccumulatedSinceReset >= 0),

        CONSTRAINT CK_SNCounter_Lifetime
            CHECK (LifetimeTotal >= 0),

        CONSTRAINT CK_SNCounter_LimitStatus
            CHECK (LimitStatus IN ('OK', 'ALERT', 'EXPIRED')),

        CONSTRAINT CK_SNCounter_ValueSource
            CHECK (ValueSource IN ('AUTO', 'MANUAL', 'PRORATE'))
    );
END
GO

-- ============================================================
-- 6. mro2.SNCounterLog
--    Full audit trail of every change to SNCounter.
--    Never deleted — permanent record.
--    OldValue / NewValue: the AccumulatedSinceReset before/after.
-- ============================================================
IF OBJECT_ID('mro2.SNCounterLog', 'U') IS NULL
BEGIN
    CREATE TABLE mro2.SNCounterLog (
        SNCounterLogId      INT             NOT NULL IDENTITY(1,1),
        SerializedItemId    INT             NOT NULL,
        PNLimitId           INT             NOT NULL,
        LogDate             DATETIME        NOT NULL
            CONSTRAINT DF_SNCounterLog_LogDate DEFAULT (GETDATE()),
        OldAccumulated      DECIMAL(10,1)   NULL,   -- NULL on first entry (insert)
        NewAccumulated      DECIMAL(10,1)   NOT NULL,
        OldLifetime         DECIMAL(10,1)   NULL,
        NewLifetime         DECIMAL(10,1)   NOT NULL,
        ValueSource         VARCHAR(10)     NOT NULL,
        CorrectionNote      NVARCHAR(200)   NULL,
        -- Aircraft counters at time of update (context snapshot)
        AircraftFH          DECIMAL(10,1)   NULL,
        AircraftFC          INT             NULL,
        LoggedByUserId      NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNCounterLog PRIMARY KEY (SNCounterLogId),

        CONSTRAINT FK_SNCounterLog_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNCounterLog_PNLimit FOREIGN KEY (PNLimitId)
            REFERENCES mro2.PNLimit (PNLimitId),

        CONSTRAINT CK_SNCounterLog_Source
            CHECK (ValueSource IN ('AUTO', 'MANUAL', 'PRORATE'))
    );
END
GO

-- ============================================================
-- INDEXES
-- ============================================================

-- SNCounter: fast lookup by SN (dashboard, configuration page)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_SNCounter_SerializedItemId'
               AND object_id = OBJECT_ID('mro2.SNCounter'))
    CREATE INDEX IX_SNCounter_SerializedItemId
        ON mro2.SNCounter (SerializedItemId);
GO

-- SNCounter: fast expired/alert status dashboard query
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_SNCounter_LimitStatus'
               AND object_id = OBJECT_ID('mro2.SNCounter'))
    CREATE INDEX IX_SNCounter_LimitStatus
        ON mro2.SNCounter (LimitStatus) INCLUDE (SerializedItemId, PNLimitId, Remaining);
GO

-- SNCounterLog: history by SN ordered by date
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_SNCounterLog_SN_Date'
               AND object_id = OBJECT_ID('mro2.SNCounterLog'))
    CREATE INDEX IX_SNCounterLog_SN_Date
        ON mro2.SNCounterLog (SerializedItemId, LogDate DESC);
GO

-- SNResetEvent: history by SN
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_SNResetEvent_SerializedItemId'
               AND object_id = OBJECT_ID('mro2.SNResetEvent'))
    CREATE INDEX IX_SNResetEvent_SerializedItemId
        ON mro2.SNResetEvent (SerializedItemId, ResetDate DESC);
GO

-- ============================================================
-- VIEW: mro2.vw_SNLimitStatus
--    Master view used by dashboard, configuration page, reports.
--    Resolves the SN override vs PN default for every limit.
--    Recomputes Remaining and LimitStatus dynamically
--    (so reporting is always accurate even if stored values
--    lag slightly after a bulk aircraft counter update).
-- ============================================================
IF OBJECT_ID('mro2.vw_SNLimitStatus', 'V') IS NOT NULL
    DROP VIEW mro2.vw_SNLimitStatus;
GO

CREATE VIEW mro2.vw_SNLimitStatus
AS
SELECT
    -- Identity
    si.SerializedItemId,
    si.SerialNumber,
    pn.PartNumberId,
    pn.PN,
    pn.Nomenclature,
    pl.PNLimitId,
    lt.LimitTypeId,
    lt.Code                                         AS LimitTypeCode,
    lt.Name                                         AS LimitTypeName,
    lt.CounterUnit,
    lt.IsDecimal,

    -- Effective limit (SN override wins if exists and active)
    COALESCE(ov.HardLimit,        pl.HardLimit)     AS EffectiveHardLimit,
    COALESCE(ov.AlertThresholdPct,pl.AlertThresholdPct) AS EffectiveAlertPct,
    COALESCE(ov.ResetTrigger,     pl.ResetTrigger)  AS EffectiveResetTrigger,

    -- Source of effective limit
    CASE WHEN ov.SNLimitOverrideId IS NOT NULL
         THEN 'OVERRIDE' ELSE 'PN_DEFAULT' END       AS LimitSource,

    -- Counter values
    ISNULL(sc.AccumulatedSinceReset, 0)             AS AccumulatedSinceReset,
    ISNULL(sc.LifetimeTotal,         0)             AS LifetimeTotal,
    sc.ValueSource,
    sc.LastUpdatedDate,
    sc.LastUpdatedByUserId,

    -- Dynamically recomputed Remaining (authoritative for reports)
    COALESCE(ov.HardLimit, pl.HardLimit)
        - ISNULL(sc.AccumulatedSinceReset, 0)       AS RemainingCalc,

    -- Stored Remaining (fast dashboard queries)
    ISNULL(sc.Remaining, COALESCE(ov.HardLimit, pl.HardLimit)) AS RemainingStored,

    -- Percentage used (0–100+)
    CASE WHEN COALESCE(ov.HardLimit, pl.HardLimit) > 0
         THEN CAST(
                ISNULL(sc.AccumulatedSinceReset, 0)
                / COALESCE(ov.HardLimit, pl.HardLimit)
                * 100 AS DECIMAL(5,1))
         ELSE 0 END                                  AS PctUsed,

    -- Alert threshold value (e.g. 90% of 500 FH = 450 FH)
    COALESCE(ov.HardLimit, pl.HardLimit)
        * COALESCE(ov.AlertThresholdPct, pl.AlertThresholdPct)
        / 100.0                                      AS AlertAtValue,

    -- Dynamically recomputed status (authoritative for reports)
    CASE
        WHEN ISNULL(sc.AccumulatedSinceReset, 0)
             >= COALESCE(ov.HardLimit, pl.HardLimit)
        THEN 'EXPIRED'
        WHEN ISNULL(sc.AccumulatedSinceReset, 0)
             >= COALESCE(ov.HardLimit, pl.HardLimit)
                * COALESCE(ov.AlertThresholdPct, pl.AlertThresholdPct) / 100.0
        THEN 'ALERT'
        ELSE 'OK'
    END                                              AS LimitStatusCalc,

    -- Stored status (fast dashboard)
    ISNULL(sc.LimitStatus, 'OK')                    AS LimitStatusStored,

    -- PNLimit metadata
    pl.Notes                                        AS LimitNotes,
    ov.OverrideReason,

    -- Last reset info
    sc.LastSNResetEventId,
    re.ResetDate                                    AS LastResetDate,
    re.ResetType                                    AS LastResetType

FROM mro2.PNLimit pl
INNER JOIN mro2.LimitType        lt ON pl.LimitTypeId       = lt.LimitTypeId
INNER JOIN mro2.PartNumber       pn ON pl.PartNumberId      = pn.PartNumberId
INNER JOIN mro2.SerializedItem   si ON pn.PartNumberId      = si.PartNumberId
LEFT  JOIN mro2.SNLimitOverride  ov ON ov.PNLimitId         = pl.PNLimitId
                                   AND ov.SerializedItemId  = si.SerializedItemId
                                   AND ov.IsActive          = 1
LEFT  JOIN mro2.SNCounter        sc ON sc.PNLimitId         = pl.PNLimitId
                                   AND sc.SerializedItemId  = si.SerializedItemId
LEFT  JOIN mro2.SNResetEvent     re ON re.SNResetEventId    = sc.LastSNResetEventId
WHERE pl.IsActive = 1
  AND si.IsActive = 1
  AND pn.IsActive = 1;
GO

-- ============================================================
-- SP: mro2.usp_SNCounter_Upsert
--    Called by the UI and the auto-update job.
--    Inserts or updates SNCounter, recomputes Remaining and
--    LimitStatus, and writes the audit log row.
--
--    Parameters:
--      @SerializedItemId  : the SN being updated
--      @PNLimitId         : which limit counter to update
--      @NewAccumulated    : new AccumulatedSinceReset value
--      @ValueSource       : 'AUTO' | 'MANUAL' | 'PRORATE'
--      @CorrectionNote    : required when ValueSource = 'MANUAL'
--      @AircraftFH        : aircraft FH at time of update (context)
--      @AircraftFC        : aircraft FC at time of update (context)
--      @UserId            : Session("UserId")
-- ============================================================
IF OBJECT_ID('mro2.usp_SNCounter_Upsert', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNCounter_Upsert;
GO

CREATE PROCEDURE mro2.usp_SNCounter_Upsert
    @SerializedItemId   INT,
    @PNLimitId          INT,
    @NewAccumulated     DECIMAL(10,1),
    @ValueSource        VARCHAR(10),
    @CorrectionNote     NVARCHAR(200)   = NULL,
    @AircraftFH         DECIMAL(10,1)   = NULL,
    @AircraftFC         INT             = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- ── 1. Resolve effective limit (SN override or PN default) ──
    DECLARE @EffectiveHardLimit     DECIMAL(10,1);
    DECLARE @EffectiveAlertPct      TINYINT;

    SELECT
        @EffectiveHardLimit = COALESCE(ov.HardLimit,         pl.HardLimit),
        @EffectiveAlertPct  = COALESCE(ov.AlertThresholdPct, pl.AlertThresholdPct)
    FROM mro2.PNLimit pl
    LEFT JOIN mro2.SNLimitOverride ov
           ON ov.PNLimitId        = pl.PNLimitId
          AND ov.SerializedItemId = @SerializedItemId
          AND ov.IsActive         = 1
    WHERE pl.PNLimitId = @PNLimitId
      AND pl.IsActive  = 1;

    IF @EffectiveHardLimit IS NULL
    BEGIN
        RAISERROR('PNLimit not found or inactive: %d', 16, 1, @PNLimitId);
        RETURN;
    END

    -- ── 2. Compute stored Remaining and LimitStatus ──
    DECLARE @Remaining      DECIMAL(10,1) = @EffectiveHardLimit - @NewAccumulated;
    DECLARE @AlertAtValue   DECIMAL(10,1) = @EffectiveHardLimit * @EffectiveAlertPct / 100.0;

    DECLARE @LimitStatus VARCHAR(10) =
        CASE
            WHEN @NewAccumulated >= @EffectiveHardLimit THEN 'EXPIRED'
            WHEN @NewAccumulated >= @AlertAtValue       THEN 'ALERT'
            ELSE 'OK'
        END;

    -- ── 3. Capture old values for audit log ──
    DECLARE @OldAccumulated DECIMAL(10,1);
    DECLARE @OldLifetime    DECIMAL(10,1);
    DECLARE @OldSource      VARCHAR(10);
    DECLARE @LifetimeDelta  DECIMAL(10,1);

    SELECT
        @OldAccumulated = AccumulatedSinceReset,
        @OldLifetime    = LifetimeTotal,
        @OldSource      = ValueSource
    FROM mro2.SNCounter
    WHERE SerializedItemId = @SerializedItemId
      AND PNLimitId        = @PNLimitId;

    -- Delta applied to lifetime total
    -- If new > old: add the difference. If decreasing (correction): lifetime unchanged.
    SET @LifetimeDelta = CASE
        WHEN @OldAccumulated IS NULL THEN @NewAccumulated      -- first entry
        WHEN @NewAccumulated > @OldAccumulated
             THEN @NewAccumulated - @OldAccumulated            -- increment
        ELSE 0                                                 -- correction down, no lifetime change
    END;

    DECLARE @NewLifetime DECIMAL(10,1) = ISNULL(@OldLifetime, 0) + @LifetimeDelta;

    -- ── 4. Upsert SNCounter ──
    IF @OldAccumulated IS NULL
    BEGIN
        -- First time this SN/limit counter is recorded
        INSERT INTO mro2.SNCounter (
            SerializedItemId, PNLimitId,
            EffectiveHardLimit, EffectiveAlertPct,
            AccumulatedSinceReset, LifetimeTotal, Remaining,
            LimitStatus, ValueSource, ManualCorrectionNote,
            LastUpdatedDate, LastUpdatedByUserId
        )
        VALUES (
            @SerializedItemId, @PNLimitId,
            @EffectiveHardLimit, @EffectiveAlertPct,
            @NewAccumulated, @NewLifetime, @Remaining,
            @LimitStatus, @ValueSource,
            CASE WHEN @ValueSource = 'MANUAL' THEN @CorrectionNote ELSE NULL END,
            GETDATE(), @UserId
        );
    END
    ELSE
    BEGIN
        UPDATE mro2.SNCounter SET
            EffectiveHardLimit      = @EffectiveHardLimit,
            EffectiveAlertPct       = @EffectiveAlertPct,
            AccumulatedSinceReset   = @NewAccumulated,
            LifetimeTotal           = @NewLifetime,
            Remaining               = @Remaining,
            LimitStatus             = @LimitStatus,
            ValueSource             = @ValueSource,
            ManualCorrectionNote    = CASE WHEN @ValueSource = 'MANUAL'
                                          THEN @CorrectionNote ELSE NULL END,
            LastUpdatedDate         = GETDATE(),
            LastUpdatedByUserId     = @UserId
        WHERE SerializedItemId = @SerializedItemId
          AND PNLimitId        = @PNLimitId;
    END

    -- ── 5. Write audit log ──
    INSERT INTO mro2.SNCounterLog (
        SerializedItemId, PNLimitId,
        LogDate,
        OldAccumulated, NewAccumulated,
        OldLifetime,    NewLifetime,
        ValueSource, CorrectionNote,
        AircraftFH, AircraftFC,
        LoggedByUserId
    )
    VALUES (
        @SerializedItemId, @PNLimitId,
        GETDATE(),
        @OldAccumulated, @NewAccumulated,
        @OldLifetime,    @NewLifetime,
        @ValueSource, @CorrectionNote,
        @AircraftFH, @AircraftFC,
        @UserId
    );
END
GO

-- ============================================================
-- SP: mro2.usp_SNCounter_Reset
--    Resets AccumulatedSinceReset to 0 for a given SN+limit.
--    Creates a SNResetEvent row first, then calls Upsert with 0.
--    Called from:
--      - RecordEvent (install) when ResetTrigger = 'INSTALL'
--      - Overhaul entry form when ResetTrigger = 'OVERHAUL'
-- ============================================================
IF OBJECT_ID('mro2.usp_SNCounter_Reset', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNCounter_Reset;
GO

CREATE PROCEDURE mro2.usp_SNCounter_Reset
    @SerializedItemId   INT,
    @PNLimitId          INT,
    @ResetType          VARCHAR(10),    -- 'INSTALL' | 'OVERHAUL'
    @ResetDate          DATE,
    @FH_AtReset         DECIMAL(10,1)   = NULL,
    @FC_AtReset         INT             = NULL,
    @InstallEventId     INT             = NULL,
    @ShopOrderRef       NVARCHAR(50)    = NULL,
    @Notes              NVARCHAR(300)   = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- ── 1. Insert the reset event record ──
    DECLARE @ResetEventId INT;

    INSERT INTO mro2.SNResetEvent (
        SerializedItemId, PNLimitId,
        ResetType, ResetDate,
        FH_AtReset, FC_AtReset,
        InstallEventId, ShopOrderRef,
        Notes, CreatedDate, CreatedByUserId
    )
    VALUES (
        @SerializedItemId, @PNLimitId,
        @ResetType, @ResetDate,
        @FH_AtReset, @FC_AtReset,
        @InstallEventId, @ShopOrderRef,
        @Notes, GETDATE(), @UserId
    );

    SET @ResetEventId = SCOPE_IDENTITY();

    -- ── 2. Zero out the counter via Upsert ──
    EXEC mro2.usp_SNCounter_Upsert
        @SerializedItemId   = @SerializedItemId,
        @PNLimitId          = @PNLimitId,
        @NewAccumulated     = 0,
        @ValueSource        = 'MANUAL',
        @CorrectionNote     = 'Counter reset',
        @AircraftFH         = @FH_AtReset,
        @AircraftFC         = @FC_AtReset,
        @UserId             = @UserId;

    -- ── 3. Link SNCounter to this reset event ──
    UPDATE mro2.SNCounter
    SET LastSNResetEventId = @ResetEventId
    WHERE SerializedItemId = @SerializedItemId
      AND PNLimitId        = @PNLimitId;

    SELECT @ResetEventId AS NewSNResetEventId;
END
GO

-- ============================================================
-- SP: mro2.usp_PNLimit_List
--    Returns all limits for a given PN with row count.
--    Used by PNLimitList.aspx.
-- ============================================================
IF OBJECT_ID('mro2.usp_PNLimit_List', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_PNLimit_List;
GO

CREATE PROCEDURE mro2.usp_PNLimit_List
    @PartNumberId   INT,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pl.PNLimitId,
        pl.PartNumberId,
        pn.PN,
        pn.Nomenclature,
        pl.LimitTypeId,
        lt.Code         AS LimitTypeCode,
        lt.Name         AS LimitTypeName,
        lt.CounterUnit,
        lt.IsDecimal,
        pl.HardLimit,
        pl.AlertThresholdPct,
        pl.ResetTrigger,
        pl.Notes,
        pl.IsActive,
        pl.CreatedDate,
        pl.CreatedByUserId
    FROM mro2.PNLimit pl
    INNER JOIN mro2.LimitType  lt ON pl.LimitTypeId  = lt.LimitTypeId
    INNER JOIN mro2.PartNumber pn ON pl.PartNumberId = pn.PartNumberId
    WHERE pl.PartNumberId = @PartNumberId
      AND (@IncludeInactive = 1 OR pl.IsActive = 1)
    ORDER BY lt.LimitTypeId;
END
GO

-- ============================================================
-- SP: mro2.usp_PNLimit_Save
--    Insert or update a PNLimit row.
--    @PNLimitId = NULL → INSERT, else UPDATE.
--    Returns the PNLimitId of the saved row.
-- ============================================================
IF OBJECT_ID('mro2.usp_PNLimit_Save', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_PNLimit_Save;
GO

CREATE PROCEDURE mro2.usp_PNLimit_Save
    @PNLimitId          INT             = NULL,
    @PartNumberId       INT,
    @LimitTypeId        TINYINT,
    @HardLimit          DECIMAL(10,1),
    @AlertThresholdPct  TINYINT,
    @ResetTrigger       VARCHAR(10),
    @Notes              NVARCHAR(300)   = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @PNLimitId IS NULL
    BEGIN
        INSERT INTO mro2.PNLimit (
            PartNumberId, LimitTypeId,
            HardLimit, AlertThresholdPct,
            ResetTrigger, Notes,
            IsActive, CreatedDate, CreatedByUserId
        )
        VALUES (
            @PartNumberId, @LimitTypeId,
            @HardLimit, @AlertThresholdPct,
            @ResetTrigger, @Notes,
            1, GETDATE(), @UserId
        );
        SELECT SCOPE_IDENTITY() AS PNLimitId;
    END
    ELSE
    BEGIN
        UPDATE mro2.PNLimit SET
            LimitTypeId         = @LimitTypeId,
            HardLimit           = @HardLimit,
            AlertThresholdPct   = @AlertThresholdPct,
            ResetTrigger        = @ResetTrigger,
            Notes               = @Notes
        WHERE PNLimitId = @PNLimitId;
        SELECT @PNLimitId AS PNLimitId;
    END
END
GO

-- ============================================================
-- SP: mro2.usp_PNLimit_SetActive
--    Soft-delete / reactivate a PNLimit row.
-- ============================================================
IF OBJECT_ID('mro2.usp_PNLimit_SetActive', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_PNLimit_SetActive;
GO

CREATE PROCEDURE mro2.usp_PNLimit_SetActive
    @PNLimitId  INT,
    @IsActive   BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.PNLimit SET IsActive = @IsActive WHERE PNLimitId = @PNLimitId;
END
GO

-- ============================================================
-- SP: mro2.usp_SNLimitStatus_GetBySN
--    Returns full limit status for one SN — used by the
--    SN detail page and configuration page.
--    Pulls from the view (dynamic Remaining + Status).
-- ============================================================
IF OBJECT_ID('mro2.usp_SNLimitStatus_GetBySN', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNLimitStatus_GetBySN;
GO

CREATE PROCEDURE mro2.usp_SNLimitStatus_GetBySN
    @SerializedItemId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_SNLimitStatus
    WHERE SerializedItemId = @SerializedItemId
    ORDER BY LimitTypeId;
END
GO

-- ============================================================
-- SP: mro2.usp_SNLimitStatus_GetExpiredAndAlert
--    Dashboard query — all SNs that are EXPIRED or in ALERT.
--    Used by the MRO2 home dashboard.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNLimitStatus_GetExpiredAndAlert', 'P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNLimitStatus_GetExpiredAndAlert;
GO

CREATE PROCEDURE mro2.usp_SNLimitStatus_GetExpiredAndAlert
    @BaseId INT = NULL  -- NULL = all bases, set = filter by base
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        v.SerializedItemId,
        v.SerialNumber,
        v.PN,
        v.Nomenclature,
        v.LimitTypeCode,
        v.LimitTypeName,
        v.CounterUnit,
        v.EffectiveHardLimit,
        v.AccumulatedSinceReset,
        v.RemainingCalc,
        v.PctUsed,
        v.LimitStatusCalc,
        v.LimitSource,
        v.LastResetDate,
        v.LastResetType
    FROM mro2.vw_SNLimitStatus v
    WHERE v.LimitStatusCalc IN ('EXPIRED', 'ALERT')
    ORDER BY
        CASE v.LimitStatusCalc WHEN 'EXPIRED' THEN 0 ELSE 1 END,
        v.PctUsed DESC;
END
GO

-- ============================================================
-- VERIFICATION QUERIES
-- Run these after executing the script to confirm all objects
-- were created successfully.
-- ============================================================
/*
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'mro2'
  AND TABLE_NAME IN ('LimitType','PNLimit','SNLimitOverride',
                     'SNResetEvent','SNCounter','SNCounterLog')
ORDER BY TABLE_NAME;

SELECT ROUTINE_NAME
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'mro2'
  AND ROUTINE_NAME IN (
      'usp_SNCounter_Upsert','usp_SNCounter_Reset',
      'usp_PNLimit_List','usp_PNLimit_Save','usp_PNLimit_SetActive',
      'usp_SNLimitStatus_GetBySN','usp_SNLimitStatus_GetExpiredAndAlert')
ORDER BY ROUTINE_NAME;

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA = 'mro2'
  AND TABLE_NAME = 'vw_SNLimitStatus';

-- Verify seed data
SELECT * FROM mro2.LimitType ORDER BY LimitTypeId;
*/
