-- ============================================================
-- MRO2 — STEP 02 of 10
-- Lookup Tables (pure seed data, no FKs to component tables)
-- ============================================================
-- TABLES CREATED:
--   mro2.CounterType        (10 rows seeded)
--   mro2.CounterDef         (10 rows seeded)
--   mro2.CounterBasis       (4 rows seeded)
--   mro2.LimitType          (4 rows seeded)
--   mro2.CounterReference   (14 rows seeded)
-- STORED PROCEDURES: 15 (List/Save/SetActive for each table)
-- PREREQUISITE: Step 01
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- TABLE 1: mro2.CounterType
-- Measurement dimension. UnitStorage drives INT storage format.
--   MINUTES : time-based (FH, APU hrs, engine hrs)
--             stored as integer minutes → 1000 FH = 60000 min
--   COUNT   : integer (cycles, landings, starts, days)
-- ============================================================
IF OBJECT_ID('mro2.CounterType','U') IS NULL
BEGIN
    CREATE TABLE mro2.CounterType (
        CounterTypeId   SMALLINT     NOT NULL IDENTITY(1,1),
        Code            VARCHAR(20)  NOT NULL,
        Name            NVARCHAR(100)NOT NULL,
        UnitStorage     VARCHAR(10)  NOT NULL
            CONSTRAINT DF_CounterType_UnitStorage DEFAULT ('COUNT'),
        DisplayUnit     VARCHAR(20)  NOT NULL,
        SortOrder       TINYINT      NOT NULL
            CONSTRAINT DF_CounterType_SortOrder   DEFAULT (99),
        IsActive        BIT          NOT NULL
            CONSTRAINT DF_CounterType_IsActive    DEFAULT (1),
        CreatedDate     DATETIME     NOT NULL
            CONSTRAINT DF_CounterType_CreatedDate DEFAULT (GETDATE()),

        CONSTRAINT PK_CounterType          PRIMARY KEY (CounterTypeId),
        CONSTRAINT UQ_CounterType_Code     UNIQUE (Code),
        CONSTRAINT CK_CounterType_UnitStorage
            CHECK (UnitStorage IN ('MINUTES','COUNT'))
    );
    PRINT 'mro2.CounterType created.';

    INSERT INTO mro2.CounterType (Code, Name, UnitStorage, DisplayUnit, SortOrder)
    VALUES
        ('FLIGHT_HOURS',  'Aircraft Flight Hours',    'MINUTES', 'hrs',    1),
        ('BLOCK_HOURS',   'Aircraft Block Hours',     'MINUTES', 'hrs',    2),
        ('APU_HOURS',     'APU Hours',                'MINUTES', 'hrs',    3),
        ('ENGINE_HOURS',  'Engine Hours',             'MINUTES', 'hrs',    4),
        ('FLIGHT_CYCLES', 'Flight Cycles',            'COUNT',   'cycles', 5),
        ('LANDINGS',      'Full Stop Landings',       'COUNT',   'ldg',    6),
        ('TGO',           'Touch-and-Go Landings',    'COUNT',   'ldg',    7),
        ('APU_CYCLES',    'APU Cycles',               'COUNT',   'cycles', 8),
        ('ENGINE_STARTS', 'Engine Starts',            'COUNT',   'starts', 9),
        ('CALENDAR_DAYS', 'Calendar Days',            'COUNT',   'days',   10);
    PRINT '  → 10 rows seeded.';
END
ELSE
    PRINT 'mro2.CounterType already exists — skipped.';
GO

