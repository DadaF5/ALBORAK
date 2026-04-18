-- ============================================================
-- MRO2 — STEP 08 of 10
-- RecordEvent — Install / Remove / Transfer / Inspect
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- ============================================================
-- TABLES CREATED:
--   mro2.RecordEvent         one row per SN event
--   mro2.AcCounterSnapshot   aircraft counter values at event time
-- VIEWS:
--   mro2.vw_CurrentInstallation  current SN at each position
--   mro2.vw_SNHistory            full install/remove history per SN
-- STORED PROCEDURES: 6
-- PREREQUISITE: Steps 01-07
-- ============================================================
--
-- DESIGN:
--
--   ONE ROW PER SN PER EVENT.
--   EventType: INSTALL | REMOVE | TRANSFER | INSPECT
--
--   INSTALL:
--     SerializedItemId installed at AcPositionId on AcID.
--     Validates: PN allowed at position (AcPositionPN check).
--     Captures aircraft counter snapshot (AcCounterSnapshot).
--     Triggers: counter reset for SINCE_INSTALL basis counters.
--     Sets SNTaskCounterState.IsFirstDone logic as applicable.
--
--   REMOVE:
--     SerializedItemId removed from AcPositionId.
--     Must match current installation (validated by SP).
--     Captures aircraft counter snapshot.
--     Computes time-on-wing (AcFH_AtEvent - AcFH_AtInstall).
--
--   TRANSFER:
--     Atomic REMOVE from source + INSTALL to destination.
--     Both recorded as separate RecordEvent rows linked by
--     TransferGroupId (same GUID — ties the pair together).
--     Single SP call from UI.
--
--   INSPECT:
--     Task accomplishment on an installed SN.
--     Links to TaskCounterId — which task was accomplished.
--     Calls usp_SNTaskCounter_RecordAccomplishment internally.
--     Captures aircraft counter snapshot.
--
--   AIRCRAFT COUNTER SNAPSHOT:
--     AcCounterSnapshot stores FH/FC/landings/TGO at event time.
--     One row per counter type per event.
--     Enables: time-on-wing calculation, counter proration,
--     audit trail of aircraft state at any historical event.
--
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- TABLE 1: mro2.RecordEvent
--    One row per SN event.
-- ============================================================
IF OBJECT_ID('mro2.RecordEvent','U') IS NULL
BEGIN
    CREATE TABLE mro2.RecordEvent (
        RecordEventId       INT             NOT NULL IDENTITY(1,1),

        -- Event classification
        EventType           VARCHAR(10)     NOT NULL,
            -- 'INSTALL' | 'REMOVE' | 'TRANSFER' | 'INSPECT'

        -- The SN involved
        SerializedItemId    INT             NOT NULL,

        -- Aircraft and position
        AcID                INT             NOT NULL,
        AcPositionId        INT             NOT NULL,

        -- Event date and time
        EventDate           DATE            NOT NULL,
        EventTime           TIME(0)         NULL,       -- optional HH:MM

        -- For TRANSFER: links the REMOVE and INSTALL pair
        -- Same UNIQUEIDENTIFIER on both rows of a transfer
        TransferGroupId     UNIQUEIDENTIFIER NULL,

        -- For INSPECT: which TaskCounter was accomplished
        TaskCounterId       INT             NULL,

        -- Who performed and authorised the work
        PerformedByUserId   NVARCHAR(50)    NOT NULL,
        AuthorisedByUserId  NVARCHAR(50)    NULL,

        -- Work order / job card reference
        WorkOrderRef        NVARCHAR(100)   NULL,

        -- Remarks
        Remarks             NVARCHAR(500)   NULL,

        -- Soft delete — events are never physically deleted
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_RecordEvent_IsActive  DEFAULT (1),

        CreatedDate         DATETIME        NOT NULL
            CONSTRAINT DF_RecordEvent_Created   DEFAULT (GETDATE()),
        CreatedByUserId     NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_RecordEvent PRIMARY KEY (RecordEventId),

        CONSTRAINT FK_RE_SerializedItem
            FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_RE_Aircraft
            FOREIGN KEY (AcID)
            REFERENCES dbo.tblAircraft (AcID),

        CONSTRAINT FK_RE_AcPosition
            FOREIGN KEY (AcPositionId)
            REFERENCES mro2.AcPosition (AcPositionId),

        CONSTRAINT FK_RE_TaskCounter
            FOREIGN KEY (TaskCounterId)
            REFERENCES mro2.TaskCounter (TaskCounterId),

        CONSTRAINT CK_RE_EventType
            CHECK (EventType IN ('INSTALL','REMOVE','TRANSFER','INSPECT')),

        -- INSPECT requires TaskCounterId
        CONSTRAINT CK_RE_InspectNeedsTask
            CHECK (EventType <> 'INSPECT' OR TaskCounterId IS NOT NULL)
    );

    -- Fast lookup: current installation per position
    CREATE INDEX IX_RecordEvent_Position_Date
        ON mro2.RecordEvent (AcPositionId, EventDate DESC, EventType)
        INCLUDE (SerializedItemId, RecordEventId);

    -- Fast lookup: full history per SN
    CREATE INDEX IX_RecordEvent_SN_Date
        ON mro2.RecordEvent (SerializedItemId, EventDate DESC)
        INCLUDE (AcID, AcPositionId, EventType, RecordEventId);

    -- Fast lookup: all events per aircraft
    CREATE INDEX IX_RecordEvent_AcID_Date
        ON mro2.RecordEvent (AcID, EventDate DESC)
        INCLUDE (SerializedItemId, AcPositionId, EventType);

    -- Transfer pair lookup
    CREATE INDEX IX_RecordEvent_TransferGroupId
        ON mro2.RecordEvent (TransferGroupId)
        WHERE TransferGroupId IS NOT NULL;

    PRINT 'mro2.RecordEvent created.';
