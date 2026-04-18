-- ============================================================
-- MRO2 — STEP 09 of 10
-- TechLog Counter Feed
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- ============================================================
-- TABLES CREATED:
--   mro2.AcCounter          live aircraft counter totals (odometer)
--   mro2.AcCounterLog       full audit trail of every update
-- VIEWS:
--   mro2.vw_AcCounterCurrent   current totals per aircraft
-- STORED PROCEDURES: 4
-- PREREQUISITE: Steps 01-08
-- ============================================================
--
-- DESIGN:
--
--   AcCounter = the live odometer per aircraft per CounterDef.
--   One row per (AcID, CounterDefId). Updated on every flight.
--   This is the authoritative current value used by:
--     - AcCounterSnapshot (captured at each RecordEvent)
--     - usp_TechLog_Feed  (propagates to all installed SNs)
--
--   TWO UPDATE SOURCES:
--     AUTO   : fed from Ops module sortie/TechLog automatically
--              after each flight leg is closed
--     MANUAL : technician enters aircraft totals directly
--              (correction, initial data entry, offline period)
--
--   PROPAGATION:
--     usp_TechLog_Feed is the single entry point.
--     After updating AcCounter, it:
--       1. Writes AcCounterLog (audit trail)
--       2. Finds all SNs currently installed on this aircraft
--          (from vw_CurrentInstallation)
--       3. For each installed SN, computes the SN-level counter
--          value by prorating: SN accumulated += aircraft delta
--       4. Calls usp_SNTaskCounterState_Update for each SN
--          so status/remaining/alerts are recomputed immediately
--
--   PRORATION:
--     SN accumulated = SN accumulated at last install +
--                      (aircraft current - aircraft at install)
--     This is exact for AIRCRAFT-basis counters.
--     COMPONENT-basis counters (APU, engine) are NOT propagated
--     from aircraft — they have their own update path.
--
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- TABLE 1: mro2.AcCounter
--    Live aircraft counter totals — the odometer.
--    One row per (AcID, CounterDefId).
--    CurrentValue in CounterDef.UnitStorage units.
--    (MINUTES for FH, COUNT for FC/landings/days)
-- ============================================================
IF OBJECT_ID('mro2.AcCounter','U') IS NULL
BEGIN
    CREATE TABLE mro2.AcCounter (
        AcCounterId         INT             NOT NULL IDENTITY(1,1),
        AcID                INT             NOT NULL,
        CounterDefId        INT             NOT NULL,

        -- Current running total (authoritative live value)
        CurrentValue        INT             NOT NULL
            CONSTRAINT DF_AcCounter_CurrentValue    DEFAULT (0),

        -- Previous value before last update (for delta computation)
        PreviousValue       INT             NOT NULL
            CONSTRAINT DF_AcCounter_PreviousValue   DEFAULT (0),

        -- Source of last update
        LastUpdateSource    VARCHAR(10)     NOT NULL
            CONSTRAINT DF_AcCounter_Source          DEFAULT ('MANUAL'),
            -- 'AUTO'   : from Ops sortie/TechLog feed
            -- 'MANUAL' : technician direct entry

        -- Reference to sortie if source=AUTO
        SortieRef           NVARCHAR(50)    NULL,

        LastUpdatedDate     DATETIME        NOT NULL
            CONSTRAINT DF_AcCounter_Updated         DEFAULT (GETDATE()),
        LastUpdatedByUserId NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_AcCounter PRIMARY KEY (AcCounterId),

        -- One row per aircraft per counter
        CONSTRAINT UQ_AcCounter_Ac_Counter
            UNIQUE (AcID, CounterDefId),

        CONSTRAINT FK_AcCounter_Aircraft
            FOREIGN KEY (AcID)
            REFERENCES dbo.tblAircraft (AcID),

        CONSTRAINT FK_AcCounter_CounterDef
            FOREIGN KEY (CounterDefId)
            REFERENCES mro2.CounterDef (CounterDefId),

        CONSTRAINT CK_AcCounter_Source
            CHECK (LastUpdateSource IN ('AUTO','MANUAL')),

        CONSTRAINT CK_AcCounter_Value
            CHECK (CurrentValue >= 0)
    );

    -- Fast lookup by aircraft (used on every propagation call)
    CREATE INDEX IX_AcCounter_AcID
        ON mro2.AcCounter (AcID)
        INCLUDE (CounterDefId, CurrentValue, LastUpdateSource);

    PRINT 'mro2.AcCounter created.';
