-- ============================================================
-- MRO2 — STEP 07 of 10
-- Aircraft Position Tree (AcPosition)
-- DB      : DB2BAFRA  (SQL Server 2012)
-- Schema  : mro2
-- ============================================================
-- TABLES CREATED:
--   mro2.AcPositionTemplate      Zone/System/Slot tree per AcType
--   mro2.AcPosition              Per-tail position tree
--                                (copied from template, tail can override)
--   mro2.AcPositionPN            Which PNs are allowed at a position
--                                (primary + alternates)
-- STORED PROCEDURES: 9
-- PREREQUISITE: Steps 01-06
--               dbo.tblAircraft, dbo.tblAcType must exist
-- ============================================================
--
-- DESIGN:
--
--   TWO-LAYER MODEL:
--
--   Layer 1 — AcPositionTemplate (per AcType)
--     Defines the canonical position tree for an aircraft type.
--     e.g. F-16C has: LH-MLG, RH-MLG, ENG-1, APU, etc.
--     All F-16C tails inherit this tree.
--     Maintained by engineering — changes here propagate to tails
--     that have not been individually overridden.
--
--   Layer 2 — AcPosition (per tail / AcID)
--     The actual position tree for one specific aircraft.
--     Populated by copying the template (usp_AcPosition_CopyFromTemplate).
--     A tail can add positions not in the type template (IsOverride=1).
--     A tail can deactivate positions from the template (IsActive=0).
--     RecordEvent always works against AcPosition rows, never template.
--
--   HIERARCHY (3 levels, self-referencing ParentPositionId):
--     Level 1 — Zone      : LH-MLG, RH-MLG, ENG-1, ENG-2, APU, AIRFRAME
--     Level 2 — System    : LH-MLG-BRAKE, LH-MLG-TIRE, ENG-1-FAN
--     Level 3 — Slot      : LH-MLG-TIRE-1, ENG-1-HPT-STAGE-1
--
--   POSITION PN LINK (AcPositionPN):
--     Links which PartNumbers are allowed at a position.
--     IsPrimary=1 : the standard/primary PN for this slot
--     IsPrimary=0 : approved alternate PN
--     Drives validation in RecordEvent — cannot install a PN
--     that is not listed for that position.
--
-- ============================================================

USE DB2BAFRA;
GO

