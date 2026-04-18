-- ============================================================
-- MRO2 — Counter Extension Schema
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- Run after: MRO2_TaskCounter_Schema.sql
-- ============================================================
--
-- DESIGN DECISIONS (locked):
--
-- 1. ONE active extension per SN per TaskCounter at a time.
--    Enforced by filtered unique index on IsActive=1.
--    Previous extensions are kept as historical record (IsActive=0).
--    Unlimited cycles: each accomplishment resets, next extension
--    is a new row for the new cycle.
--
-- 2. Extension expires on task accomplishment.
--    usp_SNTaskCounter_RecordAccomplishment calls
--    usp_SNTaskCounterExtension_Expire to deactivate the
--    current extension before advancing NextDueAt.
--
-- 3. TWO extension value mechanisms:
--    ExtensionType = 'VALUE' : fixed units added to NextDueAt
--                              ExtensionValue = 50 (FH/FC/days)
--    ExtensionType = 'PCT'   : percentage of CurrentInterval
--                              ExtensionValue = 10 (%)
--                              ComputedExtension = Interval * 10/100
--    Both stored: ExtensionValue (what was requested),
--                 ComputedExtensionUnits (what was applied in units).
--    Computed value is what gets added to NextDueAt.
--
-- 4. FIVE reason categories:
--    DOC_REF     : Document reference (CMM, SB, AD, EO)
--    MFR_TOL     : Manufacturer pre-approved tolerance in CMM
--    OPS_NEC     : Operational necessity (ops approval required)
--    REG_AUTH    : Regulatory authority approval (MARC/DGAM)
--    PARTS_AVAIL : Spare parts not available
--
-- 5. ExtensionLimit on TaskCounter (optional):
--    Some CMMs pre-define the maximum allowable extension.
--    e.g. CMM 29-10-00: max 10% or 150 FH, whichever is less.
--    Stored on TaskCounter so UI can enforce it on input.
--    NULL = no pre-defined limit (extension per case approval).
--
-- TABLE CREATION ORDER:
--   1. ALTER mro2.TaskCounter  (add extension limit columns)
--   2. mro2.ExtensionReason    (lookup — reason categories)
--   3. mro2.SNTaskCounterExtension
--   4. Indexes
--   5. Updated view vw_SNTaskCounterStatus
--   6. SPs
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- 1. ALTER mro2.TaskCounter
--    Add pre-approved extension limits (from CMM/manufacturer).
--    MaxExtensionPct   : max % of interval allowed (e.g. 10)
--    MaxExtensionValue : max fixed value allowed (e.g. 150 FH)
--    When both set: whichever is LESS applies (conservative).
--    When both NULL: no pre-defined limit, case-by-case approval.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('mro2.TaskCounter')
      AND name = 'MaxExtensionPct')
BEGIN
    ALTER TABLE mro2.TaskCounter
        ADD MaxExtensionPct     TINYINT NULL,   -- e.g. 10 (= 10%)
            MaxExtensionValue   INT     NULL;   -- e.g. 150 (FH/FC/days)
END
GO

-- ============================================================
-- 2. mro2.ExtensionReason
--    Lookup table for extension reason categories.
--    Seeded with the 5 confirmed categories.
--    RequiresDocRef  : must supply a document reference
--    RequiresApprover: must supply approver name/role
-- ============================================================
IF OBJECT_ID('mro2.ExtensionReason','U') IS NULL
BEGIN
    CREATE TABLE mro2.ExtensionReason (
        ExtensionReasonId   TINYINT         NOT NULL IDENTITY(1,1),
        Code                VARCHAR(20)     NOT NULL,
        Name                NVARCHAR(150)   NOT NULL,
        Description         NVARCHAR(300)   NULL,
        -- Drives mandatory field validation in UI
        RequiresDocRef      BIT             NOT NULL
            CONSTRAINT DF_ExtReason_DocRef    DEFAULT (0),
        RequiresApprover    BIT             NOT NULL
            CONSTRAINT DF_ExtReason_Approver  DEFAULT (0),
        -- Badge color for display
        BadgeColor          VARCHAR(20)     NOT NULL
            CONSTRAINT DF_ExtReason_Color     DEFAULT ('secondary'),
        SortOrder           TINYINT         NOT NULL
            CONSTRAINT DF_ExtReason_Sort      DEFAULT (99),
        IsActive            BIT             NOT NULL
            CONSTRAINT DF_ExtReason_IsActive  DEFAULT (1),

        CONSTRAINT PK_ExtensionReason PRIMARY KEY (ExtensionReasonId),
        CONSTRAINT UQ_ExtensionReason_Code UNIQUE (Code)
    );

    INSERT INTO mro2.ExtensionReason
        (Code, Name, Description,
         RequiresDocRef, RequiresApprover,
         BadgeColor, SortOrder)
    VALUES
        ('MFR_TOL',
         'Tol&eacute;rance Constructeur',
         'Extension pr&eacute;approuv&eacute;e par le constructeur '
         + 'dans le CMM/AMM. Limite d&eacute;finie dans le manuel.',
         1, 0, 'info', 1),

        ('DOC_REF',
         'R&eacute;f&eacute;rence Documentaire',
         'Extension autoris&eacute;e par un document technique : '
         + 'CMM, SB, AD, Ordre d''Ing&eacute;nierie.',
         1, 1, 'primary', 2),

        ('OPS_NEC',
         'N&eacute;cessit&eacute; Op&eacute;rationnelle',
         'Extension pour raison op&eacute;rationnelle. '
         + 'Approbation du chef des op&eacute;rations requise.',
         0, 1, 'warning', 3),

        ('REG_AUTH',
         'Autorit&eacute; R&eacute;glementaire',
         'Extension approuv&eacute;e par l''autorit&eacute; '
         + 'r&eacute;glementaire (MARC/DGAM/autorit&eacute; comp&eacute;tente).',
         1, 1, 'danger', 4),

        ('PARTS_AVAIL',
         'Indisponibilit&eacute; Pi&egrave;ces',
         'Pi&egrave;ces de rechange non disponibles. '
         + 'Extension en attente de r&eacute;approvisionnement.',
         0, 1, 'secondary', 5);