END
ELSE
    PRINT 'mro2.RecordEvent already exists — skipped.';
GO

-- ============================================================
-- TABLE 2: mro2.AcCounterSnapshot
--    Aircraft counter values captured at each event.
--    One row per counter type per RecordEvent.
--    Stores the RAW aircraft counter value (not SN-level).
--    Used for: time-on-wing calc, counter proration,
--              audit trail of aircraft state.
--
--    CounterValue stored in CounterDef.UnitStorage units:
--      MINUTES type → value in integer minutes
--      COUNT   type → integer count
-- ============================================================
IF OBJECT_ID('mro2.AcCounterSnapshot','U') IS NULL
BEGIN
    CREATE TABLE mro2.AcCounterSnapshot (
        AcCounterSnapshotId INT             NOT NULL IDENTITY(1,1),
        RecordEventId       INT             NOT NULL,
        CounterDefId        INT             NOT NULL,
        CounterValue        INT             NOT NULL,   -- in UnitStorage units

        CONSTRAINT PK_AcCounterSnapshot PRIMARY KEY (AcCounterSnapshotId),

        -- One snapshot per counter per event
        CONSTRAINT UQ_AcCounterSnapshot_Event_Counter
            UNIQUE (RecordEventId, CounterDefId),

        CONSTRAINT FK_AcCS_RecordEvent
            FOREIGN KEY (RecordEventId)
            REFERENCES mro2.RecordEvent (RecordEventId),

        CONSTRAINT FK_AcCS_CounterDef
            FOREIGN KEY (CounterDefId)
            REFERENCES mro2.CounterDef (CounterDefId)
    );

    CREATE INDEX IX_AcCounterSnapshot_EventId
        ON mro2.AcCounterSnapshot (RecordEventId)
        INCLUDE (CounterDefId, CounterValue);

    PRINT 'mro2.AcCounterSnapshot created.';
END
ELSE
    PRINT 'mro2.AcCounterSnapshot already exists — skipped.';
GO

-- ============================================================
-- VIEW: mro2.vw_CurrentInstallation
--    Current SN installed at each active position on each tail.
--    "Current" = most recent INSTALL event with no subsequent
--    REMOVE event at the same position.
--    Shows: SN, PN, position path, install date, time on wing,
--           aircraft FH/FC at installation.
-- ============================================================
IF OBJECT_ID('mro2.vw_CurrentInstallation','V') IS NOT NULL
    DROP VIEW mro2.vw_CurrentInstallation;