END
ELSE
    PRINT 'mro2.AcCounter already exists — skipped.';
GO

-- ============================================================
-- TABLE 2: mro2.AcCounterLog
--    Immutable audit trail of every AcCounter update.
--    Never deleted. One row per update event per counter.
-- ============================================================
IF OBJECT_ID('mro2.AcCounterLog','U') IS NULL
BEGIN
    CREATE TABLE mro2.AcCounterLog (
        AcCounterLogId      INT             NOT NULL IDENTITY(1,1),
        AcID                INT             NOT NULL,
        CounterDefId        INT             NOT NULL,
        LogDate             DATETIME        NOT NULL
            CONSTRAINT DF_AcCounterLog_Date         DEFAULT (GETDATE()),
        OldValue            INT             NOT NULL,
        NewValue            INT             NOT NULL,
        Delta               INT             NOT NULL,   -- NewValue - OldValue
        UpdateSource        VARCHAR(10)     NOT NULL,
        SortieRef           NVARCHAR(50)    NULL,
        -- How many installed SNs were propagated to
        SNsPropagated       INT             NOT NULL
            CONSTRAINT DF_AcCounterLog_SNs          DEFAULT (0),
        LoggedByUserId      NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_AcCounterLog PRIMARY KEY (AcCounterLogId),

        CONSTRAINT FK_AcCounterLog_Aircraft
            FOREIGN KEY (AcID) REFERENCES dbo.tblAircraft (AcID),

        CONSTRAINT FK_AcCounterLog_CounterDef
            FOREIGN KEY (CounterDefId)
            REFERENCES mro2.CounterDef (CounterDefId),

        CONSTRAINT CK_AcCounterLog_Source
            CHECK (UpdateSource IN ('AUTO','MANUAL'))
    );

    -- History by aircraft ordered newest first
    CREATE INDEX IX_AcCounterLog_AcID_Date
        ON mro2.AcCounterLog (AcID, LogDate DESC)
        INCLUDE (CounterDefId, OldValue, NewValue, Delta, UpdateSource);

    PRINT 'mro2.AcCounterLog created.';
END
ELSE
    PRINT 'mro2.AcCounterLog already exists — skipped.';
GO

-- ============================================================
-- VIEW: mro2.vw_AcCounterCurrent
--    Current counter totals per aircraft with display values.
--    Converts MINUTES → decimal hours for display.
--    Shows all active counters even if value is 0.
-- ============================================================
IF OBJECT_ID('mro2.vw_AcCounterCurrent','V') IS NOT NULL
    DROP VIEW mro2.vw_AcCounterCurrent;
GO
CREATE VIEW mro2.vw_AcCounterCurrent
AS
SELECT
    ac.AcCounterId,
    ac.AcID,
    ta.TailNo,
    acg.AcMainGroup                     AS AcMainGroupName,
    act.AcType                          AS AcTypeName,

    -- Counter definition
    cd.CounterDefId,
    cd.Code                             AS CounterDefCode,
    cd.Name                             AS CounterDefName,
    cd.AppliesToAssetKindCode,
    cd.UnitStorage,
    ct.DisplayUnit,

    -- Raw value (storage units)
    ac.CurrentValue,
    ac.PreviousValue,
    ac.CurrentValue - ac.PreviousValue  AS LastDelta,

    -- Display value
    -- MINUTES → decimal hours (e.g. 256800 min = 4280.00 hrs)
    CASE WHEN cd.UnitStorage = 'MINUTES'
         THEN CAST(ac.CurrentValue / 60.0 AS DECIMAL(10,1))
         ELSE CAST(ac.CurrentValue AS DECIMAL(10,0))
    END                                 AS DisplayValue,

    ct.DisplayUnit                      AS DisplayUnitLabel,

    -- Update metadata
    ac.LastUpdateSource,
    ac.SortieRef,
    ac.LastUpdatedDate,
    ac.LastUpdatedByUserId

