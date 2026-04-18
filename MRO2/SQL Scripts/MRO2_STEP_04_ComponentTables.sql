-- ============================================================
-- MRO2 — STEP 04 of 10
-- Core Component Tables: PartNumber, SerializedItem, PNLimit
-- ============================================================
-- TABLES CREATED:
--   mro2.ATAChapter          (lookup — ATA chapter codes)
--   mro2.UnitOfMeasure       (lookup — EA, KG, L, M...)
--   mro2.PartNumber          (the parts catalog)
--   mro2.SerializedItem      (tracked serialized components)
--   mro2.PNLimit             (limit definition per PN)
-- STORED PROCEDURES: 12
-- PREREQUISITE: Steps 01–03
-- ============================================================
-- NOTE: If mro2.PartNumber / mro2.SerializedItem / mro2.PNLimit
-- already exist from earlier work, this step will skip creation
-- and only ADD missing columns (IsPhased on PNLimit).
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- TABLE: mro2.ATAChapter (lookup)
-- ============================================================
IF OBJECT_ID('mro2.ATAChapter','U') IS NULL
BEGIN
    CREATE TABLE mro2.ATAChapter (
        ATAId       INT          NOT NULL IDENTITY(1,1),
        ATACode     VARCHAR(10)  NOT NULL,
        Name        NVARCHAR(100)NOT NULL,
        IsActive    BIT          NOT NULL CONSTRAINT DF_ATA_IsActive DEFAULT(1),

        CONSTRAINT PK_ATAChapter      PRIMARY KEY (ATAId),
        CONSTRAINT UQ_ATAChapter_Code UNIQUE (ATACode)
    );
    -- Common ATA chapters — extend as needed
    INSERT INTO mro2.ATAChapter (ATACode, Name) VALUES
        ('05',  'Time Limits / Maintenance Checks'),
        ('06',  'Dimensions and Areas'),
        ('12',  'Servicing'),
        ('21',  'Air Conditioning'),
        ('24',  'Electrical Power'),
        ('27',  'Flight Controls'),
        ('28',  'Fuel'),
        ('29',  'Hydraulic Power'),
        ('32',  'Landing Gear'),
        ('71',  'Power Plant'),
        ('72',  'Engine'),
        ('73',  'Engine Fuel and Control'),
        ('76',  'Engine Controls'),
        ('79',  'Oil');
    PRINT 'mro2.ATAChapter created and seeded.';
END
ELSE
    PRINT 'mro2.ATAChapter already exists — skipped.';
GO

-- ============================================================
-- TABLE: mro2.UnitOfMeasure (lookup)
-- ============================================================
IF OBJECT_ID('mro2.UnitOfMeasure','U') IS NULL
BEGIN
    CREATE TABLE mro2.UnitOfMeasure (
        UnitOfMeasureId INT         NOT NULL IDENTITY(1,1),
        Code            VARCHAR(10) NOT NULL,
        Name            NVARCHAR(50)NOT NULL,
        IsActive        BIT         NOT NULL CONSTRAINT DF_UOM_IsActive DEFAULT(1),

        CONSTRAINT PK_UnitOfMeasure      PRIMARY KEY (UnitOfMeasureId),
        CONSTRAINT UQ_UnitOfMeasure_Code UNIQUE (Code)
    );
    INSERT INTO mro2.UnitOfMeasure (Code, Name) VALUES
        ('EA',  'Each'),
        ('KG',  'Kilogram'),
        ('G',   'Gram'),
        ('L',   'Litre'),
        ('ML',  'Millilitre'),
        ('M',   'Metre'),
        ('M2',  'Square Metre'),
        ('SET', 'Set'),
        ('KIT', 'Kit');
    PRINT 'mro2.UnitOfMeasure created and seeded.';
END
ELSE
    PRINT 'mro2.UnitOfMeasure already exists — skipped.';
GO