END
GO

-- ============================================================
-- 3. mro2.SNTaskCounterExtension
--    One row per extension event per SN per TaskCounter.
--    Only one row with IsActive=1 per (SN, TaskCounter) at a time
--    (enforced by filtered unique index below).
--    Historical rows (IsActive=0) are kept permanently — audit.
--
--    EXTENSION VALUE LOGIC:
--      ExtensionType = 'VALUE':
--        ComputedExtensionUnits = ExtensionValue
--        (e.g. ExtensionValue=50 → add 50 FH)
--      ExtensionType = 'PCT':
--        ComputedExtensionUnits = CurrentInterval * ExtensionValue / 100
--        (e.g. Interval=1500 FH, ExtensionValue=10 → add 150 FH)
--
--    ExtendedNextDueAt = OriginalNextDueAt + ComputedExtensionUnits
--    This value overrides SNTaskCounterState.NextDueAt while active.
--
--    MaxAllowedExtension: computed at save time from TaskCounter
--    pre-approved limits. Stored for audit — shows what the limit
--    was at the time of the extension request.
-- ============================================================
IF OBJECT_ID('mro2.SNTaskCounterExtension','U') IS NULL
BEGIN
    CREATE TABLE mro2.SNTaskCounterExtension (
        SNTaskCounterExtensionId    INT             NOT NULL IDENTITY(1,1),
        SerializedItemId            INT             NOT NULL,
        TaskCounterId               INT             NOT NULL,
        ExtensionReasonId           TINYINT         NOT NULL,

        -- Extension value: what was requested
        ExtensionType               VARCHAR(5)      NOT NULL,
            -- 'VALUE' : fixed units
            -- 'PCT'   : percentage of current interval
        ExtensionValue              DECIMAL(8,2)    NOT NULL,
            -- VALUE mode: units (e.g. 50.0 FH, 30.0 days)
            -- PCT mode:   percent (e.g. 10.0 %)

        -- Extension value: what was computed and applied
        -- Stored at creation time — immutable audit record
        CurrentIntervalAtExtension  INT             NOT NULL,
            -- Interval value active when extension was granted
        ComputedExtensionUnits      INT             NOT NULL,
            -- Actual units added to NextDueAt
            -- VALUE: = ExtensionValue (rounded to INT)
            -- PCT:   = FLOOR(CurrentInterval * ExtensionValue / 100)

        -- Pre-approved limit at time of extension (audit snapshot)
        MaxAllowedPct               TINYINT         NULL,
        MaxAllowedValue             INT             NULL,

        -- Original and extended due points
        OriginalNextDueAt           INT             NOT NULL,
        ExtendedNextDueAt           INT             NOT NULL,
            -- = OriginalNextDueAt + ComputedExtensionUnits

        -- Mandatory audit fields
        Justification               NVARCHAR(500)   NOT NULL,
        DocReference                NVARCHAR(200)   NULL,
            -- CMM ref, SB number, EO number, AD number
        ApprovedBy                  NVARCHAR(100)   NULL,
            -- Name and role of approver
        ApprovalDate                DATE            NOT NULL,

        -- Status
        IsActive                    BIT             NOT NULL
            CONSTRAINT DF_SNTCExt_IsActive      DEFAULT (1),
            -- 1 = currently in effect (NextDueAt overridden)
            -- 0 = expired (task accomplished or manually revoked)

        -- How it was closed
        ExpiredReason               VARCHAR(20)     NULL,
            -- 'ACCOMPLISHED' : task done, extension consumed
            -- 'REVOKED'      : manually cancelled
            -- NULL           : still active
        ExpiredDate                 DATE            NULL,
        ExpiredByUserId             NVARCHAR(50)    NULL,

        CreatedDate                 DATETIME        NOT NULL
            CONSTRAINT DF_SNTCExt_Created       DEFAULT (GETDATE()),
        CreatedByUserId             NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_SNTaskCounterExtension
            PRIMARY KEY (SNTaskCounterExtensionId),

        CONSTRAINT FK_SNTCExt_SN FOREIGN KEY (SerializedItemId)
            REFERENCES mro2.SerializedItem (SerializedItemId),

        CONSTRAINT FK_SNTCExt_TC FOREIGN KEY (TaskCounterId)
            REFERENCES mro2.TaskCounter (TaskCounterId),

        CONSTRAINT FK_SNTCExt_Reason FOREIGN KEY (ExtensionReasonId)
            REFERENCES mro2.ExtensionReason (ExtensionReasonId),

        CONSTRAINT CK_SNTCExt_Type
            CHECK (ExtensionType IN ('VALUE','PCT')),

        CONSTRAINT CK_SNTCExt_Value
            CHECK (ExtensionValue > 0),

        CONSTRAINT CK_SNTCExt_Computed
            CHECK (ComputedExtensionUnits > 0),

        CONSTRAINT CK_SNTCExt_Extended
            CHECK (ExtendedNextDueAt > OriginalNextDueAt),

        CONSTRAINT CK_SNTCExt_ExpiredReason
            CHECK (ExpiredReason IS NULL
                   OR ExpiredReason IN ('ACCOMPLISHED','REVOKED'))
    );