-- ============================================================
-- TABLE 1: mro2.AcPositionTemplate
--    Canonical position tree per AcType.
--    Self-referencing: ParentTemplatePositionId = NULL → Zone
--    PositionLevel: 1=Zone, 2=System, 3=Slot
--    Quantity: how many SNs this slot holds (usually 1,
--              sometimes 2 for dual-redundant items)
-- ============================================================
IF OBJECT_ID('mro2.AcPositionTemplate','U') IS NULL
BEGIN
    CREATE TABLE mro2.AcPositionTemplate (
        AcPositionTemplateId    INT             NOT NULL IDENTITY(1,1),
        AcTypeId                INT             NOT NULL,   -- FK → dbo.tblAcType
        ParentTemplatePositionId INT            NULL,       -- FK → self (parent zone/system)
        PositionLevel           TINYINT         NOT NULL,   -- 1=Zone, 2=System, 3=Slot
        PositionCode            VARCHAR(50)     NOT NULL,   -- LH-MLG-TIRE-1
        Description             NVARCHAR(200)   NULL,       -- Left Main Gear Tire Position 1
        ATAId                   INT             NULL,       -- FK → mro2.ATA
        -- Slot-level attributes (relevant at Level 3)
        Quantity                TINYINT         NOT NULL    -- SNs this slot holds
            CONSTRAINT DF_AcPosTmpl_Qty         DEFAULT (1),
        IsInterchangeable       BIT             NOT NULL    -- accepts alternate PNs
            CONSTRAINT DF_AcPosTmpl_Interch     DEFAULT (0),
        SortOrder               SMALLINT        NOT NULL
            CONSTRAINT DF_AcPosTmpl_Sort        DEFAULT (100),
        IsActive                BIT             NOT NULL
            CONSTRAINT DF_AcPosTmpl_IsActive    DEFAULT (1),
        CreatedDate             DATETIME        NOT NULL
            CONSTRAINT DF_AcPosTmpl_Created     DEFAULT (GETDATE()),
        CreatedByUserId         NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_AcPositionTemplate
            PRIMARY KEY (AcPositionTemplateId),

        -- PositionCode unique within a type
        CONSTRAINT UQ_AcPosTmpl_Type_Code
            UNIQUE (AcTypeId, PositionCode),

        CONSTRAINT FK_AcPosTmpl_AcType
            FOREIGN KEY (AcTypeId)
            REFERENCES dbo.tblAcType (AcTypeId),

        CONSTRAINT FK_AcPosTmpl_Parent
            FOREIGN KEY (ParentTemplatePositionId)
            REFERENCES mro2.AcPositionTemplate (AcPositionTemplateId),

        CONSTRAINT FK_AcPosTmpl_ATA
            FOREIGN KEY (ATAId)
            REFERENCES mro2.ATA (ATAId),

        CONSTRAINT CK_AcPosTmpl_Level
            CHECK (PositionLevel IN (1,2,3)),

        CONSTRAINT CK_AcPosTmpl_Quantity
            CHECK (Quantity >= 1)
    );

    -- Indexes
    CREATE INDEX IX_AcPosTmpl_AcTypeId
        ON mro2.AcPositionTemplate (AcTypeId, PositionLevel)
        INCLUDE (PositionCode, Description, ParentTemplatePositionId);

    CREATE INDEX IX_AcPosTmpl_Parent
        ON mro2.AcPositionTemplate (ParentTemplatePositionId)
        INCLUDE (PositionCode, PositionLevel);

    PRINT 'mro2.AcPositionTemplate created.';
END
ELSE
    PRINT 'mro2.AcPositionTemplate already exists — skipped.';
GO

-- ============================================================
-- TABLE 2: mro2.AcPosition
--    Per-tail position tree. Copied from template via SP.
--    IsOverride=1 : position added for this tail only (not in template)
--    IsOverride=0 : copied from template
--    TemplatePositionId : links back to source template row (NULL if override)
--    A tail can deactivate a template position (IsActive=0).
--    RecordEvent always references AcPositionId.
-- ============================================================
IF OBJECT_ID('mro2.AcPosition','U') IS NULL
BEGIN
    CREATE TABLE mro2.AcPosition (
        AcPositionId            INT             NOT NULL IDENTITY(1,1),
        AcID                    INT             NOT NULL,   -- FK → dbo.tblAircraft
        -- Link back to template source (NULL = tail-specific override)
        AcPositionTemplateId    INT             NULL,
        -- Parent position on this tail
        ParentAcPositionId      INT             NULL,       -- FK → self
        PositionLevel           TINYINT         NOT NULL,   -- 1=Zone, 2=System, 3=Slot
        PositionCode            VARCHAR(50)     NOT NULL,
        Description             NVARCHAR(200)   NULL,
        ATAId                   INT             NULL,
        Quantity                TINYINT         NOT NULL
            CONSTRAINT DF_AcPos_Qty             DEFAULT (1),
        IsInterchangeable       BIT             NOT NULL
            CONSTRAINT DF_AcPos_Interch         DEFAULT (0),
        -- IsOverride: this position is tail-specific, not from template
        IsOverride              BIT             NOT NULL
            CONSTRAINT DF_AcPos_IsOverride      DEFAULT (0),
        SortOrder               SMALLINT        NOT NULL
            CONSTRAINT DF_AcPos_Sort            DEFAULT (100),
        IsActive                BIT             NOT NULL
            CONSTRAINT DF_AcPos_IsActive        DEFAULT (1),
        CreatedDate             DATETIME        NOT NULL
            CONSTRAINT DF_AcPos_Created         DEFAULT (GETDATE()),
        CreatedByUserId         NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_AcPosition PRIMARY KEY (AcPositionId),

        -- PositionCode unique per tail
        CONSTRAINT UQ_AcPosition_Tail_Code
            UNIQUE (AcID, PositionCode),

        CONSTRAINT FK_AcPosition_Aircraft
            FOREIGN KEY (AcID)
            REFERENCES dbo.tblAircraft (AcID),

        CONSTRAINT FK_AcPosition_Template
            FOREIGN KEY (AcPositionTemplateId)
            REFERENCES mro2.AcPositionTemplate (AcPositionTemplateId),

        CONSTRAINT FK_AcPosition_Parent
            FOREIGN KEY (ParentAcPositionId)
            REFERENCES mro2.AcPosition (AcPositionId),

        CONSTRAINT FK_AcPosition_ATA
            FOREIGN KEY (ATAId)
            REFERENCES mro2.ATA (ATAId),

        CONSTRAINT CK_AcPosition_Level
            CHECK (PositionLevel IN (1,2,3)),

        CONSTRAINT CK_AcPosition_Quantity
            CHECK (Quantity >= 1)
    );

    -- Fast lookup by tail (used constantly by RecordEvent)
    CREATE INDEX IX_AcPosition_AcID
        ON mro2.AcPosition (AcID, PositionLevel, IsActive)
        INCLUDE (AcPositionId, PositionCode, Description,
                 ParentAcPositionId, Quantity);

    -- Fast lookup by template source (used by sync/propagation)
    CREATE INDEX IX_AcPosition_TemplateId
        ON mro2.AcPosition (AcPositionTemplateId)
        INCLUDE (AcID, PositionCode, IsActive);

    PRINT 'mro2.AcPosition created.';