FROM mro2.AcCounter ac
INNER JOIN dbo.tblAircraft    ta  ON ta.AcID          = ac.AcID
INNER JOIN dbo.tblAcMainGroup acg ON acg.AcMainGroupID= ta.AcMainGroupID
INNER JOIN dbo.tblAcType      act ON act.AcTypeId     = ta.AcTypeID
INNER JOIN mro2.CounterDef    cd  ON cd.CounterDefId  = ac.CounterDefId
INNER JOIN mro2.CounterType   ct  ON ct.CounterTypeId = cd.CounterTypeId
WHERE cd.AppliesToAssetKindCode = 'AIRCRAFT'
  AND cd.IsActive               = 1;
GO
PRINT 'mro2.vw_AcCounterCurrent created.';
GO

-- ============================================================
-- SP: mro2.usp_TechLog_Feed
--    SINGLE ENTRY POINT for all aircraft counter updates.
--    Called by:
--      - Ops module after each sortie leg closes (AUTO)
--      - Technician direct entry form (MANUAL)
--
--    Parameters:
--      @AcID          : aircraft being updated
--      @CounterDefId  : which counter (AF_FLIGHT_MIN, AF_CYCLES...)
--      @NewValue      : new absolute total (not a delta)
--      @UpdateSource  : 'AUTO' | 'MANUAL'
--      @SortieRef     : sortie/flight reference (AUTO source)
--      @UserId        : Session("UserId")
--
--    Process:
--      1. Validate new value >= current (counters only go up)
--         Exception: MANUAL can correct downward with reason
--      2. Compute delta (NewValue - CurrentValue)
--      3. Update AcCounter
--      4. Write AcCounterLog
--      5. Propagate delta to all installed SNs (AIRCRAFT counters)
--      6. Call usp_SNTaskCounterState_Update per affected SN
--      7. Return propagation summary
-- ============================================================
IF OBJECT_ID('mro2.usp_TechLog_Feed','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_TechLog_Feed;
GO
CREATE PROCEDURE mro2.usp_TechLog_Feed
    @AcID           INT,
    @CounterDefId   INT,
    @NewValue       INT,            -- absolute total in UnitStorage units
    @UpdateSource   VARCHAR(10),    -- 'AUTO' | 'MANUAL'
    @SortieRef      NVARCHAR(50)    = NULL,
    @AllowDecrease  BIT             = 0,    -- 1 = allow manual correction downward
    @UserId         NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- ── 1. Get current value ──────────────────────────────────
    DECLARE @CurrentValue   INT = 0;
    DECLARE @AssetKind      VARCHAR(20);
    DECLARE @UnitStorage    VARCHAR(10);

    SELECT
        @CurrentValue = ISNULL(
            (SELECT CurrentValue FROM mro2.AcCounter
             WHERE AcID=@AcID AND CounterDefId=@CounterDefId), 0),
        @AssetKind    = cd.AppliesToAssetKindCode,
        @UnitStorage  = cd.UnitStorage
    FROM mro2.CounterDef cd
    WHERE cd.CounterDefId = @CounterDefId;

    -- ── 2. Validate counter type is AIRCRAFT ──────────────────
    IF @AssetKind <> 'AIRCRAFT'
    BEGIN
        RAISERROR(
            'CounterDefId %d is a COMPONENT counter. Use usp_ComponentCounter_Feed for component counters.',
            16, 1, @CounterDefId);
        RETURN;
    END

    -- ── 3. Validate value does not decrease (unless allowed) ──
    IF @NewValue < @CurrentValue AND @AllowDecrease = 0
    BEGIN
        RAISERROR(
            'New counter value (%d) is less than current value (%d). Set @AllowDecrease=1 for manual corrections.',
            16, 1, @NewValue, @CurrentValue);
        RETURN;
    END

    DECLARE @Delta INT = @NewValue - @CurrentValue;

    -- If no change, nothing to do
    IF @Delta = 0
    BEGIN
        PRINT 'No change in counter value — no update performed.';
        RETURN;
    END

    BEGIN TRANSACTION;

    -- ── 4. Upsert AcCounter ───────────────────────────────────
    IF EXISTS (SELECT 1 FROM mro2.AcCounter
               WHERE AcID=@AcID AND CounterDefId=@CounterDefId)
    BEGIN
        UPDATE mro2.AcCounter SET
            PreviousValue       = CurrentValue,
            CurrentValue        = @NewValue,
            LastUpdateSource    = @UpdateSource,
            SortieRef           = @SortieRef,
            LastUpdatedDate     = GETDATE(),
            LastUpdatedByUserId = @UserId
        WHERE AcID=@AcID AND CounterDefId=@CounterDefId;
    END
    ELSE
    BEGIN
        INSERT INTO mro2.AcCounter (
            AcID, CounterDefId, CurrentValue, PreviousValue,
            LastUpdateSource, SortieRef,
            LastUpdatedDate, LastUpdatedByUserId)
        VALUES (
            @AcID, @CounterDefId, @NewValue, 0,
            @UpdateSource, @SortieRef,
            GETDATE(), @UserId);
    END

    -- ── 5. Write audit log ────────────────────────────────────
    INSERT INTO mro2.AcCounterLog (
        AcID, CounterDefId, OldValue, NewValue, Delta,
        UpdateSource, SortieRef, SNsPropagated, LoggedByUserId)
    VALUES (
        @AcID, @CounterDefId, @CurrentValue, @NewValue, @Delta,
        @UpdateSource, @SortieRef, 0, @UserId);

    DECLARE @LogId INT = SCOPE_IDENTITY();

    -- ── 6. Propagate to installed SNs ─────────────────────────
    -- Only propagate AIRCRAFT-basis counters.
    -- Find all SNs currently installed on this aircraft.
    -- For each SN, find TaskCounters that use this CounterDef.
    -- Compute new SN accumulated = install snapshot delta + current.

    DECLARE @SNsPropagated INT = 0;

    -- Temp table: SNs to propagate + their install FH snapshot
    CREATE TABLE #SNsToUpdate (
        SerializedItemId    INT NOT NULL,
        TaskCounterId       INT NOT NULL,
        InstallCounterValue INT NOT NULL,   -- aircraft counter at install
        CurrentSNAccum      INT NOT NULL,   -- SN accumulated since last reset
        CurrentSNLifetime   INT NOT NULL    -- SN lifetime total
    );

    INSERT INTO #SNsToUpdate (
        SerializedItemId, TaskCounterId,
        InstallCounterValue,
        CurrentSNAccum, CurrentSNLifetime)
    SELECT
        ci.SerializedItemId,
        tc.TaskCounterId,
        -- Aircraft counter value at the time this SN was installed
        ISNULL(cs.CounterValue, 0)          AS InstallCounterValue,
        -- Current SN state
        ISNULL(st.AccumulatedSinceLast, 0)  AS CurrentSNAccum,
        ISNULL(st.LifetimeTotal, 0)         AS CurrentSNLifetime
    FROM mro2.vw_CurrentInstallation ci
    -- Join to TaskCounter for this SN's PN that uses this CounterDef
    INNER JOIN mro2.PNLimit          pl  ON pl.PartNumberId    = ci.PartNumberId
    INNER JOIN mro2.TaskCounter      tc  ON tc.PNLimitId       = pl.PNLimitId
                                        AND tc.CounterDefId    = @CounterDefId
                                        AND tc.IsActive        = 1
    -- SN's current counter state
    LEFT  JOIN mro2.SNTaskCounterState st ON st.SerializedItemId = ci.SerializedItemId
                                         AND st.TaskCounterId    = tc.TaskCounterId
    -- Aircraft counter value at install (from snapshot)
    LEFT  JOIN mro2.AcCounterSnapshot  cs ON cs.RecordEventId   = ci.InstallEventId
                                         AND cs.CounterDefId    = @CounterDefId
    WHERE ci.AcID = @AcID;

    -- Update each SN
    DECLARE @CurSN      INT;
    DECLARE @CurTC      INT;
    DECLARE @InstallVal INT;
    DECLARE @NewAccum   INT;
    DECLARE @NewLifetime INT;

    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT SerializedItemId, TaskCounterId,
               InstallCounterValue, CurrentSNAccum, CurrentSNLifetime
        FROM #SNsToUpdate;

    OPEN cur;
    FETCH NEXT FROM cur INTO @CurSN, @CurTC,
                             @InstallVal, @NewAccum, @NewLifetime;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- SN accumulated = aircraft current - aircraft at install
        SET @NewAccum    = @NewValue - @InstallVal;
        -- Lifetime increases by delta
        SET @NewLifetime = @NewLifetime + @Delta;

        EXEC mro2.usp_SNTaskCounterState_Update
            @SerializedItemId = @CurSN,
            @TaskCounterId    = @CurTC,
            @NewLifetimeTotal = @NewLifetime,
            @ValueSource      = @UpdateSource,
            @UserId           = @UserId;

        SET @SNsPropagated = @SNsPropagated + 1;

        FETCH NEXT FROM cur INTO @CurSN, @CurTC,
                                 @InstallVal, @NewAccum, @NewLifetime;
    END

    CLOSE cur;
    DEALLOCATE cur;
    DROP TABLE #SNsToUpdate;

    -- ── 7. Update log with propagation count ──────────────────
    UPDATE mro2.AcCounterLog
    SET SNsPropagated = @SNsPropagated
    WHERE AcCounterLogId = @LogId;

    COMMIT TRANSACTION;

    -- ── 8. Return summary ─────────────────────────────────────
    SELECT
        @AcID               AS AcID,
        @CounterDefId       AS CounterDefId,
        @CurrentValue       AS OldValue,
        @NewValue           AS NewValue,
        @Delta              AS Delta,
        @UpdateSource       AS UpdateSource,
        @SNsPropagated      AS SNsPropagated;