END
GO

-- ============================================================
-- INDEXES
-- ============================================================

-- Filtered unique index: only one ACTIVE extension per SN+TC
-- This is the enforcement mechanism for "one per cycle"
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name = 'UIX_SNTaskCounterExtension_Active'
    AND object_id = OBJECT_ID('mro2.SNTaskCounterExtension'))
    CREATE UNIQUE INDEX UIX_SNTaskCounterExtension_Active
        ON mro2.SNTaskCounterExtension (SerializedItemId, TaskCounterId)
        WHERE IsActive = 1;
GO

-- History lookup: all extensions for a given SN+TC ordered newest first
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SNTaskCounterExtension_SN_TC'
    AND object_id = OBJECT_ID('mro2.SNTaskCounterExtension'))
    CREATE INDEX IX_SNTaskCounterExtension_SN_TC
        ON mro2.SNTaskCounterExtension
            (SerializedItemId, TaskCounterId, CreatedDate DESC)
        INCLUDE (IsActive, ExtensionType, ExtensionValue,
                 ComputedExtensionUnits, ExtendedNextDueAt,
                 ExpiredReason);
GO

-- ============================================================
-- UPDATE vw_SNTaskCounterStatus
-- Now incorporates active extension into NextDueAt and status.
-- ExtendedNextDueAt overrides base NextDueAt when active.
-- Adds extension columns for display on SN detail page.
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
    cd.Code                                             AS CounterDefCode,
    cd.AppliesToAssetKindCode,
    ct.CounterTypeId,
    ct.Code                                             AS CounterTypeCode,
    ct.DisplayUnit,
    ct.UnitStorage,

    -- Counter basis
    cb.CounterBasisId,
    cb.Code                                             AS CounterBasisCode,
    cb.Name                                             AS CounterBasisName,

    -- Effective task counter values
    -- (SN override wins field-by-field)
    COALESCE(ov.FirstThreshold,    tc.FirstThreshold)   AS EffFirstThreshold,
    COALESCE(ov.RepeatInterval,    tc.RepeatInterval)    AS EffRepeatInterval,
    COALESCE(ov.Ceiling,           tc.Ceiling)           AS EffCeiling,
    COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct) AS EffAlertPct,

    -- Pre-approved extension limits
    tc.MaxExtensionPct,
    tc.MaxExtensionValue,

    -- Override source
    CASE WHEN ov.SNTaskCounterOverrideId IS NOT NULL
         THEN 'OVERRIDE' ELSE 'PN_DEFAULT' END           AS ValueSource,
    ov.OverrideReason,
    ov.AuthorisedRef,

    -- State
    st.IsFirstDone,
    st.AccumulatedSinceLast,
    st.LifetimeTotal,
    st.LastDoneAt,
    st.LastDoneDate,
    st.DoneCount,

    -- Current interval in effect
    CASE WHEN ISNULL(st.IsFirstDone, 0) = 0
         THEN COALESCE(ov.FirstThreshold,  tc.FirstThreshold)
         ELSE COALESCE(ov.RepeatInterval,  tc.RepeatInterval)
    END                                                 AS CurrentInterval,

    -- Base NextDueAt (without extension)
    st.NextDueAt                                        AS BaseNextDueAt,

    -- ACTIVE EXTENSION (if any)
    ext.SNTaskCounterExtensionId,
    ext.ExtensionReasonId,
    er.Code                                             AS ExtensionReasonCode,
    er.Name                                             AS ExtensionReasonName,
    er.BadgeColor                                       AS ExtensionReasonBadge,
    ext.ExtensionType,
    ext.ExtensionValue,
    ext.ComputedExtensionUnits,
    ext.OriginalNextDueAt,
    ext.ExtendedNextDueAt,
    ext.Justification,
    ext.DocReference,
    ext.ApprovedBy,
    ext.ApprovalDate,

    -- Effective NextDueAt: extension wins if active
    ISNULL(ext.ExtendedNextDueAt, st.NextDueAt)         AS EffNextDueAt,

    -- Extension flag
    CASE WHEN ext.SNTaskCounterExtensionId IS NOT NULL
         THEN 1 ELSE 0 END                              AS HasActiveExtension,

    -- Remaining to effective due (dynamic — authoritative for reports)
    ISNULL(ext.ExtendedNextDueAt, st.NextDueAt)
        - ISNULL(st.LifetimeTotal, 0)                   AS RemainingToNextDueCalc,

    -- Stored remaining (fast dashboard)
    st.RemainingToNextDue                               AS RemainingToNextDueStored,

    -- Remaining to ceiling
    COALESCE(ov.Ceiling, tc.Ceiling)
        - ISNULL(st.LifetimeTotal, 0)                   AS RemainingToCeilingCalc,

    -- Alert threshold value
    ISNULL(ext.ExtendedNextDueAt, st.NextDueAt) -
    (CASE WHEN ISNULL(st.IsFirstDone, 0) = 0
          THEN COALESCE(ov.FirstThreshold,  tc.FirstThreshold)
          ELSE COALESCE(ov.RepeatInterval,  tc.RepeatInterval)
     END
     * COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct) / 100)
                                                        AS AlertAtValue,

    -- Pct consumed
    st.PctConsumed,

    -- ── DYNAMICALLY RECOMPUTED STATUS ───────────────────────
    -- Uses ExtendedNextDueAt when extension is active.
    CASE
        -- Ceiling reached → EXPIRED
        WHEN COALESCE(ov.Ceiling, tc.Ceiling) IS NOT NULL
         AND ISNULL(st.LifetimeTotal,0) >=
             COALESCE(ov.Ceiling, tc.Ceiling)
        THEN 'EXPIRED'
        -- One-time task done → COMPLETE
        WHEN ISNULL(st.IsFirstDone,0) = 1
         AND COALESCE(ov.RepeatInterval, tc.RepeatInterval) IS NULL
        THEN 'COMPLETE'
        -- Past effective due → DUE
        WHEN ISNULL(ext.ExtendedNextDueAt, st.NextDueAt) IS NOT NULL
         AND ISNULL(st.LifetimeTotal,0) >=
             ISNULL(ext.ExtendedNextDueAt, st.NextDueAt)
        THEN 'DUE'
        -- In alert zone → ALERT
        WHEN ISNULL(ext.ExtendedNextDueAt, st.NextDueAt) IS NOT NULL
         AND ISNULL(st.LifetimeTotal,0) >=
            ISNULL(ext.ExtendedNextDueAt, st.NextDueAt) -
            (CASE WHEN ISNULL(st.IsFirstDone,0) = 0
                  THEN COALESCE(ov.FirstThreshold,  tc.FirstThreshold)
                  ELSE COALESCE(ov.RepeatInterval,  tc.RepeatInterval)
             END
             * COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
             / 100)
        THEN 'ALERT'
        ELSE 'OK'
    END                                                 AS CounterStatusCalc,

    -- Stored status (fast dashboard)
    st.CounterStatus                                    AS CounterStatusStored,

    -- Display label
    ISNULL(tc.DisplayLabel,
        cd.Code + N' — ' +
        CAST(COALESCE(ov.FirstThreshold,
                      tc.FirstThreshold) AS NVARCHAR)
        + N' ' + ct.DisplayUnit
        + CASE
            WHEN COALESCE(ov.RepeatInterval, tc.RepeatInterval) IS NOT NULL
             AND COALESCE(ov.RepeatInterval, tc.RepeatInterval)
              <> COALESCE(ov.FirstThreshold,  tc.FirstThreshold)
            THEN N' then every '
                + CAST(COALESCE(ov.RepeatInterval,
                                tc.RepeatInterval) AS NVARCHAR)
                + N' ' + ct.DisplayUnit
            ELSE N''
          END
        + CASE
            WHEN COALESCE(ov.Ceiling, tc.Ceiling) IS NULL
            THEN N' (no ceiling)'
            ELSE N' until '
                + CAST(COALESCE(ov.Ceiling, tc.Ceiling) AS NVARCHAR)
                + N' ' + ct.DisplayUnit
          END)                                          AS DisplayLabel,

    st.LastUpdatedDate,
    tc.Notes