END
ELSE
    PRINT 'mro2.AcPosition already exists — skipped.';
GO

-- ============================================================
-- TABLE 3: mro2.AcPositionPN
--    Links which PartNumbers are allowed at a position.
--    Works at the AcPositionTemplate level — applies to all
--    tails unless individually overridden via AcPositionPNOverride
--    (future). IsPrimary=1 = standard PN, IsPrimary=0 = alternate.
--    Drives install validation in RecordEvent.
-- ============================================================
IF OBJECT_ID('mro2.AcPositionPN','U') IS NULL
BEGIN
    CREATE TABLE mro2.AcPositionPN (
        AcPositionPNId          INT             NOT NULL IDENTITY(1,1),
        AcPositionTemplateId    INT             NOT NULL,   -- FK → template
        PartNumberId            INT             NOT NULL,   -- FK → mro2.PartNumber
        -- IsPrimary: 1 = standard/primary PN for this slot
        --            0 = approved alternate
        IsPrimary               BIT             NOT NULL
            CONSTRAINT DF_AcPosPN_IsPrimary     DEFAULT (1),
        -- Notes: e.g. "Alternate approved per EO-2024-045"
        Notes                   NVARCHAR(200)   NULL,
        IsActive                BIT             NOT NULL
            CONSTRAINT DF_AcPosPN_IsActive      DEFAULT (1),
        CreatedDate             DATETIME        NOT NULL
            CONSTRAINT DF_AcPosPN_Created       DEFAULT (GETDATE()),
        CreatedByUserId         NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_AcPositionPN PRIMARY KEY (AcPositionPNId),

        -- One entry per position + PN combination
        CONSTRAINT UQ_AcPositionPN_Pos_PN
            UNIQUE (AcPositionTemplateId, PartNumberId),

        CONSTRAINT FK_AcPosPN_Template
            FOREIGN KEY (AcPositionTemplateId)
            REFERENCES mro2.AcPositionTemplate (AcPositionTemplateId),

        CONSTRAINT FK_AcPosPN_PartNumber
            FOREIGN KEY (PartNumberId)
            REFERENCES mro2.PartNumber (PartNumberId)
    );

    CREATE INDEX IX_AcPositionPN_TemplateId
        ON mro2.AcPositionPN (AcPositionTemplateId, IsActive)
        INCLUDE (PartNumberId, IsPrimary);

    -- Reverse lookup: which positions accept this PN?
    CREATE INDEX IX_AcPositionPN_PartNumberId
        ON mro2.AcPositionPN (PartNumberId, IsActive)
        INCLUDE (AcPositionTemplateId, IsPrimary);

    PRINT 'mro2.AcPositionPN created.';
