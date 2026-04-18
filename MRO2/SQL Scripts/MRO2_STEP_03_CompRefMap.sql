-- ============================================================
-- MRO2 — STEP 03 of 10
-- ComputationReference + LimitTypeReferenceMap
-- ============================================================
-- TABLES CREATED:
--   mro2.ComputationReference     (8 rows seeded)
--   mro2.LimitTypeReferenceMap    (11 rows seeded — map)
-- STORED PROCEDURES: 6
-- PREREQUISITE: Step 02 (mro2.LimitType must exist)
-- ============================================================
-- DESIGN:
--   ComputationReference = "since when" events
--     (SNEW, SOH, install date, manufacture date, cure date...)
--     Answers: from what event does the counter interval start?
--
--   CounterReference (Step 02) = "which document" authorises
--     (AMM, CMM, SB, AD...) — these are separate concerns.
--
--   LimitTypeReferenceMap = which ComputationReferences are
--     valid for each LimitType, and which is the default.
--     Drives filtered DDL in PNLimit UI modal.
--     OR logic: first trigger reached = task due.
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- TABLE 6: mro2.ComputationReference
-- ============================================================
IF OBJECT_ID('mro2.ComputationReference','U') IS NULL
BEGIN
    CREATE TABLE mro2.ComputationReference (
        ComputationReferenceId  TINYINT      NOT NULL IDENTITY(1,1),
        Code                    VARCHAR(30)  NOT NULL,
        Name                    NVARCHAR(150)NOT NULL,
        Description             NVARCHAR(300)NULL,
        SortOrder               TINYINT      NOT NULL
            CONSTRAINT DF_CompRef_SortOrder   DEFAULT (99),
        IsActive                BIT          NOT NULL
            CONSTRAINT DF_CompRef_IsActive    DEFAULT (1),
        CreatedDate             DATETIME     NOT NULL
            CONSTRAINT DF_CompRef_CreatedDate DEFAULT (GETDATE()),

        CONSTRAINT PK_ComputationReference      PRIMARY KEY (ComputationReferenceId),
        CONSTRAINT UQ_ComputationReference_Code UNIQUE (Code)
    );
    PRINT 'mro2.ComputationReference created.';

    INSERT INTO mro2.ComputationReference (Code, Name, Description, SortOrder)
    VALUES
        ('ABSOLUTE',
         'Absolute / Total',
         'Running lifetime total. Never resets. Informational tracking only.',
         1),
        ('SINCE_NEW',
         'Since New',
         'Counter from date of manufacture or return-to-new overhaul. '
         + 'Hard-time life limits (EASA/FAA mandatory).',
         2),
        ('SINCE_OH',
         'Since Last Overhaul',
         'Resets at each overhaul shop visit. TBO and HSI intervals.',
         3),
        ('SINCE_INSTALL',
         'Since Last Installation',
         'Resets each time the component is installed on an aircraft. '
         + 'Typical for periodic inspection intervals.',
         4),
        ('EXECUTION_DATE',
         'Execution Date',
         'Date the last accomplishment was recorded in the logbook. '
         + 'Calendar-based inspection intervals.',
         5),
        ('INSTALL_DATE',
         'Install Date',
         'Date component was installed on its parent aircraft. '
         + 'Calendar limits from installation.',
         6),
        ('MANUFACTURE_DATE',
         'Manufacture Date',
         'Date of manufacture stamped on the component. '
         + 'Shelf life and calendar limits from production.',
         7),
        ('CURE_DATE',
         'Cure Date',
         'Cure/mix date for elastomers, O-rings, seals, '
         + 'pyrotechnics and perishable materials.',
         8);
    PRINT '  → 8 rows seeded.';
END
ELSE
    PRINT 'mro2.ComputationReference already exists — skipped.';
GO