FROM mro2.TaskCounter tc

INNER JOIN mro2.PNLimit            pl   ON pl.PNLimitId        = tc.PNLimitId
INNER JOIN mro2.CounterDef         cd   ON cd.CounterDefId     = tc.CounterDefId
INNER JOIN mro2.CounterType        ct   ON ct.CounterTypeId    = cd.CounterTypeId
INNER JOIN mro2.CounterBasis       cb   ON cb.CounterBasisId   = tc.CounterBasisId
INNER JOIN mro2.SerializedItem     si   ON si.PartNumberId     = pl.PartNumberId
INNER JOIN mro2.PartNumber         pn   ON pn.PartNumberId     = pl.PartNumberId

LEFT JOIN mro2.SNTaskCounterState  st   ON st.SerializedItemId = si.SerializedItemId
                                       AND st.TaskCounterId    = tc.TaskCounterId

LEFT JOIN mro2.SNTaskCounterOverride ov ON ov.SerializedItemId = si.SerializedItemId
                                       AND ov.TaskCounterId    = tc.TaskCounterId
                                       AND ov.IsActive         = 1
                                       AND (ov.ExpiryDate IS NULL
                                            OR ov.ExpiryDate >=
                                               CAST(GETDATE() AS DATE))

-- Active extension only
LEFT JOIN mro2.SNTaskCounterExtension ext
                                      ON ext.SerializedItemId  = si.SerializedItemId
                                     AND ext.TaskCounterId     = tc.TaskCounterId
                                     AND ext.IsActive          = 1