END
ELSE
    PRINT 'mro2.AcPositionPN already exists — skipped.';
GO

-- ============================================================
-- VIEW: mro2.vw_AcPositionTree
--    Full position tree for any tail with parent path.
--    ZoneName / SystemName / SlotName resolved via self-join.
--    Used by AircraftConfiguration page and RecordEvent UI.
-- ============================================================
IF OBJECT_ID('mro2.vw_AcPositionTree','V') IS NOT NULL
    DROP VIEW mro2.vw_AcPositionTree;
GO
CREATE VIEW mro2.vw_AcPositionTree
AS
SELECT
    -- Slot (Level 3)
    pos.AcPositionId,
    pos.AcID,
    pos.PositionCode,
    pos.Description,
    pos.PositionLevel,
    pos.Quantity,
    pos.IsInterchangeable,
    pos.IsOverride,
    pos.IsActive,
    pos.AcPositionTemplateId,
    pos.SortOrder,

    -- ATA
    pos.ATAId,
    ata.ATACode,

    -- Parent (Level 2 — System)
    sys.AcPositionId            AS SystemPositionId,
    sys.PositionCode            AS SystemCode,
    sys.Description             AS SystemName,

    -- Grandparent (Level 1 — Zone)
    zon.AcPositionId            AS ZonePositionId,
    zon.PositionCode            AS ZoneCode,
    zon.Description             AS ZoneName,

    -- Full path: ZONE / SYSTEM / SLOT
    ISNULL(zon.PositionCode,'') + ' / ' +
    ISNULL(sys.PositionCode,'') + ' / ' +
    pos.PositionCode            AS FullPath,

    -- Aircraft info
    ac.TailNo,
    acg.AcMainGroup             AS AcMainGroupName,
    act.AcType                  AS AcTypeName

FROM mro2.AcPosition pos

LEFT JOIN mro2.AcPosition    sys ON sys.AcPositionId = pos.ParentAcPositionId
LEFT JOIN mro2.AcPosition    zon ON zon.AcPositionId = sys.ParentAcPositionId
LEFT JOIN mro2.ATA           ata ON ata.ATAId        = pos.ATAId
LEFT JOIN dbo.tblAircraft    ac  ON ac.AcID          = pos.AcID
LEFT JOIN dbo.tblAcMainGroup acg ON acg.AcMainGroupID= ac.AcMainGroupID
LEFT JOIN dbo.tblAcType      act ON act.AcTypeId     = ac.AcTypeID;
GO
PRINT 'mro2.vw_AcPositionTree created.';
GO

-- ============================================================
-- SP: mro2.usp_AcPositionTemplate_List
--    Returns full template tree for an AcType.
--    Ordered by hierarchy: Zone → System → Slot.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPositionTemplate_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPositionTemplate_List;
GO
CREATE PROCEDURE mro2.usp_AcPositionTemplate_List
    @AcTypeId           INT,
    @IncludeInactive    BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.AcPositionTemplateId,
        t.AcTypeId,
        t.ParentTemplatePositionId,
        t.PositionLevel,
        t.PositionCode,
        t.Description,
        t.ATAId,
        ata.ATACode,
        t.Quantity,
        t.IsInterchangeable,
        t.SortOrder,
        t.IsActive,
        -- Parent code for display
        p.PositionCode  AS ParentCode,
        -- Count of allowed PNs
        ISNULL(pnc.PNCount, 0) AS PNCount
    FROM mro2.AcPositionTemplate t
    LEFT JOIN mro2.AcPositionTemplate p   ON p.AcPositionTemplateId = t.ParentTemplatePositionId
    LEFT JOIN mro2.ATA               ata  ON ata.ATAId              = t.ATAId
    LEFT JOIN (
        SELECT AcPositionTemplateId, COUNT(*) AS PNCount
        FROM mro2.AcPositionPN WHERE IsActive=1
        GROUP BY AcPositionTemplateId
    ) pnc ON pnc.AcPositionTemplateId = t.AcPositionTemplateId
    WHERE t.AcTypeId = @AcTypeId
      AND (@IncludeInactive=1 OR t.IsActive=1)
    ORDER BY t.PositionLevel, t.SortOrder, t.PositionCode;
