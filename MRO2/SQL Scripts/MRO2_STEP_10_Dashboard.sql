-- ============================================================
-- MRO2 — STEP 10 of 10
-- AircraftConfiguration Dashboard + Due List
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- ============================================================
-- VIEWS CREATED:
--   mro2.vw_AircraftConfiguration   full config per tail with
--                                   counter status per slot
--   mro2.vw_DueList                 all due/alert items across
--                                   all aircraft fleet-wide
-- STORED PROCEDURES: 5
-- PREREQUISITE: Steps 01-09 (all tables + SPs must exist)
-- ============================================================
--
-- DESIGN:
--
--   vw_AircraftConfiguration:
--     One row per position slot per aircraft.
--     Shows: installed SN (if any), PN, position path,
--            worst counter status across all TaskCounters
--            for that SN, days on wing, aircraft FH at install.
--     "Worst status" = EXPIRED > DUE > ALERT > OK > EMPTY
--     EMPTY = position has no SN currently installed.
--     Used by AircraftConfiguration.aspx page.
--
--   vw_DueList:
--     One row per TaskCounter per SN that is ALERT, DUE,
--     or EXPIRED across the entire fleet.
--     Shows both the interval remaining AND all trigger
--     remaining values (smart scheduling — FH and calendar
--     side by side).
--     Used by the MRO2 dashboard and AuditGaps.aspx.
--
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- VIEW 1: mro2.vw_AircraftConfiguration
--    Full slot-level configuration view per tail.
--    Worst counter status computed across all TaskCounters
--    for the installed SN at each slot.
-- ============================================================
IF OBJECT_ID('mro2.vw_AircraftConfiguration','V') IS NOT NULL
    DROP VIEW mro2.vw_AircraftConfiguration;
GO
CREATE VIEW mro2.vw_AircraftConfiguration
AS
-- Worst status rank: higher = worse
-- EXPIRED=4, DUE=3, ALERT=2, OK=1, no SN=0
WITH StatusRank AS (
    SELECT
        st.SerializedItemId,
        tc.PNLimitId,
        pl.PartNumberId,
        -- Numeric rank for worst-status computation
        MAX(CASE st.CounterStatus
                WHEN 'EXPIRED' THEN 4
                WHEN 'DUE'     THEN 3
                WHEN 'ALERT'   THEN 2
                WHEN 'OK'      THEN 1
                ELSE 0
            END)                                AS WorstRank,
        -- Minimum remaining across all counters for this SN
        MIN(st.RemainingToNextDue)              AS MinRemaining,
        COUNT(*)                                AS TotalCounters,
        SUM(CASE WHEN st.CounterStatus IN ('DUE','EXPIRED') THEN 1 ELSE 0 END)
                                                AS OverdueCount,
        SUM(CASE WHEN st.CounterStatus = 'ALERT' THEN 1 ELSE 0 END)
                                                AS AlertCount
    FROM mro2.SNTaskCounterState st
    INNER JOIN mro2.TaskCounter  tc ON tc.TaskCounterId = st.TaskCounterId
    INNER JOIN mro2.PNLimit      pl ON pl.PNLimitId     = tc.PNLimitId
    GROUP BY st.SerializedItemId, tc.PNLimitId, pl.PartNumberId
),
WorstStatus AS (
    SELECT
        SerializedItemId,
        PartNumberId,
        MAX(WorstRank)      AS WorstRank,
        MIN(MinRemaining)   AS MinRemaining,
        SUM(TotalCounters)  AS TotalCounters,
        SUM(OverdueCount)   AS OverdueCount,
        SUM(AlertCount)     AS AlertCount
    FROM StatusRank
    GROUP BY SerializedItemId, PartNumberId
)
SELECT
    -- Position
    pos.AcPositionId,
    pos.AcID,
    pos.TailNo,
    pos.AcMainGroupName,
    pos.AcTypeName,
    pos.PositionCode,
    pos.[Description] PositionDescription,
    pos.ZoneCode,
    pos.ZoneName,
    pos.SystemCode,
    pos.SystemName,
    pos.FullPath,
    pos.ATACode,
    pos.Quantity,
    pos.IsInterchangeable,
    pos.SortOrder,

    -- Installed SN (NULL if empty slot)
    ci.SerializedItemId,
    ci.SerialNumber,
    ci.PartNumberId,
    ci.PN,
    ci.Nomenclature,
    ci.InstallDate,
    ci.AcFH_AtInstall,
    ci.DaysOnWing,
    ci.InstallEventId,
    ci.InstalledByUserId,

    -- Slot occupancy
    CASE WHEN ci.SerializedItemId IS NULL
         THEN 'EMPTY' ELSE 'OCCUPIED' END       AS SlotStatus,

    -- Worst counter status for installed SN
    CASE ws.WorstRank
        WHEN 4 THEN 'EXPIRED'
        WHEN 3 THEN 'DUE'
        WHEN 2 THEN 'ALERT'
        WHEN 1 THEN 'OK'
        ELSE        'NO_DATA'
    END                                         AS WorstCounterStatus,

    -- Counter summary
    ws.MinRemaining,
    ws.TotalCounters,
    ws.OverdueCount,
    ws.AlertCount,

    -- Overall slot health (drives row color in UI)
    -- EMPTY > EXPIRED > DUE > ALERT > NO_DATA > OK
    CASE
        WHEN ci.SerializedItemId IS NULL            THEN 'EMPTY'
        WHEN ws.WorstRank = 4                       THEN 'EXPIRED'
        WHEN ws.WorstRank = 3                       THEN 'DUE'
        WHEN ws.WorstRank = 2                       THEN 'ALERT'
        WHEN ws.WorstRank IS NULL                   THEN 'NO_DATA'
        ELSE                                             'OK'
    END                                         AS SlotHealth,

    -- Allowed PNs for this position (count)
    ISNULL(pnc.PNCount, 0)                      AS AllowedPNCount