LEFT JOIN mro2.ExtensionReason      er   ON er.ExtensionReasonId =
                                            ext.ExtensionReasonId

WHERE tc.IsActive = 1
  AND si.IsActive = 1
  AND pn.IsActive = 1;
GO

-- ============================================================
-- SP: mro2.usp_SNTaskCounterExtension_Grant
--    Creates a new extension for one SN+TaskCounter.
--    Validates:
--      - No active extension already exists (one per cycle)
--      - Extension does not exceed MaxExtensionPct/Value limits
--      - DocReference mandatory when reason requires it
--      - ApprovedBy mandatory when reason requires it
--    Computes ComputedExtensionUnits based on type.
--    Updates SNTaskCounterState.NextDueAt to ExtendedNextDueAt.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounterExtension_Grant','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounterExtension_Grant;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounterExtension_Grant
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @ExtensionReasonId  TINYINT,
    @ExtensionType      VARCHAR(5),     -- 'VALUE' | 'PCT'
    @ExtensionValue     DECIMAL(8,2),   -- units or percent
    @Justification      NVARCHAR(500),
    @DocReference       NVARCHAR(200)   = NULL,
    @ApprovedBy         NVARCHAR(100)   = NULL,
    @ApprovalDate       DATE,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- ── 1. Guard: no active extension already ────────────────
    IF EXISTS (
        SELECT 1 FROM mro2.SNTaskCounterExtension
        WHERE SerializedItemId = @SerializedItemId
          AND TaskCounterId    = @TaskCounterId
          AND IsActive         = 1)
    BEGIN
        RAISERROR(
            'An active extension already exists for this SN/counter. Revoke it before granting a new one.',16, 1);
        RETURN;
    END

    -- ── 2. Get current state and task counter limits ──────────
    DECLARE @CurrentNextDueAt       INT;
    DECLARE @CurrentInterval        INT;
    DECLARE @IsFirstDone            BIT;
    DECLARE @MaxExtPct              TINYINT;
    DECLARE @MaxExtValue            INT;
    DECLARE @EffRepeat              INT;
    DECLARE @EffFirst               INT;
    DECLARE @EffAlertPct            TINYINT;

    SELECT
        @CurrentNextDueAt = st.NextDueAt,
        @IsFirstDone      = st.IsFirstDone,
        @MaxExtPct        = tc.MaxExtensionPct,
        @MaxExtValue      = tc.MaxExtensionValue,
        @EffFirst         = COALESCE(ov.FirstThreshold,    tc.FirstThreshold),
        @EffRepeat        = COALESCE(ov.RepeatInterval,    tc.RepeatInterval),
        @EffAlertPct      = COALESCE(ov.AlertThresholdPct, tc.AlertThresholdPct)
    FROM mro2.TaskCounter tc
    INNER JOIN mro2.SNTaskCounterState st
        ON st.SerializedItemId = @SerializedItemId
       AND st.TaskCounterId    = @TaskCounterId
    LEFT JOIN mro2.SNTaskCounterOverride ov
        ON ov.SerializedItemId = @SerializedItemId
       AND ov.TaskCounterId    = @TaskCounterId
       AND ov.IsActive         = 1
       AND (ov.ExpiryDate IS NULL
            OR ov.ExpiryDate >= CAST(GETDATE() AS DATE))
    WHERE tc.TaskCounterId = @TaskCounterId;

    IF @CurrentNextDueAt IS NULL
    BEGIN
        RAISERROR('No counter state found for this SN/TaskCounter.', 16, 1);
        RETURN;
    END

    SET @CurrentInterval =
        CASE WHEN ISNULL(@IsFirstDone, 0) = 0
             THEN @EffFirst
             ELSE @EffRepeat
        END;

    -- ── 3. Compute extension in units ────────────────────────
    DECLARE @ComputedUnits INT;

    IF @ExtensionType = 'VALUE'
        SET @ComputedUnits = CAST(FLOOR(@ExtensionValue) AS INT);
    ELSE IF @ExtensionType = 'PCT'
        SET @ComputedUnits =
            CAST(FLOOR(@CurrentInterval * @ExtensionValue / 100.0) AS INT);
    ELSE
    BEGIN
        RAISERROR('ExtensionType must be VALUE or PCT.', 16, 1);
        RETURN;
    END

    IF @ComputedUnits <= 0
    BEGIN
        RAISERROR('Computed extension must be greater than zero.', 16, 1);
        RETURN;
    END

    -- ── 4. Validate against pre-approved limits ───────────────
    -- If both limits set: apply whichever is LESS (conservative)
    DECLARE @MaxAllowedUnits INT = NULL;

    IF @MaxExtPct IS NOT NULL
    BEGIN
        DECLARE @PctLimit INT =
            CAST(FLOOR(@CurrentInterval * @MaxExtPct / 100.0) AS INT);
        SET @MaxAllowedUnits = @PctLimit;
    END

    IF @MaxExtValue IS NOT NULL
    BEGIN
        IF @MaxAllowedUnits IS NULL
            SET @MaxAllowedUnits = @MaxExtValue;
        ELSE
            -- Both set: take the lesser
            SET @MaxAllowedUnits =
                CASE WHEN @MaxExtValue < @MaxAllowedUnits
                     THEN @MaxExtValue
                     ELSE @MaxAllowedUnits END;
    END

    IF @MaxAllowedUnits IS NOT NULL AND @ComputedUnits > @MaxAllowedUnits
    BEGIN
        RAISERROR(
            'Extension of %d units exceeds the pre-approved maximum of %d units for this task counter.',
            16, 1, @ComputedUnits, @MaxAllowedUnits);
        RETURN;
    END

    -- ── 5. Validate mandatory fields per reason ───────────────
    DECLARE @RequiresDoc      BIT;
    DECLARE @RequiresApprover BIT;

    SELECT
        @RequiresDoc      = RequiresDocRef,
        @RequiresApprover = RequiresApprover
    FROM mro2.ExtensionReason
    WHERE ExtensionReasonId = @ExtensionReasonId;

    IF @RequiresDoc = 1 AND LTRIM(RTRIM(ISNULL(@DocReference,''))) = ''
    BEGIN
        RAISERROR(
            'A document reference is required for this extension reason.',
            16, 1);
        RETURN;
    END

    IF @RequiresApprover = 1 AND LTRIM(RTRIM(ISNULL(@ApprovedBy,''))) = ''
    BEGIN
        RAISERROR(
            'An approver name is required for this extension reason.',
            16, 1);
        RETURN;
    END

    -- ── 6. Compute extended due point ────────────────────────
    DECLARE @ExtendedNextDueAt INT = @CurrentNextDueAt + @ComputedUnits;

    -- ── 7. Insert extension record ────────────────────────────
    INSERT INTO mro2.SNTaskCounterExtension (
        SerializedItemId,
        TaskCounterId,
        ExtensionReasonId,
        ExtensionType,
        ExtensionValue,
        CurrentIntervalAtExtension,
        ComputedExtensionUnits,
        MaxAllowedPct,
        MaxAllowedValue,
        OriginalNextDueAt,
        ExtendedNextDueAt,
        Justification,
        DocReference,
        ApprovedBy,
        ApprovalDate,
        IsActive,
        CreatedDate,
        CreatedByUserId)
    VALUES (
        @SerializedItemId,
        @TaskCounterId,
        @ExtensionReasonId,
        @ExtensionType,
        @ExtensionValue,
        @CurrentInterval,
        @ComputedUnits,
        @MaxExtPct,
        @MaxExtValue,
        @CurrentNextDueAt,
        @ExtendedNextDueAt,
        @Justification,
        @DocReference,
        @ApprovedBy,
        @ApprovalDate,
        1,
        GETDATE(),
        @UserId);

    -- ── 8. Update SNTaskCounterState.NextDueAt ────────────────
    UPDATE mro2.SNTaskCounterState SET
        NextDueAt           = @ExtendedNextDueAt,
        RemainingToNextDue  = @ExtendedNextDueAt - LifetimeTotal,
        -- Recompute pct consumed against extended interval
        PctConsumed         = CAST(
            AccumulatedSinceLast * 100.0
            / NULLIF(@CurrentInterval + @ComputedUnits, 0)
            AS DECIMAL(5,1)),
        -- Status may improve (was DUE, now OK after extension)
        CounterStatus       =
            CASE
                WHEN LifetimeTotal >= @ExtendedNextDueAt
                THEN 'DUE'
                WHEN LifetimeTotal >=
                     @ExtendedNextDueAt -
                     (@CurrentInterval * @EffAlertPct / 100)
                THEN 'ALERT'
                ELSE 'OK'
            END,
        LastUpdatedDate     = GETDATE(),
        LastUpdatedByUserId = @UserId
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId;

    -- ── 9. Return result ──────────────────────────────────────
    SELECT
        SCOPE_IDENTITY()        AS SNTaskCounterExtensionId,
        @CurrentNextDueAt       AS OriginalNextDueAt,
        @ComputedUnits          AS ComputedExtensionUnits,
        @ExtendedNextDueAt      AS ExtendedNextDueAt,
        @MaxAllowedUnits        AS MaxAllowedUnits;