END
GO

-- ============================================================
-- SP: mro2.usp_AcPositionTemplate_Save
--    Insert or update a template position row.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPositionTemplate_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPositionTemplate_Save;
GO
CREATE PROCEDURE mro2.usp_AcPositionTemplate_Save
    @AcPositionTemplateId       INT             = NULL,
    @AcTypeId                   INT,
    @ParentTemplatePositionId   INT             = NULL,
    @PositionLevel              TINYINT,
    @PositionCode               VARCHAR(50),
    @Description                NVARCHAR(200)   = NULL,
    @ATAId                      INT             = NULL,
    @Quantity                   TINYINT         = 1,
    @IsInterchangeable          BIT             = 0,
    @SortOrder                  SMALLINT        = 100,
    @UserId                     NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @PositionCode = UPPER(LTRIM(RTRIM(@PositionCode)));

    IF @AcPositionTemplateId IS NULL
    BEGIN
        INSERT INTO mro2.AcPositionTemplate (
            AcTypeId, ParentTemplatePositionId, PositionLevel,
            PositionCode, Description, ATAId,
            Quantity, IsInterchangeable, SortOrder,
            CreatedByUserId)
        VALUES (
            @AcTypeId, @ParentTemplatePositionId, @PositionLevel,
            @PositionCode, @Description, @ATAId,
            @Quantity, @IsInterchangeable, @SortOrder,
            @UserId);
        SELECT SCOPE_IDENTITY() AS AcPositionTemplateId;
    END
    ELSE
    BEGIN
        UPDATE mro2.AcPositionTemplate SET
            ParentTemplatePositionId = @ParentTemplatePositionId,
            PositionLevel   = @PositionLevel,
            PositionCode    = @PositionCode,
            Description     = @Description,
            ATAId           = @ATAId,
            Quantity        = @Quantity,
            IsInterchangeable = @IsInterchangeable,
            SortOrder       = @SortOrder
        WHERE AcPositionTemplateId = @AcPositionTemplateId;
        SELECT @AcPositionTemplateId AS AcPositionTemplateId;
    END
END
GO