-- ============================================================
-- TABLE 2: mro2.CounterDef
-- Specific counter within a CounterType.
-- AppliesToAssetKindCode:
--   AIRCRAFT  : driven by aircraft logbook (FH, FC, landings)
--   COMPONENT : tracked per SN (APU hrs, engine hrs, starts)
-- UnitStorage is inherited from CounterType on save (via SP).
-- ============================================================
IF OBJECT_ID('mro2.CounterDef','U') IS NULL
BEGIN
    CREATE TABLE mro2.CounterDef (
        CounterDefId            INT          NOT NULL IDENTITY(1,1),
        CounterTypeId           SMALLINT     NOT NULL,
        Code                    VARCHAR(30)  NOT NULL,
        Name                    NVARCHAR(150)NOT NULL,
        AppliesToAssetKindCode  VARCHAR(20)  NOT NULL
            CONSTRAINT DF_CounterDef_AssetKind    DEFAULT ('AIRCRAFT'),
        UnitStorage             VARCHAR(10)  NOT NULL
            CONSTRAINT DF_CounterDef_UnitStorage  DEFAULT ('COUNT'),
        SortOrder               TINYINT      NOT NULL
            CONSTRAINT DF_CounterDef_SortOrder    DEFAULT (99),
        IsActive                BIT          NOT NULL
            CONSTRAINT DF_CounterDef_IsActive     DEFAULT (1),
        CreatedDate             DATETIME     NOT NULL
            CONSTRAINT DF_CounterDef_CreatedDate  DEFAULT (GETDATE()),

        CONSTRAINT PK_CounterDef           PRIMARY KEY (CounterDefId),
        CONSTRAINT UQ_CounterDef_Type_Code UNIQUE (CounterTypeId, Code),
        CONSTRAINT FK_CounterDef_CounterType
            FOREIGN KEY (CounterTypeId) REFERENCES mro2.CounterType (CounterTypeId),
        CONSTRAINT CK_CounterDef_AssetKind
            CHECK (AppliesToAssetKindCode IN ('AIRCRAFT','COMPONENT')),
        CONSTRAINT CK_CounterDef_UnitStorage
            CHECK (UnitStorage IN ('MINUTES','COUNT'))
    );
    PRINT 'mro2.CounterDef created.';

    INSERT INTO mro2.CounterDef
        (CounterTypeId, Code, Name, AppliesToAssetKindCode, UnitStorage, SortOrder)
    SELECT ct.CounterTypeId, v.Code, v.Name, v.AssetKind, ct.UnitStorage, v.SortOrder
    FROM (VALUES
        -- AIRCRAFT counters
        ('BLOCK_HOURS',   'AF_BLOCK_OFF_MIN', 'Aircraft Block Time (minutes)', 'AIRCRAFT',  1),
        ('FLIGHT_HOURS',  'AF_FLIGHT_MIN',    'Aircraft Flight Time (minutes)','AIRCRAFT',  2),
        ('LANDINGS',      'AF_LANDINGS',      'Aircraft Landings',             'AIRCRAFT',  3),
        ('TGO',           'AF_TOUCH_AND_GO',  'Touch-and-Go',                  'AIRCRAFT',  4),
        ('FLIGHT_CYCLES', 'AF_CYCLES',        'Aircraft Flight Cycles',        'AIRCRAFT',  5),
        -- COMPONENT counters
        ('APU_CYCLES',    'APU_CYCLES',       'APU Cycles',                    'COMPONENT', 1),
        ('APU_HOURS',     'APU_HOURS_MIN',    'APU Hours (minutes)',            'COMPONENT', 2),
        ('ENGINE_HOURS',  'ENG_HOURS_MIN',    'Engine Hours (minutes)',         'COMPONENT', 3),
        ('ENGINE_STARTS', 'ENG_STARTS',       'Engine Starts',                 'COMPONENT', 4),
        -- Calendar
        ('CALENDAR_DAYS', 'CAL_DAYS',         'Calendar Days',                 'AIRCRAFT',  1)
    ) AS v(TypeCode, Code, Name, AssetKind, SortOrder)
    INNER JOIN mro2.CounterType ct ON ct.Code = v.TypeCode;
    PRINT '  → 10 rows seeded.';
END
ELSE
    PRINT 'mro2.CounterDef already exists — skipped.';
GO

-- Indexes on CounterDef
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_CounterDef_CounterTypeId'
    AND object_id=OBJECT_ID('mro2.CounterDef'))
    CREATE INDEX IX_CounterDef_CounterTypeId
        ON mro2.CounterDef (CounterTypeId)
        INCLUDE (Code, Name, AppliesToAssetKindCode, UnitStorage);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name='IX_CounterDef_AssetKind'
    AND object_id=OBJECT_ID('mro2.CounterDef'))
    CREATE INDEX IX_CounterDef_AssetKind
        ON mro2.CounterDef (AppliesToAssetKindCode)
        INCLUDE (CounterTypeId, Code, Name);