-- ============================================================
-- TABLE: mro2.PartNumber
-- ============================================================
IF OBJECT_ID('mro2.PartNumber','U') IS NULL
BEGIN
    CREATE TABLE mro2.PartNumber (
        PartNumberId    INT          NOT NULL IDENTITY(1,1),
        PN              VARCHAR(60)  NOT NULL,
        Nomenclature    NVARCHAR(200)NULL,
        ATAId           INT          NULL,
        IsSerialized    BIT          NOT NULL CONSTRAINT DF_PN_IsSerialized DEFAULT(0),
        UnitOfMeasureId INT          NOT NULL,
        AcMainGroupID   INT          NULL,   -- FK → dbo.tblAcMainGroup
        IsActive        BIT          NOT NULL CONSTRAINT DF_PN_IsActive DEFAULT(1),
        CreatedDate     DATETIME     NOT NULL CONSTRAINT DF_PN_CreatedDate DEFAULT(GETDATE()),

        CONSTRAINT PK_PartNumber     PRIMARY KEY (PartNumberId),
        CONSTRAINT UQ_PartNumber_PN  UNIQUE (PN),
        CONSTRAINT FK_PN_ATA
            FOREIGN KEY (ATAId) REFERENCES mro2.ATAChapter (ATAId),
        CONSTRAINT FK_PN_UOM
            FOREIGN KEY (UnitOfMeasureId) REFERENCES mro2.UnitOfMeasure (UnitOfMeasureId),
        CONSTRAINT FK_PN_AcMainGroup
            FOREIGN KEY (AcMainGroupID) REFERENCES dbo.tblAcMainGroup (AcMainGroupID)
    );
    PRINT 'mro2.PartNumber created.';
END
ELSE
    PRINT 'mro2.PartNumber already exists — skipped.';
GO

-- ============================================================
-- TABLE: mro2.SerializedItem
-- ============================================================
IF OBJECT_ID('mro2.SerializedItem','U') IS NULL
BEGIN
    CREATE TABLE mro2.SerializedItem (
        SerializedItemId    INT          NOT NULL IDENTITY(1,1),
        PartNumberId        INT          NOT NULL,
        SerialNumber        VARCHAR(80)  NOT NULL,
        ManufacturedDate    DATE         NULL,
        ReceivedDate        DATE         NULL,
        -- ACTIVE | SERVICEABLE | UNSERVICEABLE | SCRAP
        StatusCode          VARCHAR(20)  NOT NULL
            CONSTRAINT DF_SI_StatusCode DEFAULT('ACTIVE'),
        Notes               NVARCHAR(300)NULL,
        IsActive            BIT          NOT NULL
            CONSTRAINT DF_SI_IsActive DEFAULT(1),
        CreatedDate         DATETIME     NOT NULL
            CONSTRAINT DF_SI_CreatedDate DEFAULT(GETDATE()),

        CONSTRAINT PK_SerializedItem PRIMARY KEY (SerializedItemId),
        CONSTRAINT UQ_SerializedItem_PN_SN UNIQUE (PartNumberId, SerialNumber),
        CONSTRAINT FK_SI_PartNumber
            FOREIGN KEY (PartNumberId) REFERENCES mro2.PartNumber (PartNumberId),
        CONSTRAINT CK_SI_StatusCode
            CHECK (StatusCode IN ('ACTIVE','SERVICEABLE','UNSERVICEABLE','SCRAP'))
    );
    CREATE INDEX IX_SerializedItem_PartNumberId
        ON mro2.SerializedItem (PartNumberId)
        INCLUDE (SerialNumber, StatusCode, IsActive);
    PRINT 'mro2.SerializedItem created.';
END
ELSE
    PRINT 'mro2.SerializedItem already exists — skipped.';
GO