-- ============================================================
-- SP: mro2.usp_AcPosition_CopyFromTemplate
--    Copies the full position tree from AcType template
--    to a specific tail (AcID). Safe to re-run — skips
--    positions that already exist on the tail.
--    Called when a new aircraft is added to the fleet.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPosition_CopyFromTemplate','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPosition_CopyFromTemplate;
GO
CREATE PROCEDURE mro2.usp_AcPosition_CopyFromTemplate
    @AcID   INT,
    @UserId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get AcTypeId for this tail
    DECLARE @AcTypeId INT;
    SELECT @AcTypeId = AcTypeID FROM dbo.tblAircraft WHERE AcID = @AcID;

    IF @AcTypeId IS NULL
    BEGIN
        RAISERROR('Aircraft AcID=%d not found.', 16, 1, @AcID);
        RETURN;
    END

    -- ── Level 1: Zones ────────────────────────────────────
    -- Insert Zone rows that don't already exist on this tail
    INSERT INTO mro2.AcPosition (
        AcID, AcPositionTemplateId, ParentAcPositionId,
        PositionLevel, PositionCode, Description,
        ATAId, Quantity, IsInterchangeable,
        IsOverride, SortOrder, CreatedByUserId)
    SELECT
        @AcID, t.AcPositionTemplateId, NULL,
        t.PositionLevel, t.PositionCode, t.Description,
        t.ATAId, t.Quantity, t.IsInterchangeable,
        0, t.SortOrder, @UserId
    FROM mro2.AcPositionTemplate t
    WHERE t.AcTypeId      = @AcTypeId
      AND t.PositionLevel = 1
      AND t.IsActive      = 1
      AND NOT EXISTS (
          SELECT 1 FROM mro2.AcPosition ap
          WHERE ap.AcID = @AcID
            AND ap.AcPositionTemplateId = t.AcPositionTemplateId);

    -- ── Level 2: Systems ──────────────────────────────────
    -- Parent must be resolved to the tail's AcPositionId
    INSERT INTO mro2.AcPosition (
        AcID, AcPositionTemplateId, ParentAcPositionId,
        PositionLevel, PositionCode, Description,
        ATAId, Quantity, IsInterchangeable,
        IsOverride, SortOrder, CreatedByUserId)
    SELECT
        @AcID, t.AcPositionTemplateId,
        -- Resolve parent: find the tail's AcPosition for parent template row
        parent_ap.AcPositionId,
        t.PositionLevel, t.PositionCode, t.Description,
        t.ATAId, t.Quantity, t.IsInterchangeable,
        0, t.SortOrder, @UserId
    FROM mro2.AcPositionTemplate t
    INNER JOIN mro2.AcPosition parent_ap
        ON parent_ap.AcID                = @AcID
       AND parent_ap.AcPositionTemplateId = t.ParentTemplatePositionId
    WHERE t.AcTypeId      = @AcTypeId
      AND t.PositionLevel = 2
      AND t.IsActive      = 1
      AND NOT EXISTS (
          SELECT 1 FROM mro2.AcPosition ap
          WHERE ap.AcID = @AcID
            AND ap.AcPositionTemplateId = t.AcPositionTemplateId);

    -- ── Level 3: Slots ────────────────────────────────────
    INSERT INTO mro2.AcPosition (
        AcID, AcPositionTemplateId, ParentAcPositionId,
        PositionLevel, PositionCode, Description,
        ATAId, Quantity, IsInterchangeable,
        IsOverride, SortOrder, CreatedByUserId)
    SELECT
        @AcID, t.AcPositionTemplateId,
        parent_ap.AcPositionId,
        t.PositionLevel, t.PositionCode, t.Description,
        t.ATAId, t.Quantity, t.IsInterchangeable,
        0, t.SortOrder, @UserId
    FROM mro2.AcPositionTemplate t
    INNER JOIN mro2.AcPosition parent_ap
        ON parent_ap.AcID                = @AcID
       AND parent_ap.AcPositionTemplateId = t.ParentTemplatePositionId
    WHERE t.AcTypeId      = @AcTypeId
      AND t.PositionLevel = 3
      AND t.IsActive      = 1
      AND NOT EXISTS (
          SELECT 1 FROM mro2.AcPosition ap
          WHERE ap.AcID = @AcID
            AND ap.AcPositionTemplateId = t.AcPositionTemplateId);

    -- Return summary
    SELECT
        @AcID                               AS AcID,
        @AcTypeId                           AS AcTypeId,
        COUNT(*)                            AS PositionsCopied
    FROM mro2.AcPosition
    WHERE AcID = @AcID;
END
GO

-- ============================================================
-- SP: mro2.usp_AcPosition_List
--    Returns full position tree for one tail.
--    Uses vw_AcPositionTree — includes zone/system/slot path.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPosition_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPosition_List;
GO
CREATE PROCEDURE mro2.usp_AcPosition_List
    @AcID               INT,
    @PositionLevel      TINYINT     = NULL,   -- NULL = all levels
    @IncludeInactive    BIT         = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM mro2.vw_AcPositionTree
    WHERE AcID = @AcID
      AND (@PositionLevel IS NULL OR PositionLevel = @PositionLevel)
      AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY
        ISNULL(ZoneCode,''),
        ISNULL(SystemCode,''),
        SortOrder,
        PositionCode;
END
GO