END
GO

-- ============================================================
-- SP: mro2.usp_SNTaskCounterExtension_Expire
--    Called automatically by RecordAccomplishment.
--    Marks active extension as expired (ACCOMPLISHED).
--    Called BEFORE NextDueAt is advanced by accomplishment.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounterExtension_Expire','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounterExtension_Expire;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounterExtension_Expire
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @ExpiredReason      VARCHAR(20),    -- 'ACCOMPLISHED' | 'REVOKED'
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.SNTaskCounterExtension SET
        IsActive        = 0,
        ExpiredReason   = @ExpiredReason,
        ExpiredDate     = CAST(GETDATE() AS DATE),
        ExpiredByUserId = @UserId
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId
      AND IsActive         = 1;
END
GO

-- ============================================================
-- SP: mro2.usp_SNTaskCounterExtension_Revoke
--    Manual revocation of an active extension.
--    Restores NextDueAt to OriginalNextDueAt.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounterExtension_Revoke','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounterExtension_Revoke;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounterExtension_Revoke
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get original due point before revoking
    DECLARE @OriginalNextDueAt INT;
    SELECT @OriginalNextDueAt = OriginalNextDueAt
    FROM mro2.SNTaskCounterExtension
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId
      AND IsActive         = 1;

    IF @OriginalNextDueAt IS NULL
    BEGIN
        RAISERROR('No active extension found to revoke.', 16, 1);
        RETURN;
    END

    -- Expire the extension
    EXEC mro2.usp_SNTaskCounterExtension_Expire
        @SerializedItemId = @SerializedItemId,
        @TaskCounterId    = @TaskCounterId,
        @ExpiredReason    = 'REVOKED',
        @UserId           = @UserId;

    -- Restore original NextDueAt
    UPDATE mro2.SNTaskCounterState SET
        NextDueAt           = @OriginalNextDueAt,
        RemainingToNextDue  = @OriginalNextDueAt - LifetimeTotal,
        LastUpdatedDate     = GETDATE(),
        LastUpdatedByUserId = @UserId
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId;
END
GO