FROM mro2.vw_AcPositionTree     pos

-- Current installation (LEFT JOIN — slot may be empty)
LEFT JOIN mro2.vw_CurrentInstallation ci
    ON ci.AcPositionId = pos.AcPositionId

-- Worst status for installed SN
LEFT JOIN WorstStatus ws
    ON ws.SerializedItemId = ci.SerializedItemId

-- Allowed PN count
LEFT JOIN (
    SELECT AcPositionTemplateId, COUNT(*) AS PNCount
    FROM mro2.AcPositionPN WHERE IsActive=1
    GROUP BY AcPositionTemplateId
) pnc ON pnc.AcPositionTemplateId = pos.AcPositionTemplateId

WHERE pos.PositionLevel = 3   -- slots only
  AND pos.IsActive      = 1;
GO
PRINT 'mro2.vw_AircraftConfiguration created.';
GO

-- ============================================================
-- VIEW 2: mro2.vw_DueList
--    Fleet-wide due list — all ALERT/DUE/EXPIRED items.
--    One row per TaskCounter per SN.
--    Includes extension status and smart scheduling data.
-- ============================================================
IF OBJECT_ID('mro2.vw_DueList','V') IS NOT NULL
    DROP VIEW mro2.vw_DueList;
GO
CREATE VIEW mro2.vw_DueList
AS
SELECT
    -- SN and PN
    v.SerializedItemId,
    v.SerialNumber,
    v.PN,
    v.Nomenclature,
    v.PartNumberId,

    -- Counter definition
    v.TaskCounterId,
    v.CounterDefCode,
    v.CounterTypeCode,
    v.DisplayUnit,
    v.UnitStorage,
    v.CounterBasisCode,

    -- Effective limit values
    v.EffFirstThreshold,
    v.EffRepeatInterval,
    v.EffCeiling,
    v.EffAlertPct,
    v.CurrentInterval,

    -- Counter state
    v.IsFirstDone,
    v.AccumulatedSinceLast,
    v.LifetimeTotal,
    v.BaseNextDueAt NextDueAt,
    v.EffNextDueAt,         -- extension-adjusted due point
    v.RemainingToNextDueCalc,
    v.RemainingToCeilingCalc,
    v.AlertAtValue,
    v.PctConsumed,
    v.CounterStatusCalc     AS CounterStatus,

    -- Last accomplishment
    v.LastDoneDate,
    v.DoneCount,

    -- Extension info
    v.HasActiveExtension,
    v.ExtensionType,
    v.ExtensionValue,
    v.ComputedExtensionUnits,
    v.ExtensionReasonCode,
    v.DocReference          AS ExtensionDocRef,
    v.ApprovedBy            AS ExtensionApprovedBy,

    -- Override info
    v.ValueSource,
    v.OverrideReason,

    -- Display label
    v.DisplayLabel,

    -- Where is this SN installed right now?
    ci.AcID,
    ci.TailNo,
    ci.PositionCode,
    ci.FullPath             AS PositionPath,
    ci.InstallDate,
    ci.DaysOnWing