GO

-- ============================================================
-- TABLE 3: mro2.CounterBasis
-- Reference point from which a counter accumulates.
--   ABSOLUTE     : lifetime total, never resets
--   SINCE_INSTALL: resets on each installation on aircraft
--   SINCE_NEW    : resets only on return-to-new overhaul
--   SINCE_OH     : resets on each overhaul shop visit
-- ============================================================
IF OBJECT_ID('mro2.CounterBasis','U') IS NULL
BEGIN
    CREATE TABLE mro2.CounterBasis (
        CounterBasisId  TINYINT      NOT NULL IDENTITY(1,1),
        Code            VARCHAR(20)  NOT NULL,
        Name            NVARCHAR(100)NOT NULL,
        Description     NVARCHAR(300)NULL,
        SortOrder       TINYINT      NOT NULL
            CONSTRAINT DF_CounterBasis_SortOrder DEFAULT (99),
        IsActive        BIT          NOT NULL
            CONSTRAINT DF_CounterBasis_IsActive  DEFAULT (1),

        CONSTRAINT PK_CounterBasis      PRIMARY KEY (CounterBasisId),
        CONSTRAINT UQ_CounterBasis_Code UNIQUE (Code)
    );
    PRINT 'mro2.CounterBasis created.';

    INSERT INTO mro2.CounterBasis (Code, Name, Description, SortOrder)
    VALUES
        ('ABSOLUTE',
         'Absolute / Total',
         'Lifetime running total. Never resets. Informational tracking.',
         1),
        ('SINCE_INSTALL',
         'Since Install',
         'Resets each time the component is installed on an aircraft. '
         + 'Typical for periodic inspection intervals.',
         2),
        ('SINCE_NEW',
         'Since New',
         'Resets only when returned to new standard (full overhaul). '
         + 'Used for hard-time life limits (EASA/FAA).',
         3),
        ('SINCE_OH',
         'Since Overhaul',
         'Resets at each overhaul shop visit. '
         + 'Used for TBO and HSI intervals.',
         4);
    PRINT '  → 4 rows seeded.';
END
ELSE
    PRINT 'mro2.CounterBasis already exists — skipped.';
GO

-- ============================================================
-- TABLE 4: mro2.LimitType
-- Classifies the airworthiness limit kind.
-- Standalone for now — linked to PNLimit in later sprint.
--   LIFE       : hard removal limit, cannot be deferred
--   INSPECTION : periodic check, component continues in service
--   FUNCTIONAL : performance-driven limit
--   SHELF_LIFE : calendar storage limit (elastomers, fluids)
-- ============================================================
IF OBJECT_ID('mro2.LimitType','U') IS NULL
BEGIN
    CREATE TABLE mro2.LimitType (
        LimitTypeId     TINYINT      NOT NULL IDENTITY(1,1),
        Code            VARCHAR(20)  NOT NULL,
        Name            NVARCHAR(100)NOT NULL,
        Description     NVARCHAR(300)NULL,
        BadgeColor      VARCHAR(20)  NOT NULL
            CONSTRAINT DF_LimitType_BadgeColor DEFAULT ('secondary'),
        SortOrder       TINYINT      NOT NULL
            CONSTRAINT DF_LimitType_SortOrder  DEFAULT (99),
        IsActive        BIT          NOT NULL
            CONSTRAINT DF_LimitType_IsActive   DEFAULT (1),

        CONSTRAINT PK_LimitType      PRIMARY KEY (LimitTypeId),
        CONSTRAINT UQ_LimitType_Code UNIQUE (Code)
    );
    PRINT 'mro2.LimitType created.';

    INSERT INTO mro2.LimitType (Code, Name, Description, BadgeColor, SortOrder)
    VALUES
        ('LIFE',
         'Life Limit',
         'Hard-time removal limit. Cannot be deferred. EASA/FAA mandatory.',
         'danger', 1),
        ('INSPECTION',
         'Inspection / Check',
         'Periodic inspection interval. Component continues in service after accomplishment.',
         'warning', 2),
        ('FUNCTIONAL',
         'Functional Limit',
         'Limit driven by component performance. May be extended by engineering approval.',
         'info', 3),
        ('SHELF_LIFE',
         'Shelf Life',
         'Calendar storage limit from manufacture or cure date. '
         + 'Elastomers, fluids, pyrotechnics, batteries.',
         'secondary', 4);
    PRINT '  → 4 rows seeded.';