-- ============================================================
-- TABLE 7: mro2.LimitTypeReferenceMap
-- Maps LimitType → valid ComputationReferences + default flag.
-- IsDefault=1: auto-selected in DDL when LimitType is chosen.
-- One default per LimitType enforced by filtered unique index.
-- ============================================================
IF OBJECT_ID('mro2.LimitTypeReferenceMap','U') IS NULL
BEGIN
    CREATE TABLE mro2.LimitTypeReferenceMap (
        LimitTypeReferenceMapId INT     NOT NULL IDENTITY(1,1),
        LimitTypeId             TINYINT NOT NULL,
        ComputationReferenceId  TINYINT NOT NULL,
        IsDefault               BIT     NOT NULL
            CONSTRAINT DF_LTRefMap_IsDefault DEFAULT (0),
        IsActive                BIT     NOT NULL
            CONSTRAINT DF_LTRefMap_IsActive  DEFAULT (1),

        CONSTRAINT PK_LimitTypeReferenceMap  PRIMARY KEY (LimitTypeReferenceMapId),
        CONSTRAINT UQ_LimitTypeRefMap_Pair   UNIQUE (LimitTypeId, ComputationReferenceId),
        CONSTRAINT FK_LTRefMap_LimitType
            FOREIGN KEY (LimitTypeId) REFERENCES mro2.LimitType (LimitTypeId),
        CONSTRAINT FK_LTRefMap_CompRef
            FOREIGN KEY (ComputationReferenceId)
            REFERENCES mro2.ComputationReference (ComputationReferenceId)
    );
    PRINT 'mro2.LimitTypeReferenceMap created.';

    -- Standard MRO mapping seeded
    INSERT INTO mro2.LimitTypeReferenceMap
        (LimitTypeId, ComputationReferenceId, IsDefault)
    SELECT lt.LimitTypeId, cr.ComputationReferenceId, v.IsDefault
    FROM (VALUES
        -- LIFE: default SINCE_NEW, also SINCE_OH, ABSOLUTE
        ('LIFE',        'SINCE_NEW',        1),
        ('LIFE',        'SINCE_OH',         0),
        ('LIFE',        'ABSOLUTE',         0),
        -- INSPECTION: default SINCE_INSTALL, also EXECUTION_DATE, SINCE_OH
        ('INSPECTION',  'SINCE_INSTALL',    1),
        ('INSPECTION',  'EXECUTION_DATE',   0),
        ('INSPECTION',  'SINCE_OH',         0),
        -- FUNCTIONAL: default SINCE_INSTALL, also ABSOLUTE, EXECUTION_DATE
        ('FUNCTIONAL',  'SINCE_INSTALL',    1),
        ('FUNCTIONAL',  'ABSOLUTE',         0),
        ('FUNCTIONAL',  'EXECUTION_DATE',   0),
        -- SHELF_LIFE: default CURE_DATE, also MANUFACTURE_DATE
        ('SHELF_LIFE',  'CURE_DATE',        1),
        ('SHELF_LIFE',  'MANUFACTURE_DATE', 0)
    ) AS v(LimitTypeCode, CompRefCode, IsDefault)
    INNER JOIN mro2.LimitType lt
        ON lt.Code = v.LimitTypeCode
    INNER JOIN mro2.ComputationReference cr
        ON cr.Code = v.CompRefCode;
    PRINT '  → 11 rows seeded.';
END
ELSE
    PRINT 'mro2.LimitTypeReferenceMap already exists — skipped.';
GO