-- ============================================================
-- SP: mro2.usp_AcPosition_Save
--    Add or update a single position on a tail.
--    Used for tail-specific overrides (IsOverride=1).
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPosition_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPosition_Save;
GO
CREATE PROCEDURE mro2.usp_AcPosition_Save
    @AcPositionId           INT             = NULL,
    @AcID                   INT,
    @AcPositionTemplateId   INT             = NULL,
    @ParentAcPositionId     INT             = NULL,
    @PositionLevel          TINYINT,
    @PositionCode           VARCHAR(50),
    @Description            NVARCHAR(200)   = NULL,
    @ATAId                  INT             = NULL,
    @Quantity               TINYINT         = 1,
    @IsInterchangeable      BIT             = 0,
    @SortOrder              SMALLINT        = 100,
    @UserId                 NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @PositionCode = UPPER(LTRIM(RTRIM(@PositionCode)));

    IF @AcPositionId IS NULL
    BEGIN
        INSERT INTO mro2.AcPosition (
            AcID, AcPositionTemplateId, ParentAcPositionId,
            PositionLevel, PositionCode, Description,
            ATAId, Quantity, IsInterchangeable,
            IsOverride, SortOrder, CreatedByUserId)
        VALUES (
            @AcID, @AcPositionTemplateId, @ParentAcPositionId,
            @PositionLevel, @PositionCode, @Description,
            @ATAId, @Quantity, @IsInterchangeable,
            CASE WHEN @AcPositionTemplateId IS NULL THEN 1 ELSE 0 END,
            @SortOrder, @UserId);
        SELECT SCOPE_IDENTITY() AS AcPositionId;
    END
    ELSE
    BEGIN
        UPDATE mro2.AcPosition SET
            ParentAcPositionId  = @ParentAcPositionId,
            PositionLevel       = @PositionLevel,
            PositionCode        = @PositionCode,
            Description         = @Description,
            ATAId               = @ATAId,
            Quantity            = @Quantity,
            IsInterchangeable   = @IsInterchangeable,
            SortOrder           = @SortOrder
        WHERE AcPositionId = @AcPositionId;
        SELECT @AcPositionId AS AcPositionId;
    END
END
GO

-- ============================================================
-- SP: mro2.usp_AcPosition_SetActive
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPosition_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPosition_SetActive;
GO
CREATE PROCEDURE mro2.usp_AcPosition_SetActive
    @AcPositionId INT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.AcPosition SET IsActive=@IsActive
    WHERE AcPositionId=@AcPositionId;
END
GO

-- ============================================================
-- SP: mro2.usp_AcPositionPN_List
--    All allowed PNs for a template position.
--    Used by RecordEvent install validation.
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPositionPN_List','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPositionPN_List;
GO
CREATE PROCEDURE mro2.usp_AcPositionPN_List
    @AcPositionTemplateId   INT,
    @IncludeInactive        BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pp.AcPositionPNId,
        pp.AcPositionTemplateId,
        pp.PartNumberId,
        pn.PN,
        pn.Nomenclature,
        pp.IsPrimary,
        pp.Notes,
        pp.IsActive
    FROM mro2.AcPositionPN pp
    INNER JOIN mro2.PartNumber pn ON pn.PartNumberId = pp.PartNumberId
    WHERE pp.AcPositionTemplateId = @AcPositionTemplateId
      AND (@IncludeInactive=1 OR pp.IsActive=1)
    ORDER BY pp.IsPrimary DESC, pn.PN;
END
GO