-- ============================================================
-- SP: mro2.usp_SNTaskCounterExtension_GetHistory
--    Full extension history for a given SN+TaskCounter.
--    Ordered newest first. Used by SN detail page audit tab.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounterExtension_GetHistory','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounterExtension_GetHistory;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounterExtension_GetHistory
    @SerializedItemId   INT,
    @TaskCounterId      INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ext.SNTaskCounterExtensionId,
        ext.SerializedItemId,
        ext.TaskCounterId,
        er.Code                         AS ReasonCode,
        er.Name                         AS ReasonName,
        er.BadgeColor                   AS ReasonBadge,
        ext.ExtensionType,
        ext.ExtensionValue,
        ext.CurrentIntervalAtExtension,
        ext.ComputedExtensionUnits,
        ext.MaxAllowedPct,
        ext.MaxAllowedValue,
        ext.OriginalNextDueAt,
        ext.ExtendedNextDueAt,
        ext.Justification,
        ext.DocReference,
        ext.ApprovedBy,
        ext.ApprovalDate,
        ext.IsActive,
        ext.ExpiredReason,
        ext.ExpiredDate,
        ext.ExpiredByUserId,
        ext.CreatedDate,
        ext.CreatedByUserId
    FROM mro2.SNTaskCounterExtension ext
    INNER JOIN mro2.ExtensionReason er
        ON er.ExtensionReasonId = ext.ExtensionReasonId
    WHERE ext.SerializedItemId = @SerializedItemId
      AND ext.TaskCounterId    = @TaskCounterId
    ORDER BY ext.CreatedDate DESC;
END
GO