-- ============================================================
-- STORED PROCEDURES — ComputationReference
-- ============================================================
IF OBJECT_ID('mro2.usp_ComputationReference_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_ComputationReference_List;
GO
CREATE PROCEDURE mro2.usp_ComputationReference_List
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ComputationReferenceId, Code, Name,
           Description, SortOrder, IsActive, CreatedDate
    FROM mro2.ComputationReference
    WHERE (@IncludeInactive=1 OR IsActive=1)
    ORDER BY SortOrder;
END
GO

IF OBJECT_ID('mro2.usp_ComputationReference_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_ComputationReference_Save;
GO
CREATE PROCEDURE mro2.usp_ComputationReference_Save
    @ComputationReferenceId TINYINT       = NULL,
    @Code                   VARCHAR(30),
    @Name                   NVARCHAR(150),
    @Description            NVARCHAR(300) = NULL,
    @SortOrder              TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Code = UPPER(LTRIM(RTRIM(@Code)));
    IF @ComputationReferenceId IS NULL
    BEGIN
        INSERT INTO mro2.ComputationReference (Code,Name,Description,SortOrder)
        VALUES (@Code,@Name,@Description,@SortOrder);
        SELECT SCOPE_IDENTITY() AS ComputationReferenceId;
    END
    ELSE
    BEGIN
        UPDATE mro2.ComputationReference
        SET Code=@Code, Name=@Name, Description=@Description, SortOrder=@SortOrder
        WHERE ComputationReferenceId=@ComputationReferenceId;
        SELECT @ComputationReferenceId AS ComputationReferenceId;
    END
END
GO

IF OBJECT_ID('mro2.usp_ComputationReference_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_ComputationReference_SetActive;
GO
CREATE PROCEDURE mro2.usp_ComputationReference_SetActive
    @ComputationReferenceId TINYINT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.ComputationReference SET IsActive=@IsActive
    WHERE ComputationReferenceId=@ComputationReferenceId;
END
GO

-- ============================================================
-- STORED PROCEDURES — LimitTypeReferenceMap
-- ============================================================

-- Used by PNLimit modal: filtered CompRef DDL per LimitType
-- Returns active entries, default first (IsDefault DESC)
IF OBJECT_ID('mro2.usp_LimitTypeReferenceMap_GetByLimitType','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_LimitTypeReferenceMap_GetByLimitType;
GO
CREATE PROCEDURE mro2.usp_LimitTypeReferenceMap_GetByLimitType
    @LimitTypeId TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.LimitTypeReferenceMapId, m.LimitTypeId,
           m.ComputationReferenceId,
           cr.Code  AS ComputationReferenceCode,
           cr.Name  AS ComputationReferenceName,
           cr.Description,
           m.IsDefault
    FROM mro2.LimitTypeReferenceMap m
    INNER JOIN mro2.ComputationReference cr
        ON cr.ComputationReferenceId = m.ComputationReferenceId
       AND cr.IsActive = 1
    WHERE m.LimitTypeId = @LimitTypeId
      AND m.IsActive    = 1
    ORDER BY m.IsDefault DESC, cr.SortOrder;
END
GO

-- Admin view: all mappings
IF OBJECT_ID('mro2.usp_LimitTypeReferenceMap_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_LimitTypeReferenceMap_List;
GO
CREATE PROCEDURE mro2.usp_LimitTypeReferenceMap_List
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.LimitTypeReferenceMapId,
           lt.LimitTypeId, lt.Code AS LimitTypeCode, lt.Name AS LimitTypeName,
           lt.BadgeColor,
           cr.ComputationReferenceId,
           cr.Code AS CompRefCode, cr.Name AS CompRefName,
           m.IsDefault, m.IsActive
    FROM mro2.LimitTypeReferenceMap m
    INNER JOIN mro2.LimitType            lt ON lt.LimitTypeId            = m.LimitTypeId
    INNER JOIN mro2.ComputationReference cr ON cr.ComputationReferenceId = m.ComputationReferenceId
    ORDER BY lt.SortOrder, m.IsDefault DESC, cr.SortOrder;
END
GO

IF OBJECT_ID('mro2.usp_LimitTypeReferenceMap_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_LimitTypeReferenceMap_Save;
GO
CREATE PROCEDURE mro2.usp_LimitTypeReferenceMap_Save
    @LimitTypeReferenceMapId INT     = NULL,
    @LimitTypeId             TINYINT,
    @ComputationReferenceId  TINYINT,
    @IsDefault               BIT
AS
BEGIN
    SET NOCOUNT ON;
    -- Clear existing default for this LimitType if setting new default
    IF @IsDefault = 1
        UPDATE mro2.LimitTypeReferenceMap SET IsDefault=0
        WHERE LimitTypeId=@LimitTypeId
          AND (@LimitTypeReferenceMapId IS NULL
               OR LimitTypeReferenceMapId <> @LimitTypeReferenceMapId);

    IF @LimitTypeReferenceMapId IS NULL
    BEGIN
        INSERT INTO mro2.LimitTypeReferenceMap
            (LimitTypeId,ComputationReferenceId,IsDefault)
        VALUES (@LimitTypeId,@ComputationReferenceId,@IsDefault);
        SELECT SCOPE_IDENTITY() AS LimitTypeReferenceMapId;
    END
    ELSE
    BEGIN
        UPDATE mro2.LimitTypeReferenceMap
        SET LimitTypeId=@LimitTypeId,
            ComputationReferenceId=@ComputationReferenceId,
            IsDefault=@IsDefault
        WHERE LimitTypeReferenceMapId=@LimitTypeReferenceMapId;
        SELECT @LimitTypeReferenceMapId AS LimitTypeReferenceMapId;
    END
END
GO

IF OBJECT_ID('mro2.usp_LimitTypeReferenceMap_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_LimitTypeReferenceMap_SetActive;
GO
CREATE PROCEDURE mro2.usp_LimitTypeReferenceMap_SetActive
    @LimitTypeReferenceMapId INT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.LimitTypeReferenceMap SET IsActive=@IsActive
    WHERE LimitTypeReferenceMapId=@LimitTypeReferenceMapId;
END
GO

-- ============================================================
-- STEP 03 VERIFICATION
-- Expected: 8 / 11
-- ============================================================
/*
SELECT 'ComputationReference'  AS [Table], COUNT(*) AS Rows
FROM mro2.ComputationReference
UNION ALL
SELECT 'LimitTypeReferenceMap', COUNT(*) FROM mro2.LimitTypeReferenceMap;

-- Full map with defaults
SELECT lt.Code AS LimitType, cr.Code AS CompRef, m.IsDefault
FROM mro2.LimitTypeReferenceMap m
INNER JOIN mro2.LimitType            lt ON lt.LimitTypeId            = m.LimitTypeId
INNER JOIN mro2.ComputationReference cr ON cr.ComputationReferenceId = m.ComputationReferenceId
ORDER BY lt.SortOrder, m.IsDefault DESC, cr.SortOrder;

-- Test SP (LimitType 1 = LIFE)
EXEC mro2.usp_LimitTypeReferenceMap_GetByLimitType @LimitTypeId = 1;
*/
PRINT '── Step 03 complete ─────────────────────────────────────';