FROM mro2.vw_SNTaskCounterStatus v

-- Current installation location
LEFT JOIN mro2.vw_CurrentInstallation ci
    ON ci.SerializedItemId = v.SerializedItemId

WHERE v.CounterStatusCalc IN ('EXPIRED','DUE','ALERT');
GO
PRINT 'mro2.vw_DueList created.';
GO

-- ============================================================
-- SP: mro2.usp_AircraftConfiguration_Get
--    Returns full slot configuration for one tail.
--    @PositionFilter: NULL=all, 'EMPTY'=empty only,
--                     'ALERT'=problem slots only
-- ============================================================
IF OBJECT_ID('mro2.usp_AircraftConfiguration_Get','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AircraftConfiguration_Get;
GO
CREATE PROCEDURE mro2.usp_AircraftConfiguration_Get
    @AcID           INT,
    @PositionFilter VARCHAR(10) = NULL   -- NULL | 'EMPTY' | 'ALERT'
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_AircraftConfiguration
    WHERE AcID = @AcID
      AND (
          @PositionFilter IS NULL
          OR (@PositionFilter = 'EMPTY'
              AND SlotStatus = 'EMPTY')
          OR (@PositionFilter = 'ALERT'
              AND SlotHealth IN ('EXPIRED','DUE','ALERT','EMPTY'))
      )
    ORDER BY
        ISNULL(ZoneCode,''),
        ISNULL(SystemCode,''),
        SortOrder,
        PositionCode;
END
GO

-- ============================================================
-- SP: mro2.usp_DueList_Get
--    Fleet-wide due list with filters.
--    @AcID        : NULL=all aircraft, set=one tail
--    @StatusFilter: NULL=all non-OK, 'DUE'=only due/expired
-- ============================================================
IF OBJECT_ID('mro2.usp_DueList_Get','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_DueList_Get;
GO
CREATE PROCEDURE mro2.usp_DueList_Get
    @AcID           INT         = NULL,
    @StatusFilter   VARCHAR(10) = NULL   -- NULL | 'DUE' | 'ALERT'
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_DueList
    WHERE (@AcID IS NULL OR AcID = @AcID)
      AND (@StatusFilter IS NULL
           OR (@StatusFilter = 'DUE'
               AND CounterStatus IN ('DUE','EXPIRED'))
           OR (@StatusFilter = 'ALERT'
               AND CounterStatus = 'ALERT'))
    ORDER BY
        -- Worst first
        CASE CounterStatus
            WHEN 'EXPIRED' THEN 0
            WHEN 'DUE'     THEN 1
            WHEN 'ALERT'   THEN 2
            ELSE 3
        END,
        RemainingToNextDueCalc ASC;
END
GO

-- ============================================================
-- SP: mro2.usp_DueList_GetSummary
--    Dashboard summary counts per aircraft.
--    Returns one row per tail with counts of
--    EXPIRED / DUE / ALERT items.
--    Used by MRO2 home dashboard info-boxes.
-- ============================================================
IF OBJECT_ID('mro2.usp_DueList_GetSummary','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_DueList_GetSummary;
GO
CREATE PROCEDURE mro2.usp_DueList_GetSummary
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.AcID,
        d.TailNo,
        COUNT(*)                                            AS TotalItems,
        SUM(CASE WHEN d.CounterStatus='EXPIRED' THEN 1 ELSE 0 END) AS Expired,
        SUM(CASE WHEN d.CounterStatus='DUE'     THEN 1 ELSE 0 END) AS Due,
        SUM(CASE WHEN d.CounterStatus='ALERT'   THEN 1 ELSE 0 END) AS Alert,
        -- Overall aircraft health
        CASE
            WHEN SUM(CASE WHEN d.CounterStatus='EXPIRED' THEN 1 ELSE 0 END) > 0
            THEN 'EXPIRED'
            WHEN SUM(CASE WHEN d.CounterStatus='DUE'     THEN 1 ELSE 0 END) > 0
            THEN 'DUE'
            WHEN SUM(CASE WHEN d.CounterStatus='ALERT'   THEN 1 ELSE 0 END) > 0
            THEN 'ALERT'
            ELSE 'OK'
        END                                                 AS AircraftHealth
    FROM mro2.vw_DueList d
    WHERE d.AcID IS NOT NULL
    GROUP BY d.AcID, d.TailNo
    ORDER BY
        CASE
            WHEN SUM(CASE WHEN d.CounterStatus='EXPIRED' THEN 1 ELSE 0 END) > 0 THEN 0
            WHEN SUM(CASE WHEN d.CounterStatus='DUE'     THEN 1 ELSE 0 END) > 0 THEN 1
            WHEN SUM(CASE WHEN d.CounterStatus='ALERT'   THEN 1 ELSE 0 END) > 0 THEN 2
            ELSE 3
        END,
        d.TailNo;
END
GO

-- ============================================================
-- SP: mro2.usp_FleetHealth_Get
--    Single-row fleet-wide health summary.
--    Used by MRO2 master dashboard top strip.
-- ============================================================
IF OBJECT_ID('mro2.usp_FleetHealth_Get','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_FleetHealth_Get;
GO
CREATE PROCEDURE mro2.usp_FleetHealth_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(DISTINCT SerializedItemId)                    AS TotalSNsTracked,
        COUNT(*)                                            AS TotalCounterLines,
        SUM(CASE WHEN CounterStatus='EXPIRED' THEN 1 ELSE 0 END) AS Expired,
        SUM(CASE WHEN CounterStatus='DUE'     THEN 1 ELSE 0 END) AS Due,
        SUM(CASE WHEN CounterStatus='ALERT'   THEN 1 ELSE 0 END) AS Alert,
        SUM(CASE WHEN HasActiveExtension=1    THEN 1 ELSE 0 END) AS WithExtension
    FROM mro2.vw_DueList;
END
GO

-- ============================================================
-- SP: mro2.usp_SNDetail_Get
--    Complete detail for one SN:
--      - PN and SN info
--      - Current installation location
--      - All TaskCounter lines with full status
--      - All active extensions
--      - Full event history
--    Used by SN detail page.
-- ============================================================
IF OBJECT_ID('mro2.usp_SNDetail_Get','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_SNDetail_Get;
GO
CREATE PROCEDURE mro2.usp_SNDetail_Get
    @SerializedItemId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: SN + PN info
    SELECT
        si.SerializedItemId,
        si.SerialNumber,
        si.StatusCode,
        si.ManufacturedDate,
        si.ReceivedDate,
        si.Notes,
        pn.PartNumberId,
        pn.PN,
        pn.Nomenclature,
        ata.ATACode,
        uom.Code    AS UOMCode,
        pn.IsSerialized
    FROM mro2.SerializedItem si
    INNER JOIN mro2.PartNumber   pn  ON pn.PartNumberId    = si.PartNumberId
    LEFT  JOIN mro2.ATA          ata ON ata.ATAId          = pn.ATAId
    LEFT  JOIN mro2.UnitOfMeasure uom ON uom.UnitOfMeasureId = pn.UnitOfMeasureId
    WHERE si.SerializedItemId = @SerializedItemId;

    -- Result set 2: current installation
    SELECT *
    FROM mro2.vw_CurrentInstallation
    WHERE SerializedItemId = @SerializedItemId;

    -- Result set 3: all TaskCounter lines with status
    SELECT *
    FROM mro2.vw_SNTaskCounterStatus
    WHERE SerializedItemId = @SerializedItemId
    ORDER BY CounterTypeId, TaskCounterId;

    -- Result set 4: active extensions
    SELECT
        ext.SNTaskCounterExtensionId,
        ext.TaskCounterId,
        tc_cd.Code          AS CounterDefCode,
        er.Code             AS ReasonCode,
        er.Name             AS ReasonName,
        er.BadgeColor,
        ext.ExtensionType,
        ext.ExtensionValue,
        ext.ComputedExtensionUnits,
        ext.OriginalNextDueAt,
        ext.ExtendedNextDueAt,
        ext.Justification,
        ext.DocReference,
        ext.ApprovedBy,
        ext.ApprovalDate,
        ext.IsActive
    FROM mro2.SNTaskCounterExtension ext
    INNER JOIN mro2.TaskCounter     tc  ON tc.TaskCounterId    = ext.TaskCounterId
    INNER JOIN mro2.CounterDef      tc_cd ON tc_cd.CounterDefId = tc.CounterDefId
    INNER JOIN mro2.ExtensionReason er  ON er.ExtensionReasonId= ext.ExtensionReasonId
    WHERE ext.SerializedItemId = @SerializedItemId
      AND ext.IsActive         = 1;

    -- Result set 5: event history (last 20)
    SELECT TOP 20 *
    FROM mro2.vw_SNHistory
    WHERE SerializedItemId = @SerializedItemId
    ORDER BY EventDate DESC, RecordEventId DESC;
END
GO

-- ============================================================
-- STEP 10 VERIFICATION
-- ============================================================
/*
-- Views (expect 2 new + all previous)
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA='mro2'
ORDER BY TABLE_NAME;

-- SPs (expect 5 new)
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA='mro2'
  AND ROUTINE_NAME IN (
    'usp_AircraftConfiguration_Get',
    'usp_DueList_Get',
    'usp_DueList_GetSummary',
    'usp_FleetHealth_Get',
    'usp_SNDetail_Get')
ORDER BY ROUTINE_NAME;

-- ── COMPLETE SCHEMA FINAL VERIFICATION ────────────────────
-- All mro2 tables with row counts
SELECT t.name AS TableName, p.rows AS RowCount
FROM sys.tables t
INNER JOIN sys.schemas    s ON s.schema_id = t.schema_id
INNER JOIN sys.partitions p ON p.object_id = t.object_id
                            AND p.index_id IN (0,1)
WHERE s.name = 'mro2'
ORDER BY t.name;

-- All mro2 SPs (expect ~50)
SELECT COUNT(*) AS TotalSPs
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA='mro2' AND ROUTINE_TYPE='PROCEDURE';

-- All mro2 views (expect 8)
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA='mro2';

-- ── OPERATIONAL TEST (end-to-end) ─────────────────────────
-- After adding template + copying to tail + installing SNs:

-- 1. View aircraft configuration
-- EXEC mro2.usp_AircraftConfiguration_Get @AcID=5;

-- 2. Update aircraft FH (simulates sortie close)
-- EXEC mro2.usp_TechLog_Feed
--     @AcID=5, @CounterDefId=2, @NewValue=258000,
--     @UpdateSource='AUTO', @SortieRef='SRT-001', @UserId='system';

-- 3. Check fleet due list
-- EXEC mro2.usp_DueList_Get;

-- 4. Fleet health summary
-- EXEC mro2.usp_FleetHealth_Get;

-- 5. Dashboard summary per aircraft
-- EXEC mro2.usp_DueList_GetSummary;

-- 6. Full SN detail
-- EXEC mro2.usp_SNDetail_Get @SerializedItemId=1;
*/

PRINT '── Step 10 complete ─────────────────────────────────────';
PRINT '── ALL 10 STEPS COMPLETE — MRO2 DB SCHEMA FULLY BUILT ──';