-- ============================================================
-- SP: mro2.usp_AcPositionPN_Save
-- ============================================================
IF OBJECT_ID('mro2.usp_AcPositionPN_Save','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPositionPN_Save;
GO
CREATE PROCEDURE mro2.usp_AcPositionPN_Save
    @AcPositionPNId         INT             = NULL,
    @AcPositionTemplateId   INT,
    @PartNumberId           INT,
    @IsPrimary              BIT             = 1,
    @Notes                  NVARCHAR(200)   = NULL,
    @UserId                 NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF @AcPositionPNId IS NULL
    BEGIN
        INSERT INTO mro2.AcPositionPN (
            AcPositionTemplateId, PartNumberId,
            IsPrimary, Notes, CreatedByUserId)
        VALUES (
            @AcPositionTemplateId, @PartNumberId,
            @IsPrimary, @Notes, @UserId);
        SELECT SCOPE_IDENTITY() AS AcPositionPNId;
    END
    ELSE
    BEGIN
        UPDATE mro2.AcPositionPN SET
            IsPrimary = @IsPrimary,
            Notes     = @Notes
        WHERE AcPositionPNId = @AcPositionPNId;
        SELECT @AcPositionPNId AS AcPositionPNId;
    END
END
GO

IF OBJECT_ID('mro2.usp_AcPositionPN_SetActive','P') IS NOT NULL
    DROP PROCEDURE mro2.usp_AcPositionPN_SetActive;
GO
CREATE PROCEDURE mro2.usp_AcPositionPN_SetActive
    @AcPositionPNId INT, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mro2.AcPositionPN SET IsActive=@IsActive
    WHERE AcPositionPNId=@AcPositionPNId;
END
GO

-- ============================================================
-- STEP 07 VERIFICATION
-- ============================================================
/*
-- Tables created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='mro2'
  AND TABLE_NAME IN ('AcPositionTemplate','AcPosition','AcPositionPN')
ORDER BY TABLE_NAME;

-- View created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA='mro2' AND TABLE_NAME='vw_AcPositionTree';

-- SPs created (expect 9)
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA='mro2'
  AND ROUTINE_NAME IN (
    'usp_AcPositionTemplate_List',
    'usp_AcPositionTemplate_Save',
    'usp_AcPosition_CopyFromTemplate',
    'usp_AcPosition_List',
    'usp_AcPosition_Save',
    'usp_AcPosition_SetActive',
    'usp_AcPositionPN_List',
    'usp_AcPositionPN_Save',
    'usp_AcPositionPN_SetActive')
ORDER BY ROUTINE_NAME;

-- ── SAMPLE USAGE ──────────────────────────────────────────
-- 1. Build template for AcTypeId=1 (e.g. F-16C)
-- Zone level
-- EXEC mro2.usp_AcPositionTemplate_Save
--     @AcTypeId=1, @PositionLevel=1,
--     @PositionCode='LH-MLG', @Description='Left Main Landing Gear',
--     @ATAId=3, @SortOrder=10, @UserId='admin';
-- EXEC mro2.usp_AcPositionTemplate_Save
--     @AcTypeId=1, @PositionLevel=1,
--     @PositionCode='ENG-1', @Description='Engine 1',
--     @ATAId=12, @SortOrder=20, @UserId='admin';

-- System level (ParentTemplatePositionId = Zone ID from above)
-- EXEC mro2.usp_AcPositionTemplate_Save
--     @AcTypeId=1, @ParentTemplatePositionId=1,
--     @PositionLevel=2, @PositionCode='LH-MLG-TIRE',
--     @Description='LH MLG Tire Assembly',
--     @ATAId=3, @SortOrder=10, @UserId='admin';

-- Slot level
-- EXEC mro2.usp_AcPositionTemplate_Save
--     @AcTypeId=1, @ParentTemplatePositionId=3,
--     @PositionLevel=3, @PositionCode='LH-MLG-TIRE-1',
--     @Description='LH MLG Tire Position 1',
--     @ATAId=3, @Quantity=1, @SortOrder=10, @UserId='admin';

-- 2. Copy template to tail (AcID=5, TailNo=201)
-- EXEC mro2.usp_AcPosition_CopyFromTemplate @AcID=5, @UserId='admin';

-- 3. Verify tail tree
-- EXEC mro2.usp_AcPosition_List @AcID=5;

-- 4. Link PN to position
-- EXEC mro2.usp_AcPositionPN_Save
--     @AcPositionTemplateId=4, @PartNumberId=12,
--     @IsPrimary=1, @UserId='admin';
*/

PRINT '── Step 07 complete ─────────────────────────────────────';
