USE [2BA_12]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcCategories]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcCategories](
	[AcCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Description] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_AcCategories] PRIMARY KEY CLUSTERED 
(
	[AcCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcMainGroups]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcMainGroups](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](50) NULL,
	[Active] [bit] NOT NULL,
	[AcCategoryId] [int] NOT NULL,
	[BaseId] [int] NOT NULL,
 CONSTRAINT [PK_AcMainGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcStatusTypes]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcStatusTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StatusName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](100) NULL,
 CONSTRAINT [PK_AcStatusTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcTypes]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[MaxGrossweight] [float] NOT NULL,
	[MaxPassengers] [int] NOT NULL,
	[SeatCount] [int] NOT NULL,
	[MaxEngines] [int] NOT NULL,
	[AcMainGroupId] [int] NOT NULL,
 CONSTRAINT [PK_AcTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Aircrafts]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Aircrafts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TailNo] [int] NOT NULL,
	[Registration] [nvarchar](50) NOT NULL,
	[SerialNumber] [nvarchar](100) NULL,
	[Manufacturer] [nvarchar](100) NULL,
	[Model] [nvarchar](50) NULL,
	[ManufactureDate] [datetime2](7) NULL,
	[IntCode] [nvarchar](10) NULL,
	[Obs] [nvarchar](max) NULL,
	[Active] [bit] NOT NULL,
	[Serviceable] [bit] NOT NULL,
	[AcTypeId] [int] NOT NULL,
	[AcStatusTypeId] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[RowVersion] [timestamp] NULL,
 CONSTRAINT [PK_Aircrafts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
	[BaseId] [int] NULL,
	[WingId] [int] NULL,
	[DepartmentId] [int] NULL,
	[SquadronId] [int] NULL,
	[AcMainGroupId] [int] NULL,
	[JobTitle] [nvarchar](max) NULL,
	[EmployeeNumber] [nvarchar](max) NULL,
	[TimeZone] [nvarchar](max) NULL,
	[Locale] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NULL,
	[HireDate] [datetime2](7) NULL,
	[TerminationDate] [datetime2](7) NULL,
	[LastLoginUtc] [datetime2](7) NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bases]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bases](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BaseName] [nvarchar](100) NOT NULL,
	[BaseNameLocal] [nvarchar](100) NULL,
 CONSTRAINT [PK_Bases] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CallSigns]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CallSigns](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](20) NOT NULL,
	[Description] [nvarchar](250) NULL,
	[BaseId] [int] NULL,
	[SquadronId] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[UpdatedAtUtc] [datetime2](7) NULL,
	[UpdatedBy] [nvarchar](max) NULL,
 CONSTRAINT [PK_CallSigns] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CrewMemberQualifications]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CrewMemberQualifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CrewMemberId] [int] NOT NULL,
	[QualificationId] [int] NOT NULL,
	[ValidFrom] [datetime2](7) NULL,
	[ValidUntil] [datetime2](7) NULL,
	[IssuedBy] [nvarchar](100) NULL,
	[Remarks] [nvarchar](255) NULL,
	[Status] [nvarchar](20) NULL,
 CONSTRAINT [PK_CrewMemberQualifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CrewMembers]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CrewMembers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SequenceNo] [int] NULL,
	[Captain] [nvarchar](30) NOT NULL,
	[NickName] [nvarchar](10) NOT NULL,
	[Role] [nvarchar](50) NULL,
	[Photo] [nvarchar](255) NULL,
	[Active] [bit] NOT NULL,
	[Mobile] [nvarchar](max) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[AllowedToSign] [bit] NOT NULL,
	[CrewMemberType] [nvarchar](20) NOT NULL,
	[SquadronId] [int] NOT NULL,
	[PersonId] [int] NOT NULL,
	[PrimaryQualificationId] [int] NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NULL,
 CONSTRAINT [PK_CrewMembers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Departments]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](150) NULL,
	[BaseId] [int] NOT NULL,
 CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FlightLogs]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FlightLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SortieId] [int] NOT NULL,
	[AircraftId] [int] NOT NULL,
	[TakeOffUtc] [datetime2](7) NULL,
	[LandingUtc] [datetime2](7) NULL,
	[DurationMinutes] [int] NULL,
	[Cycles] [int] NOT NULL,
	[HobbsStart] [decimal](8, 2) NULL,
	[HobbsEnd] [decimal](8, 2) NULL,
	[TachStart] [decimal](8, 2) NULL,
	[TachEnd] [decimal](8, 2) NULL,
	[FuelUsedKg] [decimal](10, 2) NULL,
	[MissionSnapshot] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
 CONSTRAINT [PK_FlightLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MaintenanceComponents]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MaintenanceComponents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AircraftId] [int] NOT NULL,
	[PartNumber] [nvarchar](max) NOT NULL,
	[SerialNumber] [nvarchar](max) NOT NULL,
	[TotalMinutes] [int] NOT NULL,
	[TotalCycles] [int] NOT NULL,
	[LastUpdatedUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MaintenanceComponents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MaintenanceThresholds]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MaintenanceThresholds](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ComponentId] [int] NOT NULL,
	[ThresholdType] [nvarchar](max) NOT NULL,
	[Value] [int] NOT NULL,
	[Repeatable] [bit] NOT NULL,
	[LastTriggeredUtc] [datetime2](7) NULL,
 CONSTRAINT [PK_MaintenanceThresholds] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MaintenanceWorkOrders]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MaintenanceWorkOrders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AircraftId] [int] NOT NULL,
	[ComponentId] [int] NULL,
	[ThresholdId] [int] NULL,
	[Title] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NOT NULL,
	[TriggeredTotalMinutes] [int] NOT NULL,
	[TriggeredTotalCycles] [int] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MaintenanceWorkOrders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicalBilans]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalBilans](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MedicalCheckId] [int] NOT NULL,
	[CheckDate] [datetime2](7) NOT NULL,
	[BilanType] [nvarchar](100) NOT NULL,
	[Instructions] [nvarchar](500) NULL,
	[FollowUpMonths] [int] NULL,
	[FollowUpDays] [int] NULL,
	[IsCompleted] [bit] NOT NULL,
	[CompletedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_MedicalBilans] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicalChecks]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalChecks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CrewMemberId] [int] NOT NULL,
	[BaseId] [int] NOT NULL,
	[CheckType] [int] NOT NULL,
	[CheckDate] [datetime2](7) NOT NULL,
	[Decision] [int] NOT NULL,
	[DecisionText] [nvarchar](100) NULL,
	[Derogation] [bit] NOT NULL,
	[NextDueDate] [datetime2](7) NULL,
	[NextVuDate] [datetime2](7) NULL,
	[LateCheckReason] [nvarchar](300) NULL,
	[OBESITE] [bit] NOT NULL,
	[C_Optique] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[UpdatedBy] [nvarchar](100) NULL,
	[RowVersion] [timestamp] NULL,
	[DurationYears] [int] NOT NULL,
	[DurationDays] [int] NOT NULL,
	[DurationMonths] [int] NOT NULL,
 CONSTRAINT [PK_MedicalChecks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MenuItems]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MenuItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[IconClass] [nvarchar](200) NULL,
	[Controller] [nvarchar](100) NULL,
	[Action] [nvarchar](100) NULL,
	[Url] [nvarchar](500) NULL,
	[ParentId] [int] NULL,
	[SortOrder] [int] NOT NULL,
	[DepartmentId] [int] NULL,
	[BaseId] [int] NULL,
	[Roles] [nvarchar](200) NULL,
	[Area] [nvarchar](max) NULL,
 CONSTRAINT [PK_MenuItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Missions]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Missions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](50) NULL,
	[PhaseId] [int] NOT NULL,
	[SquadronId] [int] NULL,
	[PlannedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[Description] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Missions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Odvs]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Odvs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SquadronId] [int] NOT NULL,
	[BaseId] [int] NULL,
	[MissionId] [int] NOT NULL,
	[OdvDate] [date] NOT NULL,
	[Zone] [nvarchar](50) NOT NULL,
	[MissionType] [nvarchar](50) NOT NULL,
	[Area] [nvarchar](200) NOT NULL,
	[OdvStatus] [nvarchar](50) NULL,
	[TOFF] [time](7) NULL,
	[Obs] [nvarchar](2000) NULL,
	[AcMainGroupId] [int] NOT NULL,
	[CallSignId] [int] NOT NULL,
	[IsPreflightApproved] [bit] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NULL,
 CONSTRAINT [PK_Odvs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Persons]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Persons](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RankId] [int] NOT NULL,
	[Matricule] [nvarchar](20) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Gender] [nvarchar](10) NULL,
	[SubDepartmentId] [int] NOT NULL,
	[DateOfBirth] [datetime2](7) NULL,
	[NationalId] [nvarchar](20) NULL,
	[Speciality] [nvarchar](100) NULL,
	[City] [nvarchar](100) NULL,
	[Country] [nvarchar](100) NULL,
	[Active] [bit] NOT NULL,
	[PatrimonialStatus] [nvarchar](50) NULL,
	[Photo] [varbinary](max) NULL,
 CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Phases]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phases](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](250) NULL,
 CONSTRAINT [PK_Phases] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Qualifications]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Qualifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[QualificationType] [nvarchar](20) NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Qualifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ranks]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ranks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](80) NOT NULL,
	[FullRank] [nvarchar](150) NOT NULL,
	[Sequence] [int] NOT NULL,
	[RankTypeId] [int] NOT NULL,
 CONSTRAINT [PK_Ranks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RankTypes]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RankTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](300) NULL,
 CONSTRAINT [PK_RankTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SortieCrews]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SortieCrews](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SortieId] [int] NOT NULL,
	[CrewMemberId] [int] NOT NULL,
	[Seat] [int] NOT NULL,
	[Role] [nvarchar](100) NULL,
	[IsPrimary] [bit] NOT NULL,
	[Remarks] [nvarchar](1000) NULL,
	[AircraftRole] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SortieCrews] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sorties]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sorties](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OdvId] [int] NOT NULL,
	[BaseId] [int] NULL,
	[AcTypeId] [int] NOT NULL,
	[AircraftId] [int] NULL,
	[SortieCode] [nvarchar](max) NOT NULL,
	[Configuration] [nvarchar](200) NULL,
	[Sequence] [int] NOT NULL,
	[FuelQuantity] [decimal](12, 2) NULL,
	[StartTime] [datetime2](7) NULL,
	[LandingTime] [datetime2](7) NULL,
	[TOFF] [time](7) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[RealTOFF] [datetime2](7) NULL,
	[RealLandingTime] [datetime2](7) NULL,
	[Notes] [nvarchar](2000) NULL,
	[DayHours] [float] NULL,
	[NightHours] [float] NULL,
	[DurationMinutes] [int] NULL,
	[Approachs] [int] NULL,
	[Landings] [int] NULL,
	[TGOsLandings] [int] NULL,
	[HobbsStart] [float] NULL,
	[HobbsEnd] [float] NULL,
	[HobbsUsed] [float] NULL,
	[TachStart] [float] NULL,
	[TachEnd] [float] NULL,
	[TachUsed] [float] NULL,
	[AirframeHours] [float] NULL,
	[AirframeCycles] [float] NULL,
	[InstSimulated] [float] NULL,
	[InstActual] [float] NULL,
	[IFRHours] [float] NULL,
	[Cycles] [int] NULL,
	[FuelUsedLiters] [decimal](12, 2) NULL,
	[Malfunctions] [nvarchar](max) NULL,
	[IsCompleted] [bit] NOT NULL,
	[IsFinalized] [bit] NULL,
	[BrakeChuteUsed] [bit] NULL,
	[Interceptions] [int] NULL,
	[RadarContacts] [int] NULL,
	[AppContacts] [int] NULL,
	[SquadronReportNotes] [nvarchar](max) NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](200) NULL,
	[UpdatedAtUtc] [datetime2](7) NULL,
	[UpdatedBy] [nvarchar](200) NULL,
	[CompletedAtUtc] [datetime2](7) NULL,
	[CompletedBy] [nvarchar](200) NULL,
	[FinalizedAtUtc] [datetime2](7) NULL,
	[FinalizedBy] [nvarchar](max) NULL,
	[RowVersion] [timestamp] NULL,
 CONSTRAINT [PK_Sorties] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Squadrons]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Squadrons](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[CallSign] [nvarchar](20) NULL,
	[LogoPath] [nvarchar](100) NULL,
	[FrenchName] [nvarchar](40) NULL,
	[CallSignShort] [nvarchar](10) NULL,
	[WingId] [int] NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Squadrons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubDepartments]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubDepartments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DepartmentId] [int] NOT NULL,
 CONSTRAINT [PK_SubDepartments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserDocuments]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserDocuments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NOT NULL,
	[DocumentType] [nvarchar](max) NOT NULL,
	[FileName] [nvarchar](max) NOT NULL,
	[ContentType] [nvarchar](max) NOT NULL,
	[FileSizeBytes] [bigint] NOT NULL,
	[StorageKey] [nvarchar](max) NOT NULL,
	[UploadedAtUtc] [datetime2](7) NOT NULL,
	[ExpiresAtUtc] [datetime2](7) NULL,
	[IsVerified] [bit] NOT NULL,
	[Notes] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserDocuments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserQualifications]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserQualifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NOT NULL,
	[AcTypeId] [int] NULL,
	[QualificationType] [nvarchar](max) NOT NULL,
	[IssuedAtUtc] [datetime2](7) NOT NULL,
	[ExpiresAtUtc] [datetime2](7) NULL,
	[IssuedByUserId] [int] NULL,
	[DocumentId] [int] NULL,
	[Notes] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserQualifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Wings]    Script Date: 18-Apr-26 18:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Wings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[WingLong] [nvarchar](max) NOT NULL,
	[DepartmentId] [int] NOT NULL,
	[AcMainGroupId] [int] NULL,
	[BaseId] [int] NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Wings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[CallSigns] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [dbo].[MedicalChecks] ADD  DEFAULT ((0)) FOR [Decision]