GO
CREATE VIEW mro2.vw_CurrentInstallation
AS
-- Last INSTALL event per position that has no subsequent REMOVE
WITH LastInstall AS (
    SELECT
        re.AcPositionId,
        re.AcID,
        re.SerializedItemId,
        re.RecordEventId,
        re.EventDate                AS InstallDate,
        re.WorkOrderRef,
        re.PerformedByUserId,
        -- Row number: latest install per position
        ROW_NUMBER() OVER (
            PARTITION BY re.AcPositionId
            ORDER BY re.EventDate DESC, re.RecordEventId DESC
        ) AS rn
    FROM mro2.RecordEvent re
    WHERE re.EventType = 'INSTALL'
      AND re.IsActive  = 1
),
LastRemove AS (
    SELECT
        re.AcPositionId,
        MAX(re.EventDate)           AS LastRemoveDate
    FROM mro2.RecordEvent re
    WHERE re.EventType IN ('REMOVE','TRANSFER')
      AND re.IsActive = 1
    GROUP BY re.AcPositionId
)
SELECT
    li.AcPositionId,
    li.AcID,
    ac.TailNo,
    acg.AcMainGroup                 AS AcMainGroupName,

    -- Position info
    pos.PositionCode,
    pos.Description                 AS PositionDescription,
    pos.PositionLevel,
    pos.ZoneCode,
    pos.SystemCode,
    pos.FullPath,
    pos.ATACode,

    -- Installed SN
    li.SerializedItemId,
    si.SerialNumber,
    pn.PartNumberId,
    pn.PN,
    pn.Nomenclature,

    -- Install event details
    li.RecordEventId                AS InstallEventId,
    li.InstallDate,
    li.WorkOrderRef                 AS InstallWorkOrderRef,
    li.PerformedByUserId            AS InstalledByUserId,

    -- Aircraft FH at installation (from snapshot — FLIGHT_HOURS counter)
    cs.CounterValue                 AS AcFH_AtInstall,

    -- Days on wing
    DATEDIFF(DAY, li.InstallDate, CAST(GETDATE() AS DATE))
                                    AS DaysOnWing

FROM LastInstall li
INNER JOIN LastRemove lr
    ON lr.AcPositionId = li.AcPositionId
    -- Only show if no remove happened AFTER the last install
    -- If LastRemoveDate < InstallDate → SN is still installed
    -- Using RIGHT join trick: include positions with no remove
RIGHT JOIN mro2.vw_AcPositionTree pos
    ON pos.AcPositionId = li.AcPositionId

INNER JOIN dbo.tblAircraft          ac  ON ac.AcID          = li.AcID
INNER JOIN dbo.tblAcMainGroup       acg ON acg.AcMainGroupID= ac.AcMainGroupID
INNER JOIN mro2.SerializedItem      si  ON si.SerializedItemId = li.SerializedItemId
INNER JOIN mro2.PartNumber          pn  ON pn.PartNumberId   = si.PartNumberId

-- FH snapshot at install
LEFT JOIN mro2.AcCounterSnapshot    cs  ON cs.RecordEventId  = li.RecordEventId
LEFT JOIN mro2.CounterDef           cd  ON cd.CounterDefId   = cs.CounterDefId
                                       AND cd.Code           = 'AF_FLIGHT_MIN'

WHERE li.rn = 1
  AND (lr.LastRemoveDate IS NULL
       OR lr.LastRemoveDate < li.InstallDate)
  AND pos.PositionLevel = 3   -- only show slot-level positions
  AND pos.IsActive      = 1;
GO
PRINT 'mro2.vw_CurrentInstallation created.';
GO

-- ============================================================
-- VIEW: mro2.vw_SNHistory
--    Full install/remove history for any SN.
--    Pairs INSTALL and REMOVE events to compute time-on-wing
--    per installation period.
-- ============================================================
IF OBJECT_ID('mro2.vw_SNHistory','V') IS NOT NULL
    DROP VIEW mro2.vw_SNHistory;
GO
CREATE VIEW mro2.vw_SNHistory
AS
SELECT
    re.RecordEventId,
    re.SerializedItemId,
    si.SerialNumber,
    pn.PN,
    pn.Nomenclature,
    re.EventType,
    re.AcID,
    ac.TailNo,
    re.AcPositionId,
    pos.PositionCode,
    pos.FullPath,
    re.EventDate,
    re.EventTime,
    re.WorkOrderRef,
    re.Remarks,
    re.TransferGroupId,
    re.TaskCounterId,
    tc.TaskCounterId                AS LinkedTaskCounterId,
    re.PerformedByUserId,
    re.AuthorisedByUserId,
    re.CreatedDate,
    -- Aircraft FH at this event
    cs_fh.CounterValue              AS AcFH_AtEvent,
    -- Aircraft FC at this event
    cs_fc.CounterValue              AS AcFC_AtEvent