END
GO

-- ============================================================
-- SP: mro2.usp_ComponentCounter_Feed
--    Updates COMPONENT-level counters (APU hrs, engine hrs,
--    engine starts) directly on the SN — not via aircraft.
--    Called by: APU/engine maintenance entry form.
--    Calls usp_SNTaskCounterState_Update after updating.
-- ============================================================
IF OBJECT_ID('mro2.usp_ComponentCounter_Feed','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_ComponentCounter_Feed;
GO
CREATE PROCEDURE mro2.usp_ComponentCounter_Feed
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @NewLifetimeTotal   INT,        -- absolute total in UnitStorage units
    @UpdateSource       VARCHAR(10),
    @CorrectionNote     NVARCHAR(200) = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate it's a COMPONENT counter
    DECLARE @AssetKind VARCHAR(20);
    SELECT @AssetKind = cd.AppliesToAssetKindCode
    FROM mro2.TaskCounter tc
    INNER JOIN mro2.CounterDef cd ON cd.CounterDefId = tc.CounterDefId
    WHERE tc.TaskCounterId = @TaskCounterId;

    IF @AssetKind <> 'COMPONENT'
    BEGIN
        RAISERROR(
            'TaskCounterId %d is not a COMPONENT counter. Use usp_TechLog_Feed for aircraft counters.',
            16, 1, @TaskCounterId);
        RETURN;
    END

    -- Update the SN counter state
    EXEC mro2.usp_SNTaskCounterState_Update
        @SerializedItemId = @SerializedItemId,
        @TaskCounterId    = @TaskCounterId,
        @NewLifetimeTotal = @NewLifetimeTotal,
        @ValueSource      = @UpdateSource,
        @UserId           = @UserId;

    SELECT
        @SerializedItemId   AS SerializedItemId,
        @TaskCounterId      AS TaskCounterId,
        @NewLifetimeTotal   AS NewLifetimeTotal,
        @UpdateSource       AS UpdateSource;
END
GO

-- ============================================================
-- SP: mro2.usp_AcCounter_GetCurrent
--    Returns current counter totals for one aircraft.
--    Used by RecordEvent UI to populate the snapshot fields.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcCounter_GetCurrent','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcCounter_GetCurrent;
GO
CREATE PROCEDURE mro2.usp_AcCounter_GetCurrent
    @AcID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_AcCounterCurrent
    WHERE AcID = @AcID
    ORDER BY CounterDefId;
END
GO

-- ============================================================
-- SP: mro2.usp_AcCounter_GetLog
--    Audit history for one aircraft counter.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcCounter_GetLog','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcCounter_GetLog;
GO
CREATE PROCEDURE mro2.usp_AcCounter_GetLog
    @AcID           INT,
    @CounterDefId   INT     = NULL,     -- NULL = all counters
    @TopN           INT     = 50
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        l.AcCounterLogId,
        l.AcID,
        ta.TailNo,
        l.CounterDefId,
        cd.Code     AS CounterDefCode,
        ct.DisplayUnit,
        l.OldValue,
        l.NewValue,
        l.Delta,
        -- Display values (MINUTES → hrs)
        CASE WHEN cd.UnitStorage='MINUTES'
             THEN CAST(l.OldValue/60.0 AS DECIMAL(10,1))
             ELSE CAST(l.OldValue AS DECIMAL(10,0)) END AS OldDisplayValue,
        CASE WHEN cd.UnitStorage='MINUTES'
             THEN CAST(l.NewValue/60.0 AS DECIMAL(10,1))
             ELSE CAST(l.NewValue AS DECIMAL(10,0)) END AS NewDisplayValue,
        CASE WHEN cd.UnitStorage='MINUTES'
             THEN CAST(l.Delta/60.0 AS DECIMAL(10,1))
             ELSE CAST(l.Delta AS DECIMAL(10,0)) END    AS DeltaDisplay,
        l.UpdateSource,
        l.SortieRef,
        l.SNsPropagated,
        l.LogDate,
        l.LoggedByUserId
    FROM mro2.AcCounterLog l
    INNER JOIN dbo.tblAircraft  ta  ON ta.AcID          = l.AcID
    INNER JOIN mro2.CounterDef  cd  ON cd.CounterDefId  = l.CounterDefId
    INNER JOIN mro2.CounterType ct  ON ct.CounterTypeId = cd.CounterTypeId
    WHERE l.AcID = @AcID
      AND (@CounterDefId IS NULL OR l.CounterDefId = @CounterDefId)
    ORDER BY l.LogDate DESC, l.AcCounterLogId DESC;
END
GO

-- ============================================================
-- STEP 09 VERIFICATION
-- ============================================================
/*
-- Tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='mro2'
  AND TABLE_NAME IN ('AcCounter','AcCounterLog')
ORDER BY TABLE_NAME;

-- View
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA='mro2' AND TABLE_NAME='vw_AcCounterCurrent';

-- SPs (expect 4)
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA='mro2'
  AND ROUTINE_NAME IN (
    'usp_TechLog_Feed',
    'usp_ComponentCounter_Feed',
    'usp_AcCounter_GetCurrent',
    'usp_AcCounter_GetLog')
ORDER BY ROUTINE_NAME;

-- ── SAMPLE USAGE ──────────────────────────────────────────

-- Manual entry: set aircraft TailNo-201 FH to 4280.5 hrs
-- (4280.5 hrs x 60 = 256830 minutes)
-- EXEC mro2.usp_TechLog_Feed
--     @AcID         = 5,
--     @CounterDefId = 2,        -- AF_FLIGHT_MIN
--     @NewValue     = 256830,
--     @UpdateSource = 'MANUAL',
--     @UserId       = 'admin';

-- Auto feed from sortie (Ops module calls this after leg close)
-- EXEC mro2.usp_TechLog_Feed
--     @AcID         = 5,
--     @CounterDefId = 2,        -- AF_FLIGHT_MIN
--     @NewValue     = 257730,   -- +900 min = +15 FH from last sortie
--     @UpdateSource = 'AUTO',
--     @SortieRef    = 'SRT-2024-1234',
--     @UserId       = 'system';

-- Component counter (APU hours on SN-APU-001)
-- EXEC mro2.usp_ComponentCounter_Feed
--     @SerializedItemId = 3,
--     @TaskCounterId    = 8,    -- APU_HOURS_MIN counter
--     @NewLifetimeTotal = 18000, -- 300 APU hrs
--     @UpdateSource     = 'MANUAL',
--     @UserId           = 'admin';

-- Check current aircraft counters
-- EXEC mro2.usp_AcCounter_GetCurrent @AcID = 5;

-- View update log
-- EXEC mro2.usp_AcCounter_GetLog @AcID=5, @TopN=10;
*/

PRINT '── Step 09 complete ─────────────────────────────────────';