GO
ALTER TABLE [dbo].[MedicalChecks] ADD  DEFAULT (CONVERT([bit],(0))) FOR [OBESITE]
GO
ALTER TABLE [dbo].[MedicalChecks] ADD  DEFAULT (CONVERT([bit],(0))) FOR [C_Optique]
GO
ALTER TABLE [dbo].[MedicalChecks] ADD  DEFAULT ((0)) FOR [DurationYears]
GO
ALTER TABLE [dbo].[MedicalChecks] ADD  DEFAULT ((0)) FOR [DurationDays]
GO
ALTER TABLE [dbo].[MedicalChecks] ADD  DEFAULT ((0)) FOR [DurationMonths]
GO
ALTER TABLE [dbo].[Missions] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [dbo].[Odvs] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsPreflightApproved]
GO
ALTER TABLE [dbo].[SortieCrews] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[AcMainGroups]  WITH CHECK ADD  CONSTRAINT [FK_AcMainGroups_AcCategories_AcCategoryId] FOREIGN KEY([AcCategoryId])
REFERENCES [dbo].[AcCategories] ([AcCategoryId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AcMainGroups] CHECK CONSTRAINT [FK_AcMainGroups_AcCategories_AcCategoryId]
GO
ALTER TABLE [dbo].[AcMainGroups]  WITH CHECK ADD  CONSTRAINT [FK_AcMainGroups_Bases_BaseId] FOREIGN KEY([BaseId])
REFERENCES [dbo].[Bases] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AcMainGroups] CHECK CONSTRAINT [FK_AcMainGroups_Bases_BaseId]
GO
ALTER TABLE [dbo].[AcTypes]  WITH CHECK ADD  CONSTRAINT [FK_AcTypes_AcMainGroups_AcMainGroupId] FOREIGN KEY([AcMainGroupId])
REFERENCES [dbo].[AcMainGroups] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AcTypes] CHECK CONSTRAINT [FK_AcTypes_AcMainGroups_AcMainGroupId]
GO
ALTER TABLE [dbo].[Aircrafts]  WITH CHECK ADD  CONSTRAINT [FK_Aircrafts_AcStatusTypes_AcStatusTypeId] FOREIGN KEY([AcStatusTypeId])
REFERENCES [dbo].[AcStatusTypes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Aircrafts] CHECK CONSTRAINT [FK_Aircrafts_AcStatusTypes_AcStatusTypeId]
GO
ALTER TABLE [dbo].[Aircrafts]  WITH CHECK ADD  CONSTRAINT [FK_Aircrafts_AcTypes_AcTypeId] FOREIGN KEY([AcTypeId])
REFERENCES [dbo].[AcTypes] ([Id])
GO
ALTER TABLE [dbo].[Aircrafts] CHECK CONSTRAINT [FK_Aircrafts_AcTypes_AcTypeId]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CallSigns]  WITH CHECK ADD  CONSTRAINT [FK_CallSigns_Bases_BaseId] FOREIGN KEY([BaseId])
REFERENCES [dbo].[Bases] ([Id])
GO
ALTER TABLE [dbo].[CallSigns] CHECK CONSTRAINT [FK_CallSigns_Bases_BaseId]
GO
ALTER TABLE [dbo].[CallSigns]  WITH CHECK ADD  CONSTRAINT [FK_CallSigns_Squadrons_SquadronId] FOREIGN KEY([SquadronId])
REFERENCES [dbo].[Squadrons] ([Id])
GO
ALTER TABLE [dbo].[CallSigns] CHECK CONSTRAINT [FK_CallSigns_Squadrons_SquadronId]
GO
ALTER TABLE [dbo].[CrewMemberQualifications]  WITH CHECK ADD  CONSTRAINT [FK_CrewMemberQualifications_CrewMembers_CrewMemberId] FOREIGN KEY([CrewMemberId])
REFERENCES [dbo].[CrewMembers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CrewMemberQualifications] CHECK CONSTRAINT [FK_CrewMemberQualifications_CrewMembers_CrewMemberId]
GO
ALTER TABLE [dbo].[CrewMemberQualifications]  WITH CHECK ADD  CONSTRAINT [FK_CrewMemberQualifications_Qualifications_QualificationId] FOREIGN KEY([QualificationId])
REFERENCES [dbo].[Qualifications] ([Id])
GO
ALTER TABLE [dbo].[CrewMemberQualifications] CHECK CONSTRAINT [FK_CrewMemberQualifications_Qualifications_QualificationId]
GO
ALTER TABLE [dbo].[CrewMembers]  WITH CHECK ADD  CONSTRAINT [FK_CrewMembers_Persons_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Persons] ([Id])
GO
ALTER TABLE [dbo].[CrewMembers] CHECK CONSTRAINT [FK_CrewMembers_Persons_PersonId]
GO
ALTER TABLE [dbo].[CrewMembers]  WITH CHECK ADD  CONSTRAINT [FK_CrewMembers_Qualifications_PrimaryQualificationId] FOREIGN KEY([PrimaryQualificationId])
REFERENCES [dbo].[Qualifications] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[CrewMembers] CHECK CONSTRAINT [FK_CrewMembers_Qualifications_PrimaryQualificationId]
GO
ALTER TABLE [dbo].[CrewMembers]  WITH CHECK ADD  CONSTRAINT [FK_CrewMembers_Squadrons_SquadronId] FOREIGN KEY([SquadronId])
REFERENCES [dbo].[Squadrons] ([Id])
GO
ALTER TABLE [dbo].[CrewMembers] CHECK CONSTRAINT [FK_CrewMembers_Squadrons_SquadronId]
GO
ALTER TABLE [dbo].[Departments]  WITH CHECK ADD  CONSTRAINT [FK_Departments_Bases_BaseId] FOREIGN KEY([BaseId])
REFERENCES [dbo].[Bases] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Departments] CHECK CONSTRAINT [FK_Departments_Bases_BaseId]
GO
ALTER TABLE [dbo].[FlightLogs]  WITH CHECK ADD  CONSTRAINT [FK_FlightLogs_Aircrafts_AircraftId] FOREIGN KEY([AircraftId])
REFERENCES [dbo].[Aircrafts] ([Id])
GO
ALTER TABLE [dbo].[FlightLogs] CHECK CONSTRAINT [FK_FlightLogs_Aircrafts_AircraftId]
GO
ALTER TABLE [dbo].[FlightLogs]  WITH CHECK ADD  CONSTRAINT [FK_FlightLogs_Sorties_SortieId] FOREIGN KEY([SortieId])
REFERENCES [dbo].[Sorties] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FlightLogs] CHECK CONSTRAINT [FK_FlightLogs_Sorties_SortieId]
GO
ALTER TABLE [dbo].[MaintenanceComponents]  WITH CHECK ADD  CONSTRAINT [FK_MaintenanceComponents_Aircrafts_AircraftId] FOREIGN KEY([AircraftId])
REFERENCES [dbo].[Aircrafts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MaintenanceComponents] CHECK CONSTRAINT [FK_MaintenanceComponents_Aircrafts_AircraftId]
GO
ALTER TABLE [dbo].[MaintenanceThresholds]  WITH CHECK ADD  CONSTRAINT [FK_MaintenanceThresholds_MaintenanceComponents_ComponentId] FOREIGN KEY([ComponentId])
REFERENCES [dbo].[MaintenanceComponents] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MaintenanceThresholds] CHECK CONSTRAINT [FK_MaintenanceThresholds_MaintenanceComponents_ComponentId]
GO
ALTER TABLE [dbo].[MaintenanceWorkOrders]  WITH CHECK ADD  CONSTRAINT [FK_MaintenanceWorkOrders_Aircrafts_AircraftId] FOREIGN KEY([AircraftId])
REFERENCES [dbo].[Aircrafts] ([Id])
GO
ALTER TABLE [dbo].[MaintenanceWorkOrders] CHECK CONSTRAINT [FK_MaintenanceWorkOrders_Aircrafts_AircraftId]
GO
ALTER TABLE [dbo].[MaintenanceWorkOrders]  WITH CHECK ADD  CONSTRAINT [FK_MaintenanceWorkOrders_MaintenanceComponents_ComponentId] FOREIGN KEY([ComponentId])
REFERENCES [dbo].[MaintenanceComponents] ([Id])
GO
ALTER TABLE [dbo].[MaintenanceWorkOrders] CHECK CONSTRAINT [FK_MaintenanceWorkOrders_MaintenanceComponents_ComponentId]
GO
ALTER TABLE [dbo].[MaintenanceWorkOrders]  WITH CHECK ADD  CONSTRAINT [FK_MaintenanceWorkOrders_MaintenanceThresholds_ThresholdId] FOREIGN KEY([ThresholdId])
REFERENCES [dbo].[MaintenanceThresholds] ([Id])
GO
ALTER TABLE [dbo].[MaintenanceWorkOrders] CHECK CONSTRAINT [FK_MaintenanceWorkOrders_MaintenanceThresholds_ThresholdId]
GO
ALTER TABLE [dbo].[MedicalBilans]  WITH CHECK ADD  CONSTRAINT [FK_MedicalBilans_MedicalChecks_MedicalCheckId] FOREIGN KEY([MedicalCheckId])
REFERENCES [dbo].[MedicalChecks] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MedicalBilans] CHECK CONSTRAINT [FK_MedicalBilans_MedicalChecks_MedicalCheckId]
GO
ALTER TABLE [dbo].[MedicalChecks]  WITH CHECK ADD  CONSTRAINT [FK_MedicalChecks_CrewMembers_CrewMemberId] FOREIGN KEY([CrewMemberId])
REFERENCES [dbo].[CrewMembers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MedicalChecks] CHECK CONSTRAINT [FK_MedicalChecks_CrewMembers_CrewMemberId]
GO
ALTER TABLE [dbo].[Missions]  WITH CHECK ADD  CONSTRAINT [FK_Missions_Phases_PhaseId] FOREIGN KEY([PhaseId])
REFERENCES [dbo].[Phases] ([Id])
GO
ALTER TABLE [dbo].[Missions] CHECK CONSTRAINT [FK_Missions_Phases_PhaseId]
GO
ALTER TABLE [dbo].[Missions]  WITH CHECK ADD  CONSTRAINT [FK_Missions_Squadrons_SquadronId] FOREIGN KEY([SquadronId])
REFERENCES [dbo].[Squadrons] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Missions] CHECK CONSTRAINT [FK_Missions_Squadrons_SquadronId]
GO
ALTER TABLE [dbo].[Odvs]  WITH CHECK ADD  CONSTRAINT [FK_Odvs_AcMainGroups_AcMainGroupId] FOREIGN KEY([AcMainGroupId])
REFERENCES [dbo].[AcMainGroups] ([Id])
GO
ALTER TABLE [dbo].[Odvs] CHECK CONSTRAINT [FK_Odvs_AcMainGroups_AcMainGroupId]
GO
ALTER TABLE [dbo].[Odvs]  WITH CHECK ADD  CONSTRAINT [FK_Odvs_Bases_BaseId] FOREIGN KEY([BaseId])
REFERENCES [dbo].[Bases] ([Id])
GO
ALTER TABLE [dbo].[Odvs] CHECK CONSTRAINT [FK_Odvs_Bases_BaseId]
GO
ALTER TABLE [dbo].[Odvs]  WITH CHECK ADD  CONSTRAINT [FK_Odvs_CallSigns_CallSignId] FOREIGN KEY([CallSignId])
REFERENCES [dbo].[CallSigns] ([Id])
GO
ALTER TABLE [dbo].[Odvs] CHECK CONSTRAINT [FK_Odvs_CallSigns_CallSignId]
GO
ALTER TABLE [dbo].[Odvs]  WITH CHECK ADD  CONSTRAINT [FK_Odvs_Missions_MissionId] FOREIGN KEY([MissionId])
REFERENCES [dbo].[Missions] ([Id])
GO
ALTER TABLE [dbo].[Odvs] CHECK CONSTRAINT [FK_Odvs_Missions_MissionId]
GO
ALTER TABLE [dbo].[Odvs]  WITH CHECK ADD  CONSTRAINT [FK_Odvs_Squadrons_SquadronId] FOREIGN KEY([SquadronId])
REFERENCES [dbo].[Squadrons] ([Id])
GO
ALTER TABLE [dbo].[Odvs] CHECK CONSTRAINT [FK_Odvs_Squadrons_SquadronId]
GO
ALTER TABLE [dbo].[Persons]  WITH CHECK ADD  CONSTRAINT [FK_Persons_Ranks_RankId] FOREIGN KEY([RankId])
REFERENCES [dbo].[Ranks] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Persons] CHECK CONSTRAINT [FK_Persons_Ranks_RankId]
GO
ALTER TABLE [dbo].[Persons]  WITH CHECK ADD  CONSTRAINT [FK_Persons_SubDepartments_SubDepartmentId] FOREIGN KEY([SubDepartmentId])
REFERENCES [dbo].[SubDepartments] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Persons] CHECK CONSTRAINT [FK_Persons_SubDepartments_SubDepartmentId]
GO
ALTER TABLE [dbo].[Ranks]  WITH CHECK ADD  CONSTRAINT [FK_Ranks_RankTypes_RankTypeId] FOREIGN KEY([RankTypeId])
REFERENCES [dbo].[RankTypes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Ranks] CHECK CONSTRAINT [FK_Ranks_RankTypes_RankTypeId]
GO
ALTER TABLE [dbo].[SortieCrews]  WITH CHECK ADD  CONSTRAINT [FK_SortieCrews_CrewMembers_CrewMemberId] FOREIGN KEY([CrewMemberId])
REFERENCES [dbo].[CrewMembers] ([Id])
GO
ALTER TABLE [dbo].[SortieCrews] CHECK CONSTRAINT [FK_SortieCrews_CrewMembers_CrewMemberId]
GO
ALTER TABLE [dbo].[SortieCrews]  WITH CHECK ADD  CONSTRAINT [FK_SortieCrews_Sorties_SortieId] FOREIGN KEY([SortieId])
REFERENCES [dbo].[Sorties] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SortieCrews] CHECK CONSTRAINT [FK_SortieCrews_Sorties_SortieId]
GO
ALTER TABLE [dbo].[Sorties]  WITH CHECK ADD  CONSTRAINT [FK_Sorties_AcTypes_AcTypeId] FOREIGN KEY([AcTypeId])
REFERENCES [dbo].[AcTypes] ([Id])
GO
ALTER TABLE [dbo].[Sorties] CHECK CONSTRAINT [FK_Sorties_AcTypes_AcTypeId]
GO
ALTER TABLE [dbo].[Sorties]  WITH CHECK ADD  CONSTRAINT [FK_Sorties_Aircrafts_AircraftId] FOREIGN KEY([AircraftId])
REFERENCES [dbo].[Aircrafts] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Sorties] CHECK CONSTRAINT [FK_Sorties_Aircrafts_AircraftId]
GO
ALTER TABLE [dbo].[Sorties]  WITH CHECK ADD  CONSTRAINT [FK_Sorties_Odvs_OdvId] FOREIGN KEY([OdvId])
REFERENCES [dbo].[Odvs] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Sorties] CHECK CONSTRAINT [FK_Sorties_Odvs_OdvId]
GO
ALTER TABLE [dbo].[Squadrons]  WITH CHECK ADD  CONSTRAINT [FK_Squadrons_Wings_WingId] FOREIGN KEY([WingId])
REFERENCES [dbo].[Wings] ([Id])
GO
ALTER TABLE [dbo].[Squadrons] CHECK CONSTRAINT [FK_Squadrons_Wings_WingId]
GO
ALTER TABLE [dbo].[SubDepartments]  WITH CHECK ADD  CONSTRAINT [FK_SubDepartments_Departments_DepartmentId] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Departments] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubDepartments] CHECK CONSTRAINT [FK_SubDepartments_Departments_DepartmentId]
GO
ALTER TABLE [dbo].[Wings]  WITH CHECK ADD  CONSTRAINT [FK_Wings_AcMainGroups_AcMainGroupId] FOREIGN KEY([AcMainGroupId])
REFERENCES [dbo].[AcMainGroups] ([Id])
GO
ALTER TABLE [dbo].[Wings] CHECK CONSTRAINT [FK_Wings_AcMainGroups_AcMainGroupId]
GO
ALTER TABLE [dbo].[Wings]  WITH CHECK ADD  CONSTRAINT [FK_Wings_Bases_BaseId] FOREIGN KEY([BaseId])
REFERENCES [dbo].[Bases] ([Id])
GO
ALTER TABLE [dbo].[Wings] CHECK CONSTRAINT [FK_Wings_Bases_BaseId]
GO
ALTER TABLE [dbo].[Wings]  WITH CHECK ADD  CONSTRAINT [FK_Wings_Departments_DepartmentId] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Departments] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Wings] CHECK CONSTRAINT [FK_Wings_Departments_DepartmentId]
GO