-- ============================================================
-- UPDATE: usp_SNTaskCounter_RecordAccomplishment
-- Now expires active extension BEFORE advancing NextDueAt.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNTaskCounter_RecordAccomplishment','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNTaskCounter_RecordAccomplishment;
GO
CREATE PROCEDURE mro2.usp_SNTaskCounter_RecordAccomplishment
    @SerializedItemId   INT,
    @TaskCounterId      INT,
    @AccomplishedAt     INT,
    @AccomplishedDate   DATE,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- ── 1. Expire active extension first (if any) ─────────────
    EXEC mro2.usp_SNTaskCounterExtension_Expire
        @SerializedItemId = @SerializedItemId,
        @TaskCounterId    = @TaskCounterId,
        @ExpiredReason    = 'ACCOMPLISHED',
        @UserId           = @UserId;

    -- ── 2. Resolve effective repeat interval ──────────────────
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

    -- ── 3. Update state ───────────────────────────────────────
    UPDATE mro2.SNTaskCounterState SET
        IsFirstDone          = 1,
        AccumulatedSinceLast = 0,
        LastDoneAt           = @AccomplishedAt,
        LastDoneDate         = @AccomplishedDate,
        DoneCount            = DoneCount + 1,
        NextDueAt            = CASE
                                   WHEN @EffRepeat IS NOT NULL
                                   THEN @AccomplishedAt + @EffRepeat
                                   ELSE NextDueAt
                               END,
        LastUpdatedDate      = GETDATE(),
        LastUpdatedByUserId  = @UserId
    WHERE SerializedItemId = @SerializedItemId
      AND TaskCounterId    = @TaskCounterId;

    -- ── 4. Insert if first accomplishment ─────────────────────
    IF @@ROWCOUNT = 0
    BEGIN
        DECLARE @EffFirst INT;
        DECLARE @EffCeiling INT;
        DECLARE @EffAlertPct TINYINT;
        SELECT
            @EffFirst    = COALESCE(ov2.FirstThreshold,    tc2.FirstThreshold),
            @EffCeiling  = COALESCE(ov2.Ceiling,           tc2.Ceiling),
            @EffAlertPct = COALESCE(ov2.AlertThresholdPct, tc2.AlertThresholdPct)
        FROM mro2.TaskCounter tc2
        LEFT JOIN mro2.SNTaskCounterOverride ov2
               ON ov2.TaskCounterId    = tc2.TaskCounterId
              AND ov2.SerializedItemId = @SerializedItemId
              AND ov2.IsActive         = 1
        WHERE tc2.TaskCounterId = @TaskCounterId;

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

    -- ── 5. Recompute status ───────────────────────────────────
    EXEC mro2.usp_SNTaskCounterState_Update
        @SerializedItemId = @SerializedItemId,
        @TaskCounterId    = @TaskCounterId,
        @NewLifetimeTotal = @AccomplishedAt,
        @ValueSource      = 'MANUAL',
        @UserId           = @UserId;
END
GO

-- ============================================================
-- VERIFICATION
-- ============================================================
/*
-- Tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'mro2'
  AND TABLE_NAME IN ('ExtensionReason','SNTaskCounterExtension')
ORDER BY TABLE_NAME;

-- Extension reason seed data
SELECT Code, Name, RequiresDocRef, RequiresApprover, BadgeColor
FROM mro2.ExtensionReason ORDER BY SortOrder;

-- Filtered unique index
SELECT name, filter_definition
FROM sys.indexes
WHERE object_id = OBJECT_ID('mro2.SNTaskCounterExtension')
  AND name = 'UIX_SNTaskCounterExtension_Active';

-- SPs
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'mro2'
  AND ROUTINE_NAME IN (
      'usp_SNTaskCounterExtension_Grant',
      'usp_SNTaskCounterExtension_Expire',
      'usp_SNTaskCounterExtension_Revoke',
      'usp_SNTaskCounterExtension_GetHistory',
      'usp_SNTaskCounter_RecordAccomplishment')
ORDER BY ROUTINE_NAME;

-- ── SAMPLE USAGE ──────────────────────────────────────────

-- Grant 10% extension (manufacturer tolerance)
-- Pump SN-001, TaskCounterId=1, current due at 300000 min (5000 FH)
-- 10% of 36000 (600 FH interval) = 3600 min (60 FH extension)
-- New due: 303600 min (5060 FH)
/*
EXEC mro2.usp_SNTaskCounterExtension_Grant
    @SerializedItemId  = 1,
    @TaskCounterId     = 1,
    @ExtensionReasonId = 1,   -- MFR_TOL
    @ExtensionType     = 'PCT',
    @ExtensionValue    = 10.0,
    @Justification     = N'CMM 29-10-00 Rev 14 §5.2 allows 10% tolerance',
    @DocReference      = N'CMM 29-10-00 Rev 14',
    @ApprovedBy        = NULL,
    @ApprovalDate      = '2024-11-15',
    @UserId            = 'admin';

-- Grant 50 FH fixed extension (operational necessity)
EXEC mro2.usp_SNTaskCounterExtension_Grant
    @SerializedItemId  = 1,
    @TaskCounterId     = 1,
    @ExtensionReasonId = 3,   -- OPS_NEC
    @ExtensionType     = 'VALUE',
    @ExtensionValue    = 3000.0,   -- 3000 minutes = 50 FH
    @Justification     = N'Aircraft required for urgent mission OPS-2024-112',
    @DocReference      = NULL,
    @ApprovedBy        = N'Cdt BENZEKRI — Chef des Opérations',
    @ApprovalDate      = '2024-11-15',
    @UserId            = 'admin';
*/
*/