END
ELSE
    PRINT 'mro2.LimitType already exists — skipped.';
GO

-- ============================================================
-- TABLE 5: mro2.CounterReference
-- Document references that authorise a limit.
-- RefCategory:
--   EVENT    : life event (SNEW, SOH, install date, cure date)
--   DOCUMENT : technical doc (AMM, CMM, SB, AD, IPC, TR)
-- ============================================================
IF OBJECT_ID('mro2.CounterReference','U') IS NULL
BEGIN
    CREATE TABLE mro2.CounterReference (
        CounterReferenceId  INT          NOT NULL IDENTITY(1,1),
        Code                VARCHAR(30)  NOT NULL,
        Name                NVARCHAR(150)NOT NULL,
        RefCategory         VARCHAR(20)  NOT NULL
            CONSTRAINT DF_CounterRef_Category    DEFAULT ('EVENT'),
        SortOrder           TINYINT      NOT NULL
            CONSTRAINT DF_CounterRef_SortOrder   DEFAULT (99),
        IsActive            BIT          NOT NULL
            CONSTRAINT DF_CounterRef_IsActive    DEFAULT (1),
        CreatedDate         DATETIME     NOT NULL
            CONSTRAINT DF_CounterRef_CreatedDate DEFAULT (GETDATE()),

        CONSTRAINT PK_CounterReference      PRIMARY KEY (CounterReferenceId),
        CONSTRAINT UQ_CounterReference_Code UNIQUE (Code),
        CONSTRAINT CK_CounterRef_Category
            CHECK (RefCategory IN ('EVENT','DOCUMENT'))
    );
    PRINT 'mro2.CounterReference created.';

    INSERT INTO mro2.CounterReference (Code, Name, RefCategory, SortOrder)
    VALUES
        ('SNEW',         'Since New',                     'EVENT',    1),
        ('SOH',          'Since Last Overhaul',           'EVENT',    2),
        ('SRI',          'Since Last Repair',             'EVENT',    3),
        ('SINSP',        'Since Last Inspection',         'EVENT',    4),
        ('MFG_DATE',     'Manufacture Date',              'EVENT',    5),
        ('INSTALL_DATE', 'Installation Date on Aircraft', 'EVENT',    6),
        ('REC_DATE',     'Received Date (in store)',      'EVENT',    7),
        ('CURE_DATE',    'Cure Date (elastomers)',        'EVENT',    8),
        ('AMM',          'Aircraft Maintenance Manual',   'DOCUMENT', 10),
        ('CMM',          'Component Maintenance Manual',  'DOCUMENT', 11),
        ('IPC',          'Illustrated Parts Catalog',     'DOCUMENT', 12),
        ('SB',           'Service Bulletin',              'DOCUMENT', 13),
        ('AD',           'Airworthiness Directive',       'DOCUMENT', 14),
        ('TR',           'Technical Report',              'DOCUMENT', 15);
    PRINT '  → 14 rows seeded.';
END
ELSE
    PRINT 'mro2.CounterReference already exists — skipped.';
GO