-- ============================================================
-- TABLE: mro2.PNLimit
-- One limit definition per PN per counter.
-- IsPhased: 0 = simple limit, 1 = uses TaskCounter plan.
-- ============================================================
IF OBJECT_ID('mro2.PNLimit','U') IS NULL
BEGIN
    CREATE TABLE mro2.PNLimit (
        PNLimitId           INT           NOT NULL IDENTITY(1,1),
        PartNumberId        INT           NOT NULL,
        -- What kind of limit (LIFE, INSPECTION, FUNCTIONAL, SHELF_LIFE)
        LimitTypeId         TINYINT       NULL,   -- FK → mro2.LimitType
        -- Simple limit fields (used when IsPhased=0)
        HardLimit           DECIMAL(10,1) NOT NULL,
        AlertThresholdPct   TINYINT       NOT NULL
            CONSTRAINT DF_PNLimit_AlertPct   DEFAULT(90),
        CounterReferenceId  INT           NULL,   -- FK → mro2.CounterReference (document)
        -- Phased/TaskCounter flag
        IsPhased            BIT           NOT NULL
            CONSTRAINT DF_PNLimit_IsPhased   DEFAULT(0),
        Notes               NVARCHAR(300) NULL,
        IsActive            BIT           NOT NULL
            CONSTRAINT DF_PNLimit_IsActive   DEFAULT(1),
        CreatedDate         DATETIME      NOT NULL
            CONSTRAINT DF_PNLimit_CreatedDate DEFAULT(GETDATE()),
        CreatedByUserId     NVARCHAR(50)  NOT NULL,

        CONSTRAINT PK_PNLimit PRIMARY KEY (PNLimitId),
        CONSTRAINT FK_PNLimit_PartNumber
            FOREIGN KEY (PartNumberId) REFERENCES mro2.PartNumber (PartNumberId),
        CONSTRAINT FK_PNLimit_LimitType
            FOREIGN KEY (LimitTypeId) REFERENCES mro2.LimitType (LimitTypeId),
        CONSTRAINT FK_PNLimit_CounterRef
            FOREIGN KEY (CounterReferenceId)
            REFERENCES mro2.CounterReference (CounterReferenceId),
        CONSTRAINT CK_PNLimit_HardLimit
            CHECK (HardLimit > 0),
        CONSTRAINT CK_PNLimit_AlertPct
            CHECK (AlertThresholdPct BETWEEN 1 AND 99)
    );
    PRINT 'mro2.PNLimit created.';
END
ELSE
BEGIN
    PRINT 'mro2.PNLimit already exists — checking for missing columns...';
    -- Add IsPhased if missing (from earlier version without it)
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id=OBJECT_ID('mro2.PNLimit') AND name='IsPhased')
    BEGIN
        ALTER TABLE mro2.PNLimit ADD IsPhased BIT NOT NULL
            CONSTRAINT DF_PNLimit_IsPhased DEFAULT(0);
        PRINT '  → IsPhased column added.';
    END
    ELSE
        PRINT '  → IsPhased already present.';

    -- Add LimitTypeId if missing
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id=OBJECT_ID('mro2.PNLimit') AND name='LimitTypeId')
    BEGIN
        ALTER TABLE mro2.PNLimit ADD LimitTypeId TINYINT NULL;
        PRINT '  → LimitTypeId column added.';
    END
    ELSE
        PRINT '  → LimitTypeId already present.';
END
GO