FROM mro2.RecordEvent re
INNER JOIN mro2.SerializedItem      si  ON si.SerializedItemId = re.SerializedItemId
INNER JOIN mro2.PartNumber          pn  ON pn.PartNumberId     = si.PartNumberId
INNER JOIN dbo.tblAircraft          ac  ON ac.AcID             = re.AcID
INNER JOIN mro2.AcPosition          ap  ON ap.AcPositionId     = re.AcPositionId
INNER JOIN mro2.vw_AcPositionTree   pos ON pos.AcPositionId    = re.AcPositionId
LEFT  JOIN mro2.TaskCounter         tc  ON tc.TaskCounterId    = re.TaskCounterId
-- FH snapshot
LEFT  JOIN mro2.AcCounterSnapshot   cs_fh ON cs_fh.RecordEventId = re.RecordEventId
LEFT  JOIN mro2.CounterDef          cd_fh ON cd_fh.CounterDefId  = cs_fh.CounterDefId
                                         AND cd_fh.Code          = 'AF_FLIGHT_MIN'
-- FC snapshot
LEFT  JOIN mro2.AcCounterSnapshot   cs_fc ON cs_fc.RecordEventId = re.RecordEventId
LEFT  JOIN mro2.CounterDef          cd_fc ON cd_fc.CounterDefId  = cs_fc.CounterDefId
                                         AND cd_fc.Code          = 'AF_CYCLES'
WHERE re.IsActive = 1;
GO
PRINT 'mro2.vw_SNHistory created.';
GO

