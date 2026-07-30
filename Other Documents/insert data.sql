USE [SQLFRA]
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
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (1, N'Fighter', N'Fighter')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (2, N'Transport', N'Transport')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (3, N'HELICOPTER', N'Helicopter')
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
INSERT [dbo].[Squadrons] ([Id], [Name], [CallSign], [LogoPath], [FrenchName], [CallSignShort], [WingId], [Active]) VALUES (2, N'EDA', N'BORAK', N'/uploads/squadrons/aaa01ab7-e0a1-4c25-9d49-dc189626e89c.jpg', N'Escadron de defense aerienne', N'BRK', 1, 1)
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
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (1, 2, NULL, 3, CAST(N'2025-12-14' AS Date), N'North', N'Training', N'D-9', N'Planned', CAST(N'10:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2025-12-14T12:46:53.1045589' AS DateTime2), NULL)
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (11, 2, NULL, 4, CAST(N'2025-12-14' AS Date), N'North', N'Training', N'D-11', N'Planned', CAST(N'08:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2025-12-14T18:08:47.6647344' AS DateTime2), CAST(N'2025-12-15T09:06:28.1150172' AS DateTime2))
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (12, 2, NULL, 2, CAST(N'2025-12-14' AS Date), N'North', N'Training', N'D-11', N'Planned', CAST(N'08:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2025-12-14T18:10:55.5494389' AS DateTime2), CAST(N'2025-12-15T08:49:27.0603456' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[Odvs] OFF
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
SET IDENTITY_INSERT [dbo].[AcTypes] ON 
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [MaxEngines], [AcMainGroupId], [SeatCount]) VALUES (1, N'F-5E', N'F-5E Single seat', 0, 1, 2, 1, 1)
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [MaxEngines], [AcMainGroupId], [SeatCount]) VALUES (2, N'F-5F', N'F-5F Duel Seats', 0, 2, 2, 1, 1)
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [MaxEngines], [AcMainGroupId], [SeatCount]) VALUES (3, N'A-JET', N'A-JET', 0, 2, 2, 2, 1)
GO
SET IDENTITY_INSERT [dbo].[AcTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Aircrafts] ON 
GO
INSERT [dbo].[Aircrafts] ([Id], [TailNo], [Registration], [SerialNumber], [Manufacturer], [Model], [ManufactureDate], [IntCode], [Obs], [Active], [Serviceable], [AcTypeId], [AcStatusTypeId], [Status]) VALUES (1, 940, N'F-5-940', N'F-5E-940', N'NORTHROP', N'F Single seat', CAST(N'1978-06-01T00:00:00.0000000' AS DateTime2), N'CN-CNCA', N'XI-Type', 1, 1, 1, 1, 0)
GO
SET IDENTITY_INSERT [dbo].[Aircrafts] OFF
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'0afd67fd-fb0a-4fe0-8ec2-d9d828f48533', N'Admin', N'ADMIN', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'33481d97-8d41-4514-880f-51155210d91d', N'Maintenance', N'MAINTENANCE', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'3f0aa96b-d769-48c3-8a02-ed17dcf199b8', N'SquadronPlanner', N'SQUADRONPLANNER', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'7d9a8fc0-b73c-4967-ad2e-fd4d4f15bac8', N'HR', N'HR', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'926b49eb-b9b1-4ab8-8999-3676c0331da9', N'Trainer', N'TRAINER', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'9b6a4e5c-06df-440a-a1d9-bb583f266239', N'Tower', N'TOWER', NULL)
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'dd5a4ade-1ad8-4034-94e2-8c53ab19bd84', N'CrewChief', N'CREWCHIEF', NULL)
GO
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BaseId], [WingId], [DepartmentId], [SquadronId], [AcMainGroupId], [JobTitle], [EmployeeNumber], [TimeZone], [Locale], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [HireDate], [TerminationDate], [LastLoginUtc], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'1f8f07c1-b929-48fa-8a75-b7a6da150e6d', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, CAST(N'2025-12-13T20:58:08.6681107' AS DateTime2), NULL, NULL, NULL, NULL, N'admin@example.com', N'ADMIN@EXAMPLE.COM', N'admin@example.com', N'ADMIN@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAEJ9NJ/jRn1XReWVhN7ljWrzBiy5Rpa8BmeNUaqaBg0Tmgne1yLHZnK7yLXbXZEob5A==', N'KVRGSGIWT456OUH3LWAMCJGBXZ2TY6R4', N'5014c5dd-d514-4340-8e38-b69a19209b72', NULL, 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BaseId], [WingId], [DepartmentId], [SquadronId], [AcMainGroupId], [JobTitle], [EmployeeNumber], [TimeZone], [Locale], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [HireDate], [TerminationDate], [LastLoginUtc], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'aba68a64-2d21-49ea-b93d-36abef6ce397', N'Dadda', N'fadwa', 2, 1, 1, 2, 1, NULL, NULL, NULL, NULL, 1, CAST(N'2025-12-14T15:31:00.9974046' AS DateTime2), NULL, NULL, NULL, NULL, N'ziyad@example.com', N'ZIYAD@EXAMPLE.COM', N'ziyad@example.com', N'ZIYAD@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAEG97RTiuh+nETUByfR+yr5S+VcpNJXUjRPvOuel50IBhTjLFh82wg3NIVruXwE5BZA==', N'JPKBS56ADURQWPEX4WAKMUEIMPXXLCJY', N'40ca994c-5db3-4571-af6b-966a5f64c825', N'0667082117', 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'1f8f07c1-b929-48fa-8a75-b7a6da150e6d', N'0afd67fd-fb0a-4fe0-8ec2-d9d828f48533')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'aba68a64-2d21-49ea-b93d-36abef6ce397', N'3f0aa96b-d769-48c3-8a02-ed17dcf199b8')
GO
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] ON 
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (6, N'aba68a64-2d21-49ea-b93d-36abef6ce397', N'BaseId', N'2')
GO
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] OFF
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251210103832_Initial_SQLFRA', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251213205601_Initial-Create', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251215104628_Plan_Sortie', N'8.0.0')
GO
SET IDENTITY_INSERT [dbo].[MenuItems] ON 
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (1, N'Squadron', N'fa fa-fighter-jet', NULL, NULL, NULL, NULL, 100, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (2, N'CrewChief', N'fas fa-user-cog', NULL, NULL, NULL, NULL, 200, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (3, N'Aircraft', N'fa fa-plane', NULL, NULL, NULL, NULL, 300, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (4, N'ODV planning', N'fa fa-fighter-jet', N'OdvPlanning', N'Index', NULL, 1, 10, NULL, NULL, N'Admin, SquadronPlanner')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles]) VALUES (5, N'Missions List', N'fas fa-dot-circle', N'Mission', N'Index', NULL, 1, 20, NULL, NULL, N'Admin')
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