-- ============================================================
-- STORED PROCEDURES — CounterType (List / Save / SetActive)
-- ============================================================
IF OBJECT_ID('mro2.usp_CounterType_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_CounterType_List;
GO
CREATE PROCEDURE mro2.usp_CounterType_List
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CounterTypeId, Code, Name, UnitStorage, DisplayUnit,
           SortOrder, IsActive, CreatedDate
    FROM mro2.CounterType
    WHERE (@IncludeInactive=1 OR IsActive=1)
    ORDER BY SortOrder, Code;
END
GO

IF OBJECT_ID('mro2.usp_CounterType_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_CounterType_Save;
GO
CREATE PROCEDURE mro2.usp_CounterType_Save
    @CounterTypeId  SMALLINT      = NULL,
    @Code           VARCHAR(20),
    @Name           NVARCHAR(100),
    @UnitStorage    VARCHAR(10),
    @DisplayUnit    VARCHAR(20),
    @SortOrder      TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Code = UPPER(LTRIM(RTRIM(@Code)));
    IF @CounterTypeId IS NULL
    BEGIN
        INSERT INTO mro2.CounterType (Code,Name,UnitStorage,DisplayUnit,SortOrder)
        VALUES (@Code,@Name,@UnitStorage,@DisplayUnit,@SortOrder);
        SELECT SCOPE_IDENTITY() AS CounterTypeId;
    END
    ELSE
    BEGIN
        UPDATE mro2.CounterType
        SET Code=@Code, Name=@Name, UnitStorage=@UnitStorage,
            DisplayUnit=@DisplayUnit, SortOrder=@SortOrder
        WHERE CounterTypeId=@CounterTypeId;
        SELECT @CounterTypeId AS CounterTypeId;
    END
END
GO

IF OBJECT_ID('mro2.usp_CounterType_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_CounterType_SetActive;
GO
CREATE PROCEDURE mro2.usp_CounterType_SetActive
    @CounterTypeId SMALLINT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.CounterType SET IsActive=@IsActive
    WHERE CounterTypeId=@CounterTypeId;
END
GO

-- ============================================================
-- STORED PROCEDURES — CounterDef
-- ============================================================
IF OBJECT_ID('mro2.usp_CounterDef_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_CounterDef_List;
GO
CREATE PROCEDURE mro2.usp_CounterDef_List
    @CounterTypeId          SMALLINT    = NULL,
    @AppliesToAssetKindCode VARCHAR(20) = NULL,
    @IncludeInactive        BIT         = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cd.CounterDefId, cd.CounterTypeId,
           ct.Code AS CounterTypeCode, ct.Name AS CounterTypeName,
           ct.DisplayUnit, cd.Code, cd.Name,
           cd.AppliesToAssetKindCode, cd.UnitStorage,
           cd.SortOrder, cd.IsActive, cd.CreatedDate
    FROM mro2.CounterDef cd
    INNER JOIN mro2.CounterType ct ON cd.CounterTypeId=ct.CounterTypeId
    WHERE (@CounterTypeId IS NULL OR cd.CounterTypeId=@CounterTypeId)
      AND (@AppliesToAssetKindCode IS NULL
           OR cd.AppliesToAssetKindCode=@AppliesToAssetKindCode)
      AND (@IncludeInactive=1 OR cd.IsActive=1)
    ORDER BY cd.AppliesToAssetKindCode, ct.SortOrder, cd.SortOrder, cd.Code;
END
GO

IF OBJECT_ID('mro2.usp_CounterDef_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_CounterDef_Save;
GO
CREATE PROCEDURE mro2.usp_CounterDef_Save
    @CounterDefId           INT         = NULL,
    @CounterTypeId          SMALLINT,
    @Code                   VARCHAR(30),
    @Name                   NVARCHAR(150),
    @AppliesToAssetKindCode VARCHAR(20),
    @SortOrder              TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Code = UPPER(LTRIM(RTRIM(@Code)));
    DECLARE @UnitStorage VARCHAR(10);
    SELECT @UnitStorage=UnitStorage FROM mro2.CounterType
    WHERE CounterTypeId=@CounterTypeId;

    IF @CounterDefId IS NULL
    BEGIN
        INSERT INTO mro2.CounterDef
            (CounterTypeId,Code,Name,AppliesToAssetKindCode,UnitStorage,SortOrder)
        VALUES (@CounterTypeId,@Code,@Name,@AppliesToAssetKindCode,@UnitStorage,@SortOrder);
        SELECT SCOPE_IDENTITY() AS CounterDefId;
    END
    ELSE
    BEGIN
        UPDATE mro2.CounterDef
        SET CounterTypeId=@CounterTypeId, Code=@Code, Name=@Name,
            AppliesToAssetKindCode=@AppliesToAssetKindCode,
            UnitStorage=@UnitStorage, SortOrder=@SortOrder
        WHERE CounterDefId=@CounterDefId;
        SELECT @CounterDefId AS CounterDefId;
    END
END
GO

IF OBJECT_ID('mro2.usp_CounterDef_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_CounterDef_SetActive;
GO
CREATE PROCEDURE mro2.usp_CounterDef_SetActive
    @CounterDefId INT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.CounterDef SET IsActive=@IsActive WHERE CounterDefId=@CounterDefId;
END
GO

-- ============================================================
-- STORED PROCEDURES — CounterBasis / LimitType / CounterReference
-- (abbreviated pattern — same List/Save/SetActive for each)
-- ============================================================
IF OBJECT_ID('mro2.usp_CounterBasis_List','P') IS NOT NULL DROP PROCEDURE mro2.usp_CounterBasis_List;
GO
CREATE PROCEDURE mro2.usp_CounterBasis_List @IncludeInactive BIT=0 AS
BEGIN
    SET NOCOUNT ON;
    SELECT CounterBasisId,Code,Name,Description,SortOrder,IsActive
    FROM mro2.CounterBasis WHERE (@IncludeInactive=1 OR IsActive=1) ORDER BY SortOrder;
END
GO
IF OBJECT_ID('mro2.usp_CounterBasis_Save','P') IS NOT NULL DROP PROCEDURE mro2.usp_CounterBasis_Save;
GO
CREATE PROCEDURE mro2.usp_CounterBasis_Save
    @CounterBasisId TINYINT=NULL, @Code VARCHAR(20),
    @Name NVARCHAR(100), @Description NVARCHAR(300)=NULL, @SortOrder TINYINT
AS
BEGIN
    SET NOCOUNT ON; SET @Code=UPPER(LTRIM(RTRIM(@Code)));
    IF @CounterBasisId IS NULL
    BEGIN INSERT INTO mro2.CounterBasis(Code,Name,Description,SortOrder)
          VALUES(@Code,@Name,@Description,@SortOrder);
          SELECT SCOPE_IDENTITY() AS CounterBasisId; END
    ELSE BEGIN UPDATE mro2.CounterBasis SET Code=@Code,Name=@Name,
               Description=@Description,SortOrder=@SortOrder
               WHERE CounterBasisId=@CounterBasisId;
               SELECT @CounterBasisId AS CounterBasisId; END
END
GO
IF OBJECT_ID('mro2.usp_CounterBasis_SetActive','P') IS NOT NULL DROP PROCEDURE mro2.usp_CounterBasis_SetActive;
GO
CREATE PROCEDURE mro2.usp_CounterBasis_SetActive @CounterBasisId TINYINT, @IsActive BIT AS
BEGIN SET NOCOUNT ON; UPDATE mro2.CounterBasis SET IsActive=@IsActive WHERE CounterBasisId=@CounterBasisId; END
GO

IF OBJECT_ID('mro2.usp_LimitType_List','P') IS NOT NULL DROP PROCEDURE mro2.usp_LimitType_List;
GO
CREATE PROCEDURE mro2.usp_LimitType_List @IncludeInactive BIT=0 AS
BEGIN
    SET NOCOUNT ON;
    SELECT LimitTypeId,Code,Name,Description,BadgeColor,SortOrder,IsActive
    FROM mro2.LimitType WHERE (@IncludeInactive=1 OR IsActive=1) ORDER BY SortOrder;
END
GO
IF OBJECT_ID('mro2.usp_LimitType_Save','P') IS NOT NULL DROP PROCEDURE mro2.usp_LimitType_Save;
GO
CREATE PROCEDURE mro2.usp_LimitType_Save
    @LimitTypeId TINYINT=NULL, @Code VARCHAR(20), @Name NVARCHAR(100),
    @Description NVARCHAR(300)=NULL, @BadgeColor VARCHAR(20), @SortOrder TINYINT
AS
BEGIN
    SET NOCOUNT ON; SET @Code=UPPER(LTRIM(RTRIM(@Code)));
    IF @LimitTypeId IS NULL
    BEGIN INSERT INTO mro2.LimitType(Code,Name,Description,BadgeColor,SortOrder)
          VALUES(@Code,@Name,@Description,@BadgeColor,@SortOrder);
          SELECT SCOPE_IDENTITY() AS LimitTypeId; END
    ELSE BEGIN UPDATE mro2.LimitType SET Code=@Code,Name=@Name,
               Description=@Description,BadgeColor=@BadgeColor,SortOrder=@SortOrder
               WHERE LimitTypeId=@LimitTypeId;
               SELECT @LimitTypeId AS LimitTypeId; END
END
GO
IF OBJECT_ID('mro2.usp_LimitType_SetActive','P') IS NOT NULL DROP PROCEDURE mro2.usp_LimitType_SetActive;
GO
CREATE PROCEDURE mro2.usp_LimitType_SetActive @LimitTypeId TINYINT, @IsActive BIT AS
BEGIN SET NOCOUNT ON; UPDATE mro2.LimitType SET IsActive=@IsActive WHERE LimitTypeId=@LimitTypeId; END
GO

IF OBJECT_ID('mro2.usp_CounterReference_List','P') IS NOT NULL DROP PROCEDURE mro2.usp_CounterReference_List;
GO
CREATE PROCEDURE mro2.usp_CounterReference_List
    @RefCategory VARCHAR(20)=NULL, @IncludeInactive BIT=0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CounterReferenceId,Code,Name,RefCategory,SortOrder,IsActive,CreatedDate
    FROM mro2.CounterReference
    WHERE (@RefCategory IS NULL OR RefCategory=@RefCategory)
      AND (@IncludeInactive=1 OR IsActive=1)
    ORDER BY RefCategory,SortOrder,Code;
END
GO
IF OBJECT_ID('mro2.usp_CounterReference_Save','P') IS NOT NULL DROP PROCEDURE mro2.usp_CounterReference_Save;
GO
CREATE PROCEDURE mro2.usp_CounterReference_Save
    @CounterReferenceId INT=NULL, @Code VARCHAR(30),
    @Name NVARCHAR(150), @RefCategory VARCHAR(20), @SortOrder TINYINT
AS
BEGIN
    SET NOCOUNT ON; SET @Code=UPPER(LTRIM(RTRIM(@Code)));
    IF @CounterReferenceId IS NULL
    BEGIN INSERT INTO mro2.CounterReference(Code,Name,RefCategory,SortOrder)
          VALUES(@Code,@Name,@RefCategory,@SortOrder);
          SELECT SCOPE_IDENTITY() AS CounterReferenceId; END
    ELSE BEGIN UPDATE mro2.CounterReference SET Code=@Code,Name=@Name,
               RefCategory=@RefCategory,SortOrder=@SortOrder
               WHERE CounterReferenceId=@CounterReferenceId;
               SELECT @CounterReferenceId AS CounterReferenceId; END
END
GO
IF OBJECT_ID('mro2.usp_CounterReference_SetActive','P') IS NOT NULL DROP PROCEDURE mro2.usp_CounterReference_SetActive;
GO
CREATE PROCEDURE mro2.usp_CounterReference_SetActive @CounterReferenceId INT, @IsActive BIT AS
BEGIN SET NOCOUNT ON; UPDATE mro2.CounterReference SET IsActive=@IsActive WHERE CounterReferenceId=@CounterReferenceId; END
GO

-- ============================================================
-- STEP 02 VERIFICATION
-- Run this block manually after executing the script above.
-- Expected: 10 / 10 / 4 / 4 / 14
-- ============================================================
/*
SELECT 'CounterType'     AS [Table], COUNT(*) AS Rows FROM mro2.CounterType
UNION ALL SELECT 'CounterDef',       COUNT(*) FROM mro2.CounterDef
UNION ALL SELECT 'CounterBasis',     COUNT(*) FROM mro2.CounterBasis
UNION ALL SELECT 'LimitType',        COUNT(*) FROM mro2.LimitType
UNION ALL SELECT 'CounterReference', COUNT(*) FROM mro2.CounterReference;
*/
PRINT '── Step 02 complete ─────────────────────────────────────';