-- ============================================================
-- SP: mro2.usp_RecordEvent_Install
--    Records an INSTALL event.
--    Validates:
--      1. PN is allowed at this position (AcPositionPN check)
--      2. Position is not already occupied (no current install)
--    On success:
--      - Inserts RecordEvent row
--      - Inserts AcCounterSnapshot rows
--      - Calls usp_SNTaskCounterState_Update for SINCE_INSTALL
--        basis counters to reset them
--    Returns: new RecordEventId
-- ============================================================
IF OBJECT_ID('mro2.usp_RecordEvent_Install','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_RecordEvent_Install;
GO
CREATE PROCEDURE mro2.usp_RecordEvent_Install
    @SerializedItemId   INT,
    @AcID               INT,
    @AcPositionId       INT,
    @EventDate          DATE,
    @EventTime          TIME(0)         = NULL,
    @WorkOrderRef       NVARCHAR(100)   = NULL,
    @Remarks            NVARCHAR(500)   = NULL,
    @PerformedByUserId  NVARCHAR(50),
    @AuthorisedByUserId NVARCHAR(50)    = NULL,
    -- Aircraft counter snapshots at install time
    -- Pass as comma-separated CounterDefId:Value pairs via temp table
    -- or individually for the most common counters:
    @AcFH_Minutes       INT             = NULL,   -- AF_FLIGHT_MIN value
    @AcFC               INT             = NULL,   -- AF_CYCLES value
    @AcLandings         INT             = NULL,   -- AF_LANDINGS value
    @AcTGO              INT             = NULL,   -- AF_TOUCH_AND_GO value
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- ── 1. Validate: PN allowed at this position ─────────────
    DECLARE @PartNumberId INT;
    SELECT @PartNumberId = PartNumberId
    FROM mro2.SerializedItem
    WHERE SerializedItemId = @SerializedItemId;

    DECLARE @AcPositionTemplateId INT;
    SELECT @AcPositionTemplateId = AcPositionTemplateId
    FROM mro2.AcPosition
    WHERE AcPositionId = @AcPositionId;

    IF @AcPositionTemplateId IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM mro2.AcPositionPN
            WHERE AcPositionTemplateId = @AcPositionTemplateId
              AND PartNumberId         = @PartNumberId
              AND IsActive             = 1)
        BEGIN
            RAISERROR(
                'PN is not authorised for this position. Add it to AcPositionPN first.',
                16, 1);
            RETURN;
        END
    END

    -- ── 2. Validate: position not already occupied ───────────
    IF EXISTS (
        SELECT 1 FROM mro2.vw_CurrentInstallation
        WHERE AcPositionId = @AcPositionId)
    BEGIN
        RAISERROR(
            'Position already occupied. Remove current SN before installing.',
            16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    -- ── 3. Insert RecordEvent ─────────────────────────────────
    INSERT INTO mro2.RecordEvent (
        EventType, SerializedItemId, AcID, AcPositionId,
        EventDate, EventTime, WorkOrderRef, Remarks,
        PerformedByUserId, AuthorisedByUserId,
        CreatedByUserId)
    VALUES (
        'INSTALL', @SerializedItemId, @AcID, @AcPositionId,
        @EventDate, @EventTime, @WorkOrderRef, @Remarks,
        @PerformedByUserId, @AuthorisedByUserId,
        @UserId);

    DECLARE @EventId INT = SCOPE_IDENTITY();

    -- ── 4. Insert counter snapshots ───────────────────────────
    IF @AcFH_Minutes IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcFH_Minutes
        FROM mro2.CounterDef WHERE Code = 'AF_FLIGHT_MIN';

    IF @AcFC IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcFC
        FROM mro2.CounterDef WHERE Code = 'AF_CYCLES';

    IF @AcLandings IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcLandings
        FROM mro2.CounterDef WHERE Code = 'AF_LANDINGS';

    IF @AcTGO IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcTGO
        FROM mro2.CounterDef WHERE Code = 'AF_TOUCH_AND_GO';

    -- ── 5. Reset SINCE_INSTALL counters for this SN ───────────
    -- For every TaskCounter on this SN's PN that uses SINCE_INSTALL basis,
    -- update SNTaskCounterState to reset accumulated to 0
    UPDATE st SET
        AccumulatedSinceLast  = 0,
        LastDoneAt            = @AcFH_Minutes,
        LastDoneDate          = @EventDate,
        LastUpdatedDate       = GETDATE(),
        LastUpdatedByUserId   = @UserId
    FROM mro2.SNTaskCounterState st
    INNER JOIN mro2.TaskCounter tc
        ON tc.TaskCounterId = st.TaskCounterId
    INNER JOIN mro2.PNLimit pl
        ON pl.PNLimitId = tc.PNLimitId
    INNER JOIN mro2.CounterBasis cb
        ON cb.CounterBasisId = tc.CounterBasisId
       AND cb.Code = 'SINCE_INSTALL'
    WHERE st.SerializedItemId = @SerializedItemId
      AND pl.PartNumberId     = @PartNumberId;

    COMMIT TRANSACTION;

    -- Return new event ID
    SELECT @EventId AS RecordEventId,
           'INSTALL' AS EventType,
           @SerializedItemId AS SerializedItemId,
           @AcPositionId AS AcPositionId,
           @EventDate AS EventDate;
END
GO

-- ============================================================
-- SP: mro2.usp_RecordEvent_Remove
--    Records a REMOVE event.
--    Validates SN is currently installed at this position.
--    Captures counter snapshot. Computes time on wing.
-- ============================================================
IF OBJECT_ID('mro2.usp_RecordEvent_Remove','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_RecordEvent_Remove;
GO
CREATE PROCEDURE mro2.usp_RecordEvent_Remove
    @SerializedItemId   INT,
    @AcID               INT,
    @AcPositionId       INT,
    @EventDate          DATE,
    @EventTime          TIME(0)         = NULL,
    @WorkOrderRef       NVARCHAR(100)   = NULL,
    @Remarks            NVARCHAR(500)   = NULL,
    @PerformedByUserId  NVARCHAR(50),
    @AuthorisedByUserId NVARCHAR(50)    = NULL,
    @AcFH_Minutes       INT             = NULL,
    @AcFC               INT             = NULL,
    @AcLandings         INT             = NULL,
    @AcTGO              INT             = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- ── 1. Validate: SN currently installed at this position ──
    IF NOT EXISTS (
        SELECT 1 FROM mro2.vw_CurrentInstallation
        WHERE AcPositionId     = @AcPositionId
          AND SerializedItemId = @SerializedItemId)
    BEGIN
        RAISERROR(
            'This SN is not currently installed at the specified position.',
            16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    -- ── 2. Insert RecordEvent ─────────────────────────────────
    INSERT INTO mro2.RecordEvent (
        EventType, SerializedItemId, AcID, AcPositionId,
        EventDate, EventTime, WorkOrderRef, Remarks,
        PerformedByUserId, AuthorisedByUserId,
        CreatedByUserId)
    VALUES (
        'REMOVE', @SerializedItemId, @AcID, @AcPositionId,
        @EventDate, @EventTime, @WorkOrderRef, @Remarks,
        @PerformedByUserId, @AuthorisedByUserId,
        @UserId);

    DECLARE @EventId INT = SCOPE_IDENTITY();

    -- ── 3. Counter snapshots ──────────────────────────────────
    IF @AcFH_Minutes IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcFH_Minutes
        FROM mro2.CounterDef WHERE Code = 'AF_FLIGHT_MIN';

    IF @AcFC IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcFC
        FROM mro2.CounterDef WHERE Code = 'AF_CYCLES';

    IF @AcLandings IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcLandings
        FROM mro2.CounterDef WHERE Code = 'AF_LANDINGS';

    IF @AcTGO IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcTGO
        FROM mro2.CounterDef WHERE Code = 'AF_TOUCH_AND_GO';

    COMMIT TRANSACTION;

    -- Return time-on-wing computation
    SELECT
        @EventId        AS RecordEventId,
        ci.InstallDate,
        @EventDate      AS RemoveDate,
        DATEDIFF(DAY, ci.InstallDate, @EventDate) AS DaysOnWing,
        ci.AcFH_AtInstall,
        @AcFH_Minutes   AS AcFH_AtRemove,
        CASE WHEN @AcFH_Minutes IS NOT NULL AND ci.AcFH_AtInstall IS NOT NULL
             THEN @AcFH_Minutes - ci.AcFH_AtInstall
             ELSE NULL END AS FH_OnWing_Minutes
    FROM mro2.vw_CurrentInstallation ci
    WHERE ci.AcPositionId     = @AcPositionId
      AND ci.SerializedItemId = @SerializedItemId;
END
GO

-- ============================================================
-- SP: mro2.usp_RecordEvent_Transfer
--    Atomic REMOVE from source + INSTALL to destination.
--    Both rows share a TransferGroupId (UNIQUEIDENTIFIER).
--    Single transaction — rolls back both if either fails.
-- ============================================================
IF OBJECT_ID('mro2.usp_RecordEvent_Transfer','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_RecordEvent_Transfer;
GO
CREATE PROCEDURE mro2.usp_RecordEvent_Transfer
    @SerializedItemId       INT,
    -- Source (where SN is coming FROM)
    @FromAcID               INT,
    @FromAcPositionId       INT,
    -- Destination (where SN is going TO)
    @ToAcID                 INT,
    @ToAcPositionId         INT,
    @EventDate              DATE,
    @EventTime              TIME(0)         = NULL,
    @WorkOrderRef           NVARCHAR(100)   = NULL,
    @Remarks                NVARCHAR(500)   = NULL,
    @PerformedByUserId      NVARCHAR(50),
    @AuthorisedByUserId     NVARCHAR(50)    = NULL,
    @AcFH_Minutes           INT             = NULL,
    @AcFC                   INT             = NULL,
    @UserId                 NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Shared transfer group ID ties the two rows together
    DECLARE @TransferGroupId UNIQUEIDENTIFIER = NEWID();

    BEGIN TRANSACTION;

    -- ── 1. REMOVE from source ─────────────────────────────────
    INSERT INTO mro2.RecordEvent (
        EventType, SerializedItemId, AcID, AcPositionId,
        EventDate, EventTime, TransferGroupId,
        WorkOrderRef, Remarks,
        PerformedByUserId, AuthorisedByUserId,
        CreatedByUserId)
    VALUES (
        'TRANSFER', @SerializedItemId, @FromAcID, @FromAcPositionId,
        @EventDate, @EventTime, @TransferGroupId,
        @WorkOrderRef,
        ISNULL(@Remarks,'') + ' [TRANSFER FROM]',
        @PerformedByUserId, @AuthorisedByUserId,
        @UserId);

    DECLARE @RemoveEventId INT = SCOPE_IDENTITY();

    -- ── 2. INSTALL at destination ─────────────────────────────
    INSERT INTO mro2.RecordEvent (
        EventType, SerializedItemId, AcID, AcPositionId,
        EventDate, EventTime, TransferGroupId,
        WorkOrderRef, Remarks,
        PerformedByUserId, AuthorisedByUserId,
        CreatedByUserId)
    VALUES (
        'TRANSFER', @SerializedItemId, @ToAcID, @ToAcPositionId,
        @EventDate, @EventTime, @TransferGroupId,
        @WorkOrderRef,
        ISNULL(@Remarks,'') + ' [TRANSFER TO]',
        @PerformedByUserId, @AuthorisedByUserId,
        @UserId);

    DECLARE @InstallEventId INT = SCOPE_IDENTITY();

    -- ── 3. Counter snapshots on both rows ─────────────────────
    IF @AcFH_Minutes IS NOT NULL
    BEGIN
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @RemoveEventId, CounterDefId, @AcFH_Minutes
        FROM mro2.CounterDef WHERE Code = 'AF_FLIGHT_MIN';

        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @InstallEventId, CounterDefId, @AcFH_Minutes
        FROM mro2.CounterDef WHERE Code = 'AF_FLIGHT_MIN';
    END

    IF @AcFC IS NOT NULL
    BEGIN
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @RemoveEventId, CounterDefId, @AcFC
        FROM mro2.CounterDef WHERE Code = 'AF_CYCLES';

        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @InstallEventId, CounterDefId, @AcFC
        FROM mro2.CounterDef WHERE Code = 'AF_CYCLES';
    END

    -- ── 4. Reset SINCE_INSTALL counters at destination ────────
    DECLARE @PartNumberId INT;
    SELECT @PartNumberId = PartNumberId FROM mro2.SerializedItem
    WHERE SerializedItemId = @SerializedItemId;

    UPDATE st SET
        AccumulatedSinceLast  = 0,
        LastDoneAt            = @AcFH_Minutes,
        LastDoneDate          = @EventDate,
        LastUpdatedDate       = GETDATE(),
        LastUpdatedByUserId   = @UserId
    FROM mro2.SNTaskCounterState st
    INNER JOIN mro2.TaskCounter  tc ON tc.TaskCounterId  = st.TaskCounterId
    INNER JOIN mro2.PNLimit      pl ON pl.PNLimitId      = tc.PNLimitId
    INNER JOIN mro2.CounterBasis cb ON cb.CounterBasisId = tc.CounterBasisId
                                   AND cb.Code = 'SINCE_INSTALL'
    WHERE st.SerializedItemId = @SerializedItemId
      AND pl.PartNumberId     = @PartNumberId;

    COMMIT TRANSACTION;

    SELECT @TransferGroupId AS TransferGroupId,
           @RemoveEventId   AS RemoveEventId,
           @InstallEventId  AS InstallEventId;
END
GO

-- ============================================================
-- SP: mro2.usp_RecordEvent_Inspect
--    Records task accomplishment on an installed SN.
--    Calls usp_SNTaskCounter_RecordAccomplishment.
--    Captures aircraft counter snapshot.
-- ============================================================
IF OBJECT_ID('mro2.usp_RecordEvent_Inspect','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_RecordEvent_Inspect;
GO
CREATE PROCEDURE mro2.usp_RecordEvent_Inspect
    @SerializedItemId   INT,
    @AcID               INT,
    @AcPositionId       INT,
    @TaskCounterId      INT,
    @EventDate          DATE,
    @EventTime          TIME(0)         = NULL,
    @WorkOrderRef       NVARCHAR(100)   = NULL,
    @Remarks            NVARCHAR(500)   = NULL,
    @PerformedByUserId  NVARCHAR(50),
    @AuthorisedByUserId NVARCHAR(50)    = NULL,
    @AcFH_Minutes       INT             = NULL,
    @AcFC               INT             = NULL,
    @AcLandings         INT             = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- ── 1. Insert INSPECT event ───────────────────────────────
    INSERT INTO mro2.RecordEvent (
        EventType, SerializedItemId, AcID, AcPositionId,
        TaskCounterId,
        EventDate, EventTime, WorkOrderRef, Remarks,
        PerformedByUserId, AuthorisedByUserId,
        CreatedByUserId)
    VALUES (
        'INSPECT', @SerializedItemId, @AcID, @AcPositionId,
        @TaskCounterId,
        @EventDate, @EventTime, @WorkOrderRef, @Remarks,
        @PerformedByUserId, @AuthorisedByUserId,
        @UserId);

    DECLARE @EventId INT = SCOPE_IDENTITY();

    -- ── 2. Counter snapshots ──────────────────────────────────
    IF @AcFH_Minutes IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcFH_Minutes
        FROM mro2.CounterDef WHERE Code = 'AF_FLIGHT_MIN';

    IF @AcFC IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcFC
        FROM mro2.CounterDef WHERE Code = 'AF_CYCLES';

    IF @AcLandings IS NOT NULL
        INSERT INTO mro2.AcCounterSnapshot (RecordEventId, CounterDefId, CounterValue)
        SELECT @EventId, CounterDefId, @AcLandings
        FROM mro2.CounterDef WHERE Code = 'AF_LANDINGS';

    -- ── 3. Record task accomplishment → resets counter ────────
    -- Use aircraft FH as the lifetime value at accomplishment
    EXEC mro2.usp_SNTaskCounter_RecordAccomplishment
        @SerializedItemId = @SerializedItemId,
        @TaskCounterId    = @TaskCounterId,
        @AccomplishedAt   = @AcFH_Minutes,
        @AccomplishedDate = @EventDate,
        @UserId           = @UserId;

    COMMIT TRANSACTION;

    SELECT @EventId AS RecordEventId,
           'INSPECT' AS EventType,
           @SerializedItemId AS SerializedItemId,
           @TaskCounterId AS TaskCounterId,
           @EventDate AS EventDate;
END
GO

-- ============================================================
-- SP: mro2.usp_RecordEvent_GetHistory
--    Full history for a given SN or position.
--    @SerializedItemId: filter by SN (NULL = all)
--    @AcPositionId    : filter by position (NULL = all)
--    @AcID            : filter by tail (NULL = all)
-- ============================================================
IF OBJECT_ID('mro2.usp_RecordEvent_GetHistory','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_RecordEvent_GetHistory;
GO
CREATE PROCEDURE mro2.usp_RecordEvent_GetHistory
    @SerializedItemId   INT  = NULL,
    @AcPositionId       INT  = NULL,
    @AcID               INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_SNHistory
    WHERE (@SerializedItemId IS NULL OR SerializedItemId = @SerializedItemId)
      AND (@AcPositionId     IS NULL OR AcPositionId     = @AcPositionId)
      AND (@AcID             IS NULL OR AcID             = @AcID)
    ORDER BY EventDate DESC, RecordEventId DESC;
END
GO

-- ============================================================
-- STEP 08 VERIFICATION
-- ============================================================
/*
-- Tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='mro2'
  AND TABLE_NAME IN ('RecordEvent','AcCounterSnapshot')
ORDER BY TABLE_NAME;

-- Views
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA='mro2'
  AND TABLE_NAME IN ('vw_CurrentInstallation','vw_SNHistory');

-- SPs (expect 6)
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA='mro2'
  AND ROUTINE_NAME IN (
    'usp_RecordEvent_Install',
    'usp_RecordEvent_Remove',
    'usp_RecordEvent_Transfer',
    'usp_RecordEvent_Inspect',
    'usp_RecordEvent_GetHistory',
    'usp_AcPosition_CopyFromTemplate')
ORDER BY ROUTINE_NAME;

-- ── SAMPLE USAGE ──────────────────────────────────────────
-- Install SN on aircraft
-- EXEC mro2.usp_RecordEvent_Install
--     @SerializedItemId  = 1,
--     @AcID              = 5,
--     @AcPositionId      = 12,
--     @EventDate         = '2024-11-20',
--     @WorkOrderRef      = 'WO-2024-1234',
--     @AcFH_Minutes      = 256800,   -- 4280 FH in minutes
--     @AcFC              = 3210,
--     @AcLandings        = 3190,
--     @PerformedByUserId = 'TECH01',
--     @UserId            = 'TECH01';

-- Remove SN from aircraft
-- EXEC mro2.usp_RecordEvent_Remove
--     @SerializedItemId  = 1,
--     @AcID              = 5,
--     @AcPositionId      = 12,
--     @EventDate         = '2024-12-15',
--     @WorkOrderRef      = 'WO-2024-1567',
--     @AcFH_Minutes      = 261600,   -- 4360 FH
--     @AcFC              = 3290,
--     @PerformedByUserId = 'TECH01',
--     @UserId            = 'TECH01';

-- Transfer SN from one aircraft to another
-- EXEC mro2.usp_RecordEvent_Transfer
--     @SerializedItemId  = 1,
--     @FromAcID          = 5,  @FromAcPositionId = 12,
--     @ToAcID            = 7,  @ToAcPositionId   = 15,
--     @EventDate         = '2024-12-20',
--     @AcFH_Minutes      = 261600,
--     @PerformedByUserId = 'TECH01',
--     @UserId            = 'TECH01';

-- Record inspection / task accomplishment
-- EXEC mro2.usp_RecordEvent_Inspect
--     @SerializedItemId  = 1,
--     @AcID              = 5,
--     @AcPositionId      = 12,
--     @TaskCounterId     = 3,
--     @EventDate         = '2024-11-20',
--     @WorkOrderRef      = 'WO-2024-1234',
--     @AcFH_Minutes      = 256800,
--     @PerformedByUserId = 'TECH01',
--     @UserId            = 'TECH01';

-- View current configuration of a tail
-- SELECT * FROM mro2.vw_CurrentInstallation WHERE AcID = 5;

-- Full SN history
-- EXEC mro2.usp_RecordEvent_GetHistory @SerializedItemId = 1;
*/

PRINT '── Step 08 complete ─────────────────────────────────────';
