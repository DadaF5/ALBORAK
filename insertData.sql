USE [2BA]
GO
SET IDENTITY_INSERT [dbo].[Bases] ON 
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseNameLocal]) VALUES (1, N'1°BAFRA', N'SALE')
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseNameLocal]) VALUES (2, N'2°BAFRA', N'MEKNES')
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseNameLocal]) VALUES (3, N'3°BAFRA', N'KENITRA')
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseNameLocal]) VALUES (4, N'4°BAFRA', N'LAYOUNE')
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseNameLocal]) VALUES (5, N'5°BAFRA', N'SIDI SLIMANE')
GO
SET IDENTITY_INSERT [dbo].[Bases] OFF
GO
SET IDENTITY_INSERT [dbo].[Departments] ON 
GO
INSERT [dbo].[Departments] ([Id], [Name], [Description], [BaseId]) VALUES (1, N'CMDMT', N'PC Commandement', 2)
GO
INSERT [dbo].[Departments] ([Id], [Name], [Description], [BaseId]) VALUES (2, N'GAC', N'Groupement Aerien Chasse F-5', 2)
GO
SET IDENTITY_INSERT [dbo].[Departments] OFF
GO
SET IDENTITY_INSERT [dbo].[AcCategories] ON 
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (1, N'Name', N'Description')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (2, N'Fighter', N'Fighter')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (3, N'Transport', N'Transport')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (4, N'HELICOPTER', N'Helicopter')
GO
SET IDENTITY_INSERT [dbo].[AcCategories] OFF
GO
SET IDENTITY_INSERT [dbo].[AcMainGroups] ON 
GO
INSERT [dbo].[AcMainGroups] ([Id], [Name], [Description], [Active], [AcCategoryId], [BaseId]) VALUES (1, N'F-5', N'F-5 Tiger II', 1, 1, 2)
GO
INSERT [dbo].[AcMainGroups] ([Id], [Name], [Description], [Active], [AcCategoryId], [BaseId]) VALUES (2, N'A-Jet', N'A-Jet', 1, 1, 2)
GO
SET IDENTITY_INSERT [dbo].[AcMainGroups] OFF
GO
SET IDENTITY_INSERT [dbo].[Wings] ON 
GO
INSERT [dbo].[Wings] ([Id], [Name], [WingLong], [DepartmentId], [AcMainGroupId], [BaseId], [Active]) VALUES (1, N'ESC-CHASSE', N'ESCADRE DE CHASSE ', 2, 1, 2, 1)
GO
INSERT [dbo].[Wings] ([Id], [Name], [WingLong], [DepartmentId], [AcMainGroupId], [BaseId], [Active]) VALUES (2, N'CIPC', N'Centre d''instruction pilote de combat', 2, 2, 2, 1)
GO
SET IDENTITY_INSERT [dbo].[Wings] OFF
GO
SET IDENTITY_INSERT [dbo].[Squadrons] ON 
GO
INSERT [dbo].[Squadrons] ([Id], [Name], [CallSign], [LogoPath], [FrenchName], [CallSignShort], [WingId], [Active]) VALUES (1, N'EDA', N'BORAK', N'/uploads/squadrons/aaa01ab7-e0a1-4c25-9d49-dc189626e89c.jpg', N'Escadron de defense aerienne', N'BRK', 1, 1)
GO
SET IDENTITY_INSERT [dbo].[Squadrons] OFF
GO
SET IDENTITY_INSERT [dbo].[Phases] ON 
GO
INSERT [dbo].[Phases] ([Id], [Name], [Description]) VALUES (1, N'Planning', N'Planning activities')
GO
INSERT [dbo].[Phases] ([Id], [Name], [Description]) VALUES (2, N'Training', N'Training missions')
GO
INSERT [dbo].[Phases] ([Id], [Name], [Description]) VALUES (3, N'Operational', N'Operational missions')
GO
INSERT [dbo].[Phases] ([Id], [Name], [Description]) VALUES (4, N'Evaluation', N'Evaluation / check rides')
GO
SET IDENTITY_INSERT [dbo].[Phases] OFF
GO
SET IDENTITY_INSERT [dbo].[Missions] ON 
GO
INSERT [dbo].[Missions] ([Id], [Name], [Code], [PhaseId], [SquadronId], [PlannedDate], [IsActive], [Description]) VALUES (1, N'CAP', N'CAP', 3, NULL, NULL, 1, NULL)
GO
INSERT [dbo].[Missions] ([Id], [Name], [Code], [PhaseId], [SquadronId], [PlannedDate], [IsActive], [Description]) VALUES (2, N'DACT', N'DACT', 2, NULL, NULL, 1, NULL)
GO
INSERT [dbo].[Missions] ([Id], [Name], [Code], [PhaseId], [SquadronId], [PlannedDate], [IsActive], [Description]) VALUES (3, N'BFM', N'BFM', 2, NULL, NULL, 1, NULL)
GO
INSERT [dbo].[Missions] ([Id], [Name], [Code], [PhaseId], [SquadronId], [PlannedDate], [IsActive], [Description]) VALUES (4, N'NAV', N'NAV', 2, NULL, NULL, 1, NULL)
GO
INSERT [dbo].[Missions] ([Id], [Name], [Code], [PhaseId], [SquadronId], [PlannedDate], [IsActive], [Description]) VALUES (5, N'A2G', N'A2G', 3, NULL, NULL, 1, NULL)
GO
SET IDENTITY_INSERT [dbo].[Missions] OFF
GO
SET IDENTITY_INSERT [dbo].[CallSigns] ON 
GO
INSERT [dbo].[CallSigns] ([Id], [Code], [Description], [BaseId], [SquadronId], [IsActive], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy]) VALUES (1, N'ROMEO', N'ROMEO', NULL, NULL, 1, CAST(N'2025-12-14T12:46:32.4451202' AS DateTime2), N'admin@example.com', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[CallSigns] OFF
GO
SET IDENTITY_INSERT [dbo].[Odvs] ON 
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (1, 1, NULL, 3, CAST(N'2025-12-15' AS Date), N'North', N'Training', N'D-11', N'Planned', CAST(N'10:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2025-12-15T21:06:13.8415905' AS DateTime2), NULL)
GO
SET IDENTITY_INSERT [dbo].[Odvs] OFF
GO
SET IDENTITY_INSERT [dbo].[AcTypes] ON 
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [MaxEngines], [AcMainGroupId]) VALUES (2, N'F-5E', N'F-5E Single seat', 0, 1, 2, 1)
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [MaxEngines], [AcMainGroupId]) VALUES (3, N'F-5F', N'F-5F Duel Seats', 0, 2, 2, 1)
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [MaxEngines], [AcMainGroupId]) VALUES (4, N'A-JET', N'A-JET', 0, 2, 2, 2)
GO
SET IDENTITY_INSERT [dbo].[AcTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[AcStatusTypes] ON 
GO
INSERT [dbo].[AcStatusTypes] ([Id], [StatusName], [Description]) VALUES (1, N'Serviceable', N'Serviceable')
GO
INSERT [dbo].[AcStatusTypes] ([Id], [StatusName], [Description]) VALUES (2, N'Maintenance', N'Maintenance')
GO
INSERT [dbo].[AcStatusTypes] ([Id], [StatusName], [Description]) VALUES (3, N'Waiting Spare Part', N'Waiting Spare Part')
GO
SET IDENTITY_INSERT [dbo].[AcStatusTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Aircrafts] ON 
GO
INSERT [dbo].[Aircrafts] ([Id], [TailNo], [Registration], [SerialNumber], [Manufacturer], [Model], [ManufactureDate], [IntCode], [Obs], [Active], [Serviceable], [AcTypeId], [AcStatusTypeId], [Status]) VALUES (4, 940, N'F-5-940', N'F-5E-940', N'NORTHROP', N'F Single seat', CAST(N'1978-06-01T00:00:00.0000000' AS DateTime2), N'CN-CNCA', N'XI-Type', 1, 1, 2, 1, 1)
GO
SET IDENTITY_INSERT [dbo].[Aircrafts] OFF
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'348f318f-3dda-43d0-a574-35cb447d5781', N'Maintenance', N'MAINTENANCE', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'36662047-5961-43f6-a08d-72a631f15d63', N'Tower', N'TOWER', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'5495aff7-0c96-4dda-ad24-79782a31a13c', N'Trainer', N'TRAINER', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'a15a7097-1e61-4e97-aaf0-efea9d157ecf', N'SquadronPlanner', N'SQUADRONPLANNER', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'c366244c-44eb-4b57-bdbc-19eaa65a006b', N'HR', N'HR', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'c3a8cd93-f2a2-47e1-9190-60935ae72242', N'Admin', N'ADMIN', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'fc378a93-59bf-4adf-b489-fb14af9858f3', N'CrewChief', N'CREWCHIEF', NULL)
GO
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BaseId], [WingId], [DepartmentId], [SquadronId], [AcMainGroupId], [JobTitle], [EmployeeNumber], [TimeZone], [Locale], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [HireDate], [TerminationDate], [LastLoginUtc], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'c4e38a7b-9745-426e-9fec-196482f2a290', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, CAST(N'2025-12-15T21:00:30.1085150' AS DateTime2), NULL, NULL, NULL, NULL, N'admin@example.com', N'ADMIN@EXAMPLE.COM', N'admin@example.com', N'ADMIN@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAEMVCqNy++pQo3U1SZEREa+OW4iny8KDi9JI8o5MlKysHznxtlOJg6uGJjMBmCWYxOg==', N'O4OCFXVYTIS4VKFGDASS2A6QAVHHZ2XK', N'a09b537c-bab7-4635-a9de-ae7629f4e510', NULL, 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c4e38a7b-9745-426e-9fec-196482f2a290', N'c3a8cd93-f2a2-47e1-9190-60935ae72242')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251215201110_Initial_Create', N'8.0.0')
GO
SET IDENTITY_INSERT [dbo].[MenuItems] ON 
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (1, N'Squadron', N'fa fa-fighter-jet', NULL, NULL, NULL, NULL, 100, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (2, N'CrewChief', N'fas fa-user-cog', NULL, NULL, NULL, NULL, 200, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (3, N'Aircraft', N'fa fa-plane', NULL, NULL, NULL, NULL, 300, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (4, N'Create ODV', NULL, N'OdvPlanning', N'Index', NULL, 1, 10, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (5, N'Pilot Logbook', NULL, N'PilotLog', N'Index', NULL, 1, 20, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (6, N'Update Sortie', NULL, N'Sortie', N'Edit', NULL, 1, 30, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (7, N'Assign Aircraft', NULL, N'CrewChief', N'AssignAircraft', NULL, 2, 10, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (8, N'Report Malfunction', NULL, N'CrewChief', N'ReportMalfunction', NULL, 2, 20, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (9, N'Maintenance Log', NULL, N'CrewChief', N'MaintenanceLog', NULL, 2, 30, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (10, N'List', NULL, N'Aircraft', N'Index', NULL, 3, 10, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (11, N'Create', NULL, N'Aircraft', N'Create', NULL, 3, 20, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[MenuItems] OFF
GO