-- ============================================================
-- STORED PROCEDURES — PartNumber / SerializedItem / PNLimit
-- ============================================================
IF OBJECT_ID('mro2.usp_PartNumber_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_PartNumber_List;
GO
ALTER PROCEDURE mro2.usp_PartNumber_List
    @IncludeInactive    BIT           = 0,
    @Search             NVARCHAR(200) = NULL,
    @SortColumn         VARCHAR(50)   = 'PN',
    @SortDir            VARCHAR(4)    = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    IF @SortColumn NOT IN ('PN','Nomenclature','ATACode','UOMCode','IsSerialized','IsActive')
        SET @SortColumn = 'PN';
    IF @SortDir NOT IN ('ASC','DESC') SET @SortDir = 'ASC';

    SELECT
        pn.PartNumberId,
        pn.PN,
        ISNULL(pn.Nomenclature,'')          AS Nomenclature,
        ISNULL(ata.ATACode,'')              AS ATACode,
        ISNULL(uom.Code,'')                 AS UOMCode,
        pn.IsSerialized,
        pn.IsActive,
        ISNULL(lc.LimitCount, 0)            AS LimitCount
    FROM mro2.PartNumber pn
    LEFT JOIN mro2.ATA    ata ON pn.ATAId          = ata.ATAId
    LEFT JOIN mro2.UnitOfMeasure uom ON pn.UnitOfMeasureId= uom.UnitOfMeasureId
    LEFT JOIN (
        SELECT PartNumberId, COUNT(*) AS LimitCount
        FROM mro2.PNLimit WHERE IsActive=1
        GROUP BY PartNumberId
    ) lc ON lc.PartNumberId = pn.PartNumberId
    WHERE (@IncludeInactive=1 OR pn.IsActive=1)
      AND (@Search IS NULL
           OR pn.PN           LIKE '%'+@Search+'%'
           OR pn.Nomenclature LIKE '%'+@Search+'%'
           OR ata.ATACode     LIKE '%'+@Search+'%')
    ORDER BY
        CASE WHEN @SortColumn='PN'           AND @SortDir='ASC'  THEN pn.PN          END ASC,
        CASE WHEN @SortColumn='PN'           AND @SortDir='DESC' THEN pn.PN          END DESC,
        CASE WHEN @SortColumn='Nomenclature' AND @SortDir='ASC'  THEN pn.Nomenclature END ASC,
        CASE WHEN @SortColumn='Nomenclature' AND @SortDir='DESC' THEN pn.Nomenclature END DESC,
        pn.PN ASC;
END
GO

IF OBJECT_ID('mro2.usp_PartNumber_Save','P') IS NOT NULL DROP PROCEDURE mro2.usp_PartNumber_Save;
GO
CREATE PROCEDURE mro2.usp_PartNumber_Save
    @PartNumberId    INT          = NULL,
    @PN              VARCHAR(60),
    @Nomenclature    NVARCHAR(200)= NULL,
    @ATAId           INT          = NULL,
    @IsSerialized    BIT,
    @UnitOfMeasureId INT,
    @AcMainGroupID   INT          = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @PN = UPPER(LTRIM(RTRIM(@PN)));
    IF @PartNumberId IS NULL
    BEGIN
        INSERT INTO mro2.PartNumber
            (PN,Nomenclature,ATAId,IsSerialized,UnitOfMeasureId,AcMainGroupID)
        VALUES (@PN,@Nomenclature,@ATAId,@IsSerialized,@UnitOfMeasureId,@AcMainGroupID);
        SELECT SCOPE_IDENTITY() AS PartNumberId;
    END
    ELSE
    BEGIN
        UPDATE mro2.PartNumber
        SET PN=@PN, Nomenclature=@Nomenclature, ATAId=@ATAId,
            IsSerialized=@IsSerialized, UnitOfMeasureId=@UnitOfMeasureId,
            AcMainGroupID=@AcMainGroupID
        WHERE PartNumberId=@PartNumberId;
        SELECT @PartNumberId AS PartNumberId;
    END
END
GO

IF OBJECT_ID('mro2.usp_PartNumber_SetActive','P') IS NOT NULL DROP PROCEDURE mro2.usp_PartNumber_SetActive;
GO
CREATE PROCEDURE mro2.usp_PartNumber_SetActive @PartNumberId INT, @IsActive BIT AS
BEGIN SET NOCOUNT ON; UPDATE mro2.PartNumber SET IsActive=@IsActive WHERE PartNumberId=@PartNumberId; END
GO

IF OBJECT_ID('mro2.usp_SerializedItem_List','P') IS NOT NULL DROP PROCEDURE mro2.usp_SerializedItem_List;
GO
CREATE PROCEDURE mro2.usp_SerializedItem_List
    @IncludeInactive BIT=0, @Search NVARCHAR(200)=NULL,
    @SortColumn VARCHAR(50)='SerialNumber', @SortDir VARCHAR(4)='ASC'
AS
BEGIN
    SET NOCOUNT ON;
    SELECT si.SerializedItemId, si.PartNumberId,
           pn.PN, pn.Nomenclature,
           si.SerialNumber, si.ManufacturedDate, si.ReceivedDate,
           si.StatusCode, si.Notes, si.IsActive
    FROM mro2.SerializedItem si
    INNER JOIN mro2.PartNumber pn ON si.PartNumberId=pn.PartNumberId
    WHERE (@IncludeInactive=1 OR si.IsActive=1)
      AND (@Search IS NULL
           OR si.SerialNumber LIKE '%'+@Search+'%'
           OR pn.PN           LIKE '%'+@Search+'%'
           OR pn.Nomenclature LIKE '%'+@Search+'%')
    ORDER BY
        CASE WHEN @SortColumn='SerialNumber' AND @SortDir='ASC' THEN si.SerialNumber END ASC,
        CASE WHEN @SortColumn='SerialNumber' AND @SortDir='DESC' THEN si.SerialNumber END DESC,
        CASE WHEN @SortColumn='PN' AND @SortDir='ASC' THEN pn.PN END ASC,
        si.SerialNumber ASC;
END
GO

IF OBJECT_ID('mro2.usp_SerializedItem_Save','P') IS NOT NULL DROP PROCEDURE mro2.usp_SerializedItem_Save;
GO
CREATE PROCEDURE mro2.usp_SerializedItem_Save
    @SerializedItemId INT=NULL, @PartNumberId INT,
    @SerialNumber VARCHAR(80), @ManufacturedDate DATE=NULL,
    @ReceivedDate DATE=NULL, @StatusCode VARCHAR(20), @Notes NVARCHAR(300)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @SerialNumber=UPPER(LTRIM(RTRIM(@SerialNumber)));
    IF @SerializedItemId IS NULL
    BEGIN
        INSERT INTO mro2.SerializedItem
            (PartNumberId,SerialNumber,ManufacturedDate,ReceivedDate,StatusCode,Notes)
        VALUES (@PartNumberId,@SerialNumber,@ManufacturedDate,@ReceivedDate,@StatusCode,@Notes);
        SELECT SCOPE_IDENTITY() AS SerializedItemId;
    END
    ELSE
    BEGIN
        UPDATE mro2.SerializedItem
        SET SerialNumber=@SerialNumber, ManufacturedDate=@ManufacturedDate,
            ReceivedDate=@ReceivedDate, StatusCode=@StatusCode, Notes=@Notes
        WHERE SerializedItemId=@SerializedItemId;
        SELECT @SerializedItemId AS SerializedItemId;
    END
END
GO

IF OBJECT_ID('mro2.usp_SerializedItem_SetActive','P') IS NOT NULL DROP PROCEDURE mro2.usp_SerializedItem_SetActive;
GO
CREATE PROCEDURE mro2.usp_SerializedItem_SetActive @SerializedItemId INT, @IsActive BIT AS
BEGIN SET NOCOUNT ON; UPDATE mro2.SerializedItem SET IsActive=@IsActive WHERE SerializedItemId=@SerializedItemId; END
GO

IF OBJECT_ID('mro2.usp_PNLimit_List','P') IS NOT NULL DROP PROCEDURE mro2.usp_PNLimit_List;
GO
CREATE PROCEDURE mro2.usp_PNLimit_List
    @PartNumberId INT, @IncludeInactive BIT=1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pl.PNLimitId, pl.PartNumberId,
           lt.LimitTypeId, lt.Code AS LimitTypeCode,
           lt.Name AS LimitTypeName, lt.BadgeColor,
           pl.HardLimit, pl.AlertThresholdPct,
           pl.CounterReferenceId,
           cr.Code AS CounterReferenceCode,
           pl.IsPhased, pl.Notes, pl.IsActive,
           pl.CreatedDate, pl.CreatedByUserId,
           ISNULL(snc.SNCount,0) AS SNCount
    FROM mro2.PNLimit pl
    LEFT JOIN mro2.LimitType       lt  ON lt.LimitTypeId        = pl.LimitTypeId
    LEFT JOIN mro2.CounterReference cr ON cr.CounterReferenceId = pl.CounterReferenceId
    LEFT JOIN (
        SELECT PNLimitId, COUNT(*) AS SNCount
        FROM mro2.TaskCounter tc
        INNER JOIN mro2.SNTaskCounterState st ON st.TaskCounterId=tc.TaskCounterId
        GROUP BY PNLimitId
    ) snc ON snc.PNLimitId = pl.PNLimitId
    WHERE pl.PartNumberId=@PartNumberId
      AND (@IncludeInactive=1 OR pl.IsActive=1)
    ORDER BY pl.PNLimitId;
END
GO

IF OBJECT_ID('mro2.usp_PNLimit_Save','P') IS NOT NULL DROP PROCEDURE mro2.usp_PNLimit_Save;
GO
CREATE PROCEDURE mro2.usp_PNLimit_Save
    @PNLimitId          INT           = NULL,
    @PartNumberId       INT,
    @LimitTypeId        TINYINT       = NULL,
    @HardLimit          DECIMAL(10,1),
    @AlertThresholdPct  TINYINT,
    @CounterReferenceId INT           = NULL,
    @Notes              NVARCHAR(300) = NULL,
    @UserId             NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF @PNLimitId IS NULL
    BEGIN
        INSERT INTO mro2.PNLimit
            (PartNumberId,LimitTypeId,HardLimit,AlertThresholdPct,
             CounterReferenceId,Notes,CreatedByUserId)
        VALUES (@PartNumberId,@LimitTypeId,@HardLimit,@AlertThresholdPct,
                @CounterReferenceId,@Notes,@UserId);
        SELECT SCOPE_IDENTITY() AS PNLimitId;
    END
    ELSE
    BEGIN
        UPDATE mro2.PNLimit
        SET LimitTypeId=@LimitTypeId, HardLimit=@HardLimit,
            AlertThresholdPct=@AlertThresholdPct,
            CounterReferenceId=@CounterReferenceId, Notes=@Notes
        WHERE PNLimitId=@PNLimitId;
        SELECT @PNLimitId AS PNLimitId;
    END
END
GO

IF OBJECT_ID('mro2.usp_PNLimit_SetActive','P') IS NOT NULL DROP PROCEDURE mro2.usp_PNLimit_SetActive;
GO
CREATE PROCEDURE mro2.usp_PNLimit_SetActive @PNLimitId INT, @IsActive BIT AS
BEGIN SET NOCOUNT ON; UPDATE mro2.PNLimit SET IsActive=@IsActive WHERE PNLimitId=@PNLimitId; END
GO

-- ============================================================
-- STEP 04 VERIFICATION
-- ============================================================
/*
SELECT 'ATA'     AS [Table], COUNT(*) AS Rows FROM mro2.ATAChapter
UNION ALL SELECT 'UnitOfMeasure',   COUNT(*) FROM mro2.UnitOfMeasure
UNION ALL SELECT 'PartNumber',      COUNT(*) FROM mro2.PartNumber
UNION ALL SELECT 'SerializedItem',  COUNT(*) FROM mro2.SerializedItem
UNION ALL SELECT 'PNLimit',         COUNT(*) FROM mro2.PNLimit;

-- Verify IsPhased column exists
SELECT COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='mro2' AND TABLE_NAME='PNLimit'
  AND COLUMN_NAME IN ('IsPhased','LimitTypeId');
*/
PRINT '── Step 04 complete ─────────────────────────────────────';
