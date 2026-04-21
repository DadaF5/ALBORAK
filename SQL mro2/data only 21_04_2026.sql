USE [2BA_12]
GO
SET IDENTITY_INSERT [dbo].[AcStatusTypes] ON 
GO
INSERT [dbo].[AcStatusTypes] ([Id], [StatusName], [Description], [IsActive], [SortOrder], [StatusCode]) VALUES (1, N'Serviceable', N'Serviceable', 0, 0, NULL)
GO
INSERT [dbo].[AcStatusTypes] ([Id], [StatusName], [Description], [IsActive], [SortOrder], [StatusCode]) VALUES (2, N'Maintenance', N'Maintenance', 0, 0, NULL)
GO
INSERT [dbo].[AcStatusTypes] ([Id], [StatusName], [Description], [IsActive], [SortOrder], [StatusCode]) VALUES (3, N'Waiting Spare Part', N'Waiting Spare Part', 0, 0, NULL)
GO
SET IDENTITY_INSERT [dbo].[AcStatusTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Bases] ON 
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseCode], [IsActive], [Latitude], [Location], [Longitude]) VALUES (1, N'1°BAFRA', N'1BA', 1, NULL, N'SALE', NULL)
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseCode], [IsActive], [Latitude], [Location], [Longitude]) VALUES (2, N'2°BAFRA', N'2BA', 1, CAST(33.8944672 AS Decimal(10, 7)), N'MEKNES', CAST(-5.5492397 AS Decimal(10, 7)))
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseCode], [IsActive], [Latitude], [Location], [Longitude]) VALUES (3, N'3°BAFRA', N'', 0, NULL, N'', NULL)
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseCode], [IsActive], [Latitude], [Location], [Longitude]) VALUES (4, N'4°BAFRA', N'', 0, NULL, N'', NULL)
GO
INSERT [dbo].[Bases] ([Id], [BaseName], [BaseCode], [IsActive], [Latitude], [Location], [Longitude]) VALUES (5, N'5°BAFRA', N'', 0, NULL, N'', NULL)
GO
SET IDENTITY_INSERT [dbo].[Bases] OFF
GO
SET IDENTITY_INSERT [dbo].[AcCategories] ON 
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (1, N'Fighter', N'Fighter')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (2, N'Transport', N'Transport')
GO
INSERT [dbo].[AcCategories] ([AcCategoryId], [Name], [Description]) VALUES (3, N'Training', N'Training')
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
SET IDENTITY_INSERT [dbo].[AcTypes] ON 
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [SeatCount], [MaxEngines], [AcMainGroupId], [AircraftManufacturerId], [AircraftVersionId], [Code], [IsActive], [SortOrder]) VALUES (1, N'F-5E', N'F-5E Single seat', 0, 1, 1, 2, 1, NULL, NULL, NULL, 0, 0)
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [SeatCount], [MaxEngines], [AcMainGroupId], [AircraftManufacturerId], [AircraftVersionId], [Code], [IsActive], [SortOrder]) VALUES (3, N'F-5F', N'F-5F Duel Seats', 0, 2, 2, 2, 1, NULL, NULL, NULL, 0, 0)
GO
INSERT [dbo].[AcTypes] ([Id], [Name], [Description], [MaxGrossweight], [MaxPassengers], [SeatCount], [MaxEngines], [AcMainGroupId], [AircraftManufacturerId], [AircraftVersionId], [Code], [IsActive], [SortOrder]) VALUES (4, N'A-JET', N'A-JET', 0, 2, 2, 2, 2, NULL, NULL, NULL, 0, 0)
GO
SET IDENTITY_INSERT [dbo].[AcTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Aircrafts] ON 
GO
INSERT [dbo].[Aircrafts] ([Id], [TailNo], [Registration], [SerialNumber], [Manufacturer], [Model], [ManufactureDate], [IntCode], [Obs], [Active], [Serviceable], [AcTypeId], [AcStatusTypeId], [Status], [BaseId]) VALUES (5, 940, N'F-5-940', N'F-5E-940', N'NORTHROP', N'F Single seat', CAST(N'1978-06-01T00:00:00.0000000' AS DateTime2), N'CN-CNCA', N'XI-Type', 1, 1, 1, 1, 1, NULL)
GO
SET IDENTITY_INSERT [dbo].[Aircrafts] OFF
GO
SET IDENTITY_INSERT [dbo].[AircraftDocumentTypes] ON 
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (1, N'CDN', N'Certificat de navigabilité (CdN)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (2, N'CEN', N'Certificat d’examen de navigabilité (CEN)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (3, N'PEA', N'Programme d’entretien aéronef (PEA)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (4, N'LME', N'Liste minimale d’équipements (LME / LMER)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (5, N'LTTE', N'Liste type de tolérance d’entretien (LTTE)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (6, N'CDL', N'Configuration Deviation List (CDL)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (7, N'CN', N'Consigne de navigabilité (CN)', 1)
GO
INSERT [dbo].[AircraftDocumentTypes] ([Id], [Code], [Name], [IsActive]) VALUES (8, N'SB', N'Service Bulletin (SB) / Modifications', 1)
GO
SET IDENTITY_INSERT [dbo].[AircraftDocumentTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[AircraftDocuments] ON 
GO
INSERT [dbo].[AircraftDocuments] ([Id], [AircraftId], [DocumentTypeId], [ReferenceNo], [Revision], [Title], [IssuedAtUtc], [ValidFromUtc], [ValidUntilUtc], [IsCurrent], [Status], [StorageKey], [FileName], [ContentType], [FileSizeBytes], [Notes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy]) VALUES (1, 5, 1, N'DAM/CN/2024/0001', NULL, N'CdN initial', NULL, NULL, NULL, 1, N'Valid', NULL, NULL, NULL, NULL, NULL, CAST(N'2026-04-19T23:19:12.4957551' AS DateTime2), NULL, NULL, NULL)
GO
INSERT [dbo].[AircraftDocuments] ([Id], [AircraftId], [DocumentTypeId], [ReferenceNo], [Revision], [Title], [IssuedAtUtc], [ValidFromUtc], [ValidUntilUtc], [IsCurrent], [Status], [StorageKey], [FileName], [ContentType], [FileSizeBytes], [Notes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy]) VALUES (2, 5, 2, N'DAM/CEN/2026/0001', NULL, N'CEN annuel', NULL, NULL, NULL, 1, N'Valid', NULL, NULL, NULL, NULL, NULL, CAST(N'2026-04-19T23:19:12.4957551' AS DateTime2), NULL, NULL, NULL)
GO
INSERT [dbo].[AircraftDocuments] ([Id], [AircraftId], [DocumentTypeId], [ReferenceNo], [Revision], [Title], [IssuedAtUtc], [ValidFromUtc], [ValidUntilUtc], [IsCurrent], [Status], [StorageKey], [FileName], [ContentType], [FileSizeBytes], [Notes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy]) VALUES (3, 5, 3, N'PEA-FRA-DEFAULT-Rev01', NULL, N'PEA applicable', NULL, NULL, NULL, 1, N'Current', NULL, NULL, NULL, NULL, NULL, CAST(N'2026-04-19T23:19:12.4957551' AS DateTime2), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[AircraftDocuments] OFF
GO
SET IDENTITY_INSERT [dbo].[Departments] ON 
GO
INSERT [dbo].[Departments] ([Id], [Name], [Description], [BaseId]) VALUES (1, N'CMDMT', N'PC Commandement', 2)
GO
INSERT [dbo].[Departments] ([Id], [Name], [Description], [BaseId]) VALUES (2, N'GAC', N'Groupement Aerien Chasse F-5', 2)
GO
SET IDENTITY_INSERT [dbo].[Departments] OFF
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
INSERT [dbo].[Squadrons] ([Id], [Name], [CallSign], [LogoPath], [FrenchName], [CallSignShort], [WingId], [Active]) VALUES (2, N'Ecole-Chasse', N'TOBKAL', N'/uploads/squadrons/b77a4d3d-9ae7-481b-b151-801dd8f49ff4.png', N'Ecole de chasse', N'TBKL', 2, 1)
GO
SET IDENTITY_INSERT [dbo].[Squadrons] OFF
GO
SET IDENTITY_INSERT [dbo].[CallSigns] ON 
GO
INSERT [dbo].[CallSigns] ([Id], [Code], [Description], [BaseId], [SquadronId], [IsActive], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy]) VALUES (1, N'ROMEO', N'ROMEO', NULL, NULL, 1, CAST(N'2025-12-14T12:46:32.4451202' AS DateTime2), N'admin@example.com', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[CallSigns] OFF
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
SET IDENTITY_INSERT [dbo].[Odvs] ON 
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (1, 1, NULL, 3, CAST(N'2025-12-15' AS Date), N'North', N'Training', N'D-11', N'Planned', CAST(N'10:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2025-12-15T21:06:13.8415905' AS DateTime2), NULL)
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (2, 1, NULL, 3, CAST(N'2025-12-25' AS Date), N'North', N'Training', N'D-11', N'Planned', CAST(N'08:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2025-12-25T20:47:50.7270689' AS DateTime2), CAST(N'2025-12-25T20:48:12.0017873' AS DateTime2))
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (3, 2, NULL, 1, CAST(N'2025-12-26' AS Date), N'North', N'Training', N'R-10', N'Planned', CAST(N'10:00:00' AS Time), NULL, 2, 1, 0, CAST(N'2025-12-26T22:37:45.7378458' AS DateTime2), NULL)
GO
INSERT [dbo].[Odvs] ([Id], [SquadronId], [BaseId], [MissionId], [OdvDate], [Zone], [MissionType], [Area], [OdvStatus], [TOFF], [Obs], [AcMainGroupId], [CallSignId], [IsPreflightApproved], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (4, 1, NULL, 5, CAST(N'2026-01-18' AS Date), N'North', N'Training', N'D-9', N'Planned', CAST(N'08:00:00' AS Time), NULL, 1, 1, 0, CAST(N'2026-01-17T23:28:10.7866129' AS DateTime2), NULL)
GO
SET IDENTITY_INSERT [dbo].[Odvs] OFF
GO
SET IDENTITY_INSERT [dbo].[Sorties] ON 
GO
INSERT [dbo].[Sorties] ([Id], [OdvId], [BaseId], [AcTypeId], [AircraftId], [SortieCode], [Configuration], [Sequence], [FuelQuantity], [StartTime], [LandingTime], [TOFF], [Status], [RealTOFF], [RealLandingTime], [Notes], [DayHours], [NightHours], [DurationMinutes], [Approachs], [Landings], [TGOsLandings], [HobbsStart], [HobbsEnd], [HobbsUsed], [TachStart], [TachEnd], [TachUsed], [AirframeHours], [AirframeCycles], [InstSimulated], [InstActual], [IFRHours], [Cycles], [FuelUsedLiters], [Malfunctions], [IsCompleted], [IsFinalized], [BrakeChuteUsed], [Interceptions], [RadarContacts], [AppContacts], [SquadronReportNotes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy], [CompletedAtUtc], [CompletedBy], [FinalizedAtUtc], [FinalizedBy]) VALUES (1, 2, NULL, 1, NULL, N'A', N'Clean', 1, NULL, NULL, NULL, NULL, N'Planned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-12-25T21:58:50.9249320' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Sorties] ([Id], [OdvId], [BaseId], [AcTypeId], [AircraftId], [SortieCode], [Configuration], [Sequence], [FuelQuantity], [StartTime], [LandingTime], [TOFF], [Status], [RealTOFF], [RealLandingTime], [Notes], [DayHours], [NightHours], [DurationMinutes], [Approachs], [Landings], [TGOsLandings], [HobbsStart], [HobbsEnd], [HobbsUsed], [TachStart], [TachEnd], [TachUsed], [AirframeHours], [AirframeCycles], [InstSimulated], [InstActual], [IFRHours], [Cycles], [FuelUsedLiters], [Malfunctions], [IsCompleted], [IsFinalized], [BrakeChuteUsed], [Interceptions], [RadarContacts], [AppContacts], [SquadronReportNotes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy], [CompletedAtUtc], [CompletedBy], [FinalizedAtUtc], [FinalizedBy]) VALUES (2, 2, NULL, 1, NULL, N'A', N'Clean', 1, NULL, NULL, NULL, NULL, N'Planned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-12-26T18:24:00.2521200' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Sorties] ([Id], [OdvId], [BaseId], [AcTypeId], [AircraftId], [SortieCode], [Configuration], [Sequence], [FuelQuantity], [StartTime], [LandingTime], [TOFF], [Status], [RealTOFF], [RealLandingTime], [Notes], [DayHours], [NightHours], [DurationMinutes], [Approachs], [Landings], [TGOsLandings], [HobbsStart], [HobbsEnd], [HobbsUsed], [TachStart], [TachEnd], [TachUsed], [AirframeHours], [AirframeCycles], [InstSimulated], [InstActual], [IFRHours], [Cycles], [FuelUsedLiters], [Malfunctions], [IsCompleted], [IsFinalized], [BrakeChuteUsed], [Interceptions], [RadarContacts], [AppContacts], [SquadronReportNotes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy], [CompletedAtUtc], [CompletedBy], [FinalizedAtUtc], [FinalizedBy]) VALUES (3, 3, NULL, 4, NULL, N'A', N'Clean', 1, CAST(2400.00 AS Decimal(12, 2)), NULL, NULL, NULL, N'Planned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-12-26T22:38:09.8036485' AS DateTime2), NULL, CAST(N'2025-12-27T22:09:57.3040649' AS DateTime2), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Sorties] ([Id], [OdvId], [BaseId], [AcTypeId], [AircraftId], [SortieCode], [Configuration], [Sequence], [FuelQuantity], [StartTime], [LandingTime], [TOFF], [Status], [RealTOFF], [RealLandingTime], [Notes], [DayHours], [NightHours], [DurationMinutes], [Approachs], [Landings], [TGOsLandings], [HobbsStart], [HobbsEnd], [HobbsUsed], [TachStart], [TachEnd], [TachUsed], [AirframeHours], [AirframeCycles], [InstSimulated], [InstActual], [IFRHours], [Cycles], [FuelUsedLiters], [Malfunctions], [IsCompleted], [IsFinalized], [BrakeChuteUsed], [Interceptions], [RadarContacts], [AppContacts], [SquadronReportNotes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy], [CompletedAtUtc], [CompletedBy], [FinalizedAtUtc], [FinalizedBy]) VALUES (4, 3, NULL, 4, NULL, N'B', N'Clean', 2, NULL, NULL, NULL, NULL, N'Planned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-12-27T15:11:57.2898335' AS DateTime2), NULL, CAST(N'2025-12-27T18:22:56.0052644' AS DateTime2), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Sorties] ([Id], [OdvId], [BaseId], [AcTypeId], [AircraftId], [SortieCode], [Configuration], [Sequence], [FuelQuantity], [StartTime], [LandingTime], [TOFF], [Status], [RealTOFF], [RealLandingTime], [Notes], [DayHours], [NightHours], [DurationMinutes], [Approachs], [Landings], [TGOsLandings], [HobbsStart], [HobbsEnd], [HobbsUsed], [TachStart], [TachEnd], [TachUsed], [AirframeHours], [AirframeCycles], [InstSimulated], [InstActual], [IFRHours], [Cycles], [FuelUsedLiters], [Malfunctions], [IsCompleted], [IsFinalized], [BrakeChuteUsed], [Interceptions], [RadarContacts], [AppContacts], [SquadronReportNotes], [CreatedAtUtc], [CreatedBy], [UpdatedAtUtc], [UpdatedBy], [CompletedAtUtc], [CompletedBy], [FinalizedAtUtc], [FinalizedBy]) VALUES (5, 4, NULL, 3, NULL, N'Test', N'Clean', 1, CAST(1200.00 AS Decimal(12, 2)), NULL, NULL, NULL, N'Planned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2026-01-18T18:39:30.1997073' AS DateTime2), NULL, CAST(N'2026-01-18T18:39:58.3709221' AS DateTime2), NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Sorties] OFF
GO
SET IDENTITY_INSERT [dbo].[RankTypes] ON 
GO
INSERT [dbo].[RankTypes] ([Id], [Name], [Description]) VALUES (1, N'OFF', NULL)
GO
INSERT [dbo].[RankTypes] ([Id], [Name], [Description]) VALUES (2, N'ODR', NULL)
GO
INSERT [dbo].[RankTypes] ([Id], [Name], [Description]) VALUES (3, N'MDR', NULL)
GO
SET IDENTITY_INSERT [dbo].[RankTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Ranks] ON 
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (1, N'COL', N'Colonel', 5, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (2, N'GAL.B.A             ', N'Général de Brigade Aérienne                       ', 3, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (3, N'LTCol               ', N'LT-COLONEL                                        ', 6, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (4, N'CDT                 ', N'COMMANDANT                                        ', 7, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (5, N'CNE                 ', N'CAPITAINE                                         ', 8, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (6, N'LT                  ', N'LIEUTENANT                                        ', 9, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (7, N'SLT                 ', N'SOUS LIEUTENANT                                   ', 10, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (8, N'A/C                 ', N'ADJUDENT CHEF                                     ', 11, 2)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (16, N'SOLDAT', N'SOLDAT', 19, 3)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (17, N'COL-MAJ             ', N'Colonel Major                                     ', 4, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (19, N'ADJT                ', N'ADJUDENT', 12, 2)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (20, N'S/M                 ', N'SERGENT-MAJOR', 13, 2)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (21, N'S/C                 ', N'SERGRNT-CHEF', 14, 2)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (22, N'SGT                 ', N'SERGENT', 15, 2)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (23, N'C/C                 ', N'CAPORAL-CHEF', 16, 3)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (24, N'CAL                 ', N'CAPORAL', 17, 3)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (25, N'S/1CL               ', N'Soldat de 1ère classe', 18, 3)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (29, N'GAL.D.A             ', N'Général de division Aérienne', 2, 1)
GO
INSERT [dbo].[Ranks] ([Id], [Name], [FullRank], [Sequence], [RankTypeId]) VALUES (30, N'GAL.C.A', N'Général de corps d''armée', 1, 1)
GO
SET IDENTITY_INSERT [dbo].[Ranks] OFF
GO
SET IDENTITY_INSERT [dbo].[SubDepartments] ON 
GO
INSERT [dbo].[SubDepartments] ([Id], [Name], [DepartmentId]) VALUES (1, N'ESC-Chase', 2)
GO
INSERT [dbo].[SubDepartments] ([Id], [Name], [DepartmentId]) VALUES (2, N'CIPC', 2)
GO
SET IDENTITY_INSERT [dbo].[SubDepartments] OFF
GO
SET IDENTITY_INSERT [dbo].[Persons] ON 
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (1, 1, N'15096/87', N'Dadda', N'Abdellah', N'Male', 1, CAST(N'1967-01-01T00:00:00.0000000' AS DateTime2), N'I149330', N'PN', N'Meknes', N'Morocco', 1, N'M+2', 0xFFD8FFE1001845786966000049492A00080000000000000000000000FFEC00114475636B79000100040000003F0000FFE103E2687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F003C3F787061636B657420626567696E3D22EFBBBF222069643D2257354D304D7043656869487A7265537A4E54637A6B633964223F3E203C783A786D706D65746120786D6C6E733A783D2261646F62653A6E733A6D6574612F2220783A786D70746B3D2241646F626520584D5020436F726520352E332D633031312036362E3134353636312C20323031322F30322F30362D31343A35363A32372020202020202020223E203C7264663A52444620786D6C6E733A7264663D22687474703A2F2F7777772E77332E6F72672F313939392F30322F32322D7264662D73796E7461782D6E7323223E203C7264663A4465736372697074696F6E207264663A61626F75743D222220786D6C6E733A786D704D4D3D22687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F6D6D2F2220786D6C6E733A73745265663D22687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F73547970652F5265736F75726365526566232220786D6C6E733A786D703D22687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F2220786D6C6E733A64633D22687474703A2F2F7075726C2E6F72672F64632F656C656D656E74732F312E312F2220786D704D4D3A4F726967696E616C446F63756D656E7449443D22786D702E6469643A31323332323544444432443245343131393434434632434236304139333539382220786D704D4D3A446F63756D656E7449443D22786D702E6469643A34344635393134414437383031314534393333414434363843354632333836462220786D704D4D3A496E7374616E636549443D22786D702E6969643A34344635393134394437383031314534393333414434363843354632333836462220786D703A43726561746F72546F6F6C3D2241646F62652050686F746F73686F70204353352057696E646F7773223E203C786D704D4D3A4465726976656446726F6D2073745265663A696E7374616E636549443D22786D702E6969643A4132353239353333443444324534313139343443463243423630413933353938222073745265663A646F63756D656E7449443D22786D702E6469643A3132333232354444443244324534313139343443463243423630413933353938222F3E203C64633A63726561746F723E203C7264663A5365713E203C7264663A6C693E756E6B6E6F776E3C2F7264663A6C693E203C2F7264663A5365713E203C2F64633A63726561746F723E203C2F7264663A4465736372697074696F6E3E203C2F7264663A5244463E203C2F783A786D706D6574613E203C3F787061636B657420656E643D2272223F3EFFED004850686F746F73686F7020332E30003842494D040400000000000F1C015A00031B25471C020000020002003842494D0425000000000010FCE11F89C8B7C9782F346234075877EBFFEE000E41646F62650064C000000001FFDB0084000604040404040604040608050505080A070606070A0B09090A09090B0E0B0C0C0C0C0B0E0C0D0D0E0D0D0C11111212111119181818191C1C1C1C1C1C1C1C1C1C010606060B0A0B150E0E1517131013171C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1CFFC00011080081006403011100021101031101FFC400A100000104030100000000000000000000000003040507020608010101000301010100000000000000000000000102040305061000020103030105050309050900000000010203001104120506213141511307617122321481422391A1B1527292331608C162533415D1B243832474253617110002020103030203060700000000000000011102032112043141511305713214F0819122423361C1D1F1522324FFDA000C03010002110311003F00EA9A00A00A00A00A00A00A00A00A00A00A00A00A00A00A00A00A00A0309A6871E269F22458628C5DE472155478927A0A01A616FF00B16E53B62EDDB8E2664E82ED1413C72381E255189A01F50050050050050050050050050050107CDB99EC7C078DE5F27E4137938986BF0A0B192694FC90C4BF79E43D00FB4F404D01C87CC797FA99EB8CCD34F19DBF8E924E26D0B298F185BE5321B039128BFCCC34FEA815972F2A9470D9B30F072645296857793C479071A9464E4453EDD2E31BA6563DD4291DEB2C6415F7DEAD8F3D2FD194CBC5C98D4D9685A7E95FF531CB78CEE7060734DC1B9171B3F8734B280F9F8E0F648AE3E29827DE56B9B761BF6F69833AD4EC4C3CBC6CFC4873B0A559F1B2A359A0990DD5E37019581F020D58815A00A00A00A00A00A00A00A039A7D6ADC7F9BFD5C8389EE24C9B2718C64957115BE0973B2A3D6CF25BB74445540F69F1AC9CBCAEB5D0DBC1C2AF7D47FB26D38AC803FE169B04441D0015E0F567D32D113136CDB5E4C2C92057BDC15750411E0411D4574DA8AEF6685CB78171FCEC76C210C5097B98E685151E17B7461A40EC3DDDF57C39AD5B7538727056F5882C6FE96F77DCF23846E1C737571249C6B729312020F418F22ACA8ABFDD5666D3ECE9DD5EFD2D2A4F98C958705CD572814014014014014014014072FE46D866F5879D65645F5626646CABDB712E3C654DFF006474AF2FDC3B23D6F6DEEC758BCAB90E14C31FFD3F074EA00A4B3F979054F51A631727F2566AE3A253A9E86FC8DC686D797BEE1C3B6AEE698E1D99008E20D62F21FBB5CA6A769B41A664F22DCF7268933B6718259C2BBA4C930D04F420A77FB0D5EF86BD53D4E2B25FF5236FFE9C719F1B7BE750EAF822CCC34D3DC58C523EAF674602BD7E339A23C2E5FEE32EFAD0660A00A00A00A00A00A00A0293CED8530BD45E53B9A17FFCB4D88CA5CDC968A008DA6FDC0D80AF1B9966ED1E0F7B8544AA9F923A6E1F8A990D9F0431C593147320C875D52689CEB95413FAE45EE7B3BAB8D73D95627437FA349DCD6A2597B632ECF8B3404B34730732B0BDCB1F98FB0DEB8AD353AB5A0962710C4C0CA6C982110BBC92644CD113A257947C5AAFF77BD57EE9ECAED9335AEA199D60A53546F3E896CBF40793EEDAAE376DCD485B760C785631D7DB5EA70DB753C2E6D52B7F1659D5ACC4140140140140140140140571CDF6B9B0B7E4DCB52987703D2D7D41A355041BF4F75793CDC6D3DDE4F6781993AEDF0453646882445432C8EA428EDB16AC299EAA6BBB19CF2EF29B6438ACDE642641E6BE81E769BDEC6C2D6EEF1F1AEADB839F7D660C32B718258649A1BC68A1AE0DC104743D0DAB9772D91E8589E9CEDF99B7F19897353CB7C8739082E09D1200549B765FC2BDCE351D6BA9F3BCBC8AD6D3B2367AD0650A00A00A00A00A00A00A0340F55391EC98236DE3F2C8D26FD9EEF9581890A97610E3A933CB2DBF87104B8D47B5AC05FADB3F2F1EEC6DF83570EF1912F268CC71B78C5292CF3C519B32BE348D19208EF2BD7ECAF0D59A3E8291D46195C7F0D9557CD9F1D2F749A299C3023C05FE127BEBABCBA74345B24A8809F323932A0C31E64F1C8638083F14D2202359EB6D4C56E07B6AB8D6EB6A62CCE1685FBB567606E5B763E76D722CD853460C0E9D9A474B7B0ADAC41EA0F4AFA2758D0F9A99D475500280280280280F1DD2346924608882ECCC6C001DE49A0348E45EB67A61C659E2CFDFB1F232631738B837CB96FE16843807DE457458ACCABBA4557C9BFAB092576C7E17B3688C5FFEBB756EB63D8571E13FEF483DD5DABC7F251E4227D0FF00ACE77CE792725E47912E66E32EDBF4C329ADA93EA641AB42FCAAAAA802A0E8055F26356A3AF6E845323AD9597546C5BAEDDBA713DC2584C5E6ACC0B3C29D1645FF001A027B8FDE4ED535F2F9703C76DB6FB99F4B8B32C95DF4FBD782226E4A3488E2C699E517BC42365EA7A0BB3002D50E8BBB2EF259F61F71DE37B96F79AC1E4F2F332407C898754C3C6BF523C5DBB17C4FB055F8D85E5BAAD7E5453939561C6ED6F99E88D6E4F56390FA49EA2F26D9F6B54DCF613B8C921DAB25CAAC6F285959A1914318D8963A858A93D6D7EB5F4D6A2B753E66B682D1E31FD4E7A73BD2A45BD34FC6B25BE619A9AF1EFD9613C5A93F7B4D70781F6D4E8AE8B476CDDF6ADEB18666CF9906E18CD6B4D8D224A9D7DA848AE4D35D4BA63BA80140565CEBD61876FF004EB71E67C255372FA7C91838D933AB79064FA8FA7794282AD2468D7D2C080DEEACDF52BEA561EFDFF092DB7F26E396F9573CE69CDA669392EED91971312570D58C58897EE5863B27EF5CFB6BD9AD12323B49031C51C602A80AA3B80B0ABC11266025FE116E9D6A6049637A07C806C5EA3E1634842E36FA8FB6CB736F8CFE2427C3E75B7DB5165A30752EEFB26DFBE621C1DC50B203AA3910E99227ECD71B771FCC7BEB165C55C958B1DB0E6B63B6EAB2BCCBE1BBB26F71EC09998D932CCBE6C12C9222CAB083D5CC17D77163F2820F757917F6EBCE9F2F93DCA7B963759B7CDE3FA1BCEDBB3EDBC736EFA682FE4C40CF9790FF003CA505D9DCFB8741DD5ECF1B02C5584787C8E45B2DB75BFB1C4FC8B779B7FDF771DF720EA9374CA9B249FEEBB9D03EC4B0AD4CE2860841BE9FBA6C689122D83999FB4648CCDA32A7DB7254EA13E24AF03DC7898CADFEDA860BA7D1BF5BBD45CCE5BB5F12DEB263DF3037067479F2D00CA852389E42E258F4EBF96C75A9F7D61E75962C4EF1D0ED866D648B9FFF00B171AFE6BFE46F2F23F987569FA7D3F836FA7FA8D5E75ED6D1D2D6D57EEEFAC3F55FF37AF1A78347A5FECD92725BF31E453F12878209D60D87125795E0892D24ECF21997CE90924AA39F8556DEDBD7A75E063F59E66BF3991E6B6DDBD886296EEADA723C2BD2809AC6CAE158BC5258A64C8CAE519A24886A4D30E28F3018E48E4240D45058D831EA474EDAC16AF21E794D2C4A3EF352B6258A226E4663CB91852439588DA3271244C881AF6B4B130753FBC2BD05A194ED3DB7959DF384E3F2FD92019F919F83F5387845B479B95A0FE093F77F154A9ACAE9F9A3ED05A60E38DCB916FF00BA6E2791674B932F28CDCB2C258352E5A6587D2914217E24313D91107676568E853A9D2BC9F90F28DB3D13DC32F992C58FCB06D831F346311E599F248894F4B28934B7E22AF40D7B74AE74AEB3DBEDF6459B9672A3C23CB08B71E5FC2A40BF41D2AF0097DE775E39B96CFB6A6DD84F81BD62018FB8116304B0C48551D5AC353B3756D43578DEB0F1F166A65B6E738DF4F89AB2E4C76C7585175D487D3D3DD5BA0CA4971EDFF77E2DBB45BEEC52A63EE1023C4AD246B2A18E51675646EE61D2E2C6B872B8B4CF474BF465F1E474B4A1D7F3A6F7FCEBFF00D0AD0FFAC7D6FD5F9766FA7BF97E4E8D37D5E5E9E96BD67FA0A7A1E8FE9883A7ACF7EEEE44A122407B9C11F68EA2BD1466323DA2A405A80C155439BFCC3B01FECF1A4132281FB8D482F0F4139BE2ED7C5B90EDDBB3031F178E4DE704313FC39EE2441FF39401E1AAB9DD748EFA128D0769E41060F3EDBB9F6FB02C871B707CEDC120402C270CB2346A2C3545AF50B75E9E353645132D6FEA2773C5C7E03B760E1CA278F7DCE8A686553AB5C304667D60F786D49515725A0E7317AB224C5B4EA03B49EA7DDE3420F4014017D0A5BF5413F92804AC7CBB5FA69ECF6DAFFA6A8499B31F9FF5581B55C8178D1E43A224695CDEC91A97636EDB0504D1B494B0937D051B1B2A29931A58258A7974E886446491B59B2E956009D47B3C6AAB2D1A94D47C49D96988723ADD78F6F7B3C714BBD604DB724EDA61390021736D5F00BEA361DF6AE78B958B2B6A96568EB05AF86F453650475C83A5BEC6F1F7FB6B41CE4718F9791885CC12346932F95908A6C248B50628E3BD75286B788A8649232485A12D19EA7A8FC950CA09EE3C8F73DDF63D8F63CE20C1C6E19F1F10DCDCA4F289006BFF00860685F65117229BE11E24F60F1A403C034F6F527A9352405EA091299AE9A476B103F3D4308C3FE25FBAF7A82451BA2903C0D58A8E30F72CADB3261CFDBF225C3CC8087827818ACA848B5D48F7D56F4ADD6DB295E0B56CD394F536D3EAF72B6C2311870E5DD05B4EF936246F988CA4E865B8D01D013A5ADD0F5AF257B2615795B957FC7B7E3D4DAFDC2EEB0D29F26A7939D919F972676E73CF999B392D2E4E4B192462DD4F53F2827B96C2BD6C58E98D6DAA8462BDDD9CB7278C548E96208FB2BA95122FA2EAE7E13F7BC2FE35560750E4492613C4A40922E82FD9D2AA06C1F4AFC46F61F94D591202D62F2103C2FF00A280F1A64EEB9B7829A89026D320EE7FDD35122049E456642BD9727C3B3C69203574B541264EDF0B1F01562030DCB62C6E7A92BDBEE35157A06384491C318D1DC46BAE52AA58225EDA9881F0ADFBCD4BB25D4433C5BF6775588089EE1947606361EFA20644DC5BB68043CC584B6A2238C805CB1B016E805FDB556B524C95F590EDD3C17C07FB6A503D2433A83D4004DBBA8C840CE40E950489F9A5AE6FD86C7DF4122136A13C44F61536FCB557D4932BF5A90792BE985CF7DBF4D1BD0812C10AF8A84DF502C2E091D87D955A74259B4719DF37BDAB6ADFB69DA727C8837AC51167BB42B3B08012BD19BF863E3B6A3E3E35939786B6B52CF5757A7C4E98F23AA6977200C719760DA9AC4801C92458DAB65755A9CD8AA055F945AAE8A9EB1A904CF0EE47B7F1ADCF27373F6CC7DD84F8C71E13928251033FCCE88D752ECA74DD874AF3BDC78F97351571DB6B9D7B1AB8B7C74B4DD4A20EC8A4F960AA5CE8526E42DFA027DD5BEAA16A677AB3C2BABA9241F11D0D490212EA1D15DAFD9D4EA1F9EAAC0E370DCA7DC06309952338702E30F25426B54BD99EC05DBAF6D67C383D3DDAB7B9CEA77CB977C6896D51A119191F54C4765EDDA4F60F6D5D7539761D5FE2B57420F323F82D50FA0461B77F945FDA7FD34A740CB77D1CFF00D679C7FD8AFF006578FEEBFB98FE3FCCD9C5F96C55B2FF009997F6BFB057AF8FA231B325AEA8A99B76548115F9CD412663BE800D00DDBE7FB6AAC9426DDBF6D4303583F8CFFB66B9AEA5879DFF00655FB907FFD9)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (2, 6, N'1233/20             ', N'AL MAJIDI                     ', N'ABDELKADER                    ', N'Male', 1, CAST(N'2002-08-01T00:00:00.0000000' AS DateTime2), N'AE304389  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (3, 6, N'12040/22            ', N'DIHAJI                        ', N'HAMZA                         ', N'Male', 1, CAST(N'2001-07-13T00:00:00.0000000' AS DateTime2), N'D588864   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (4, 6, N'1423/19             ', N'EL AIDI                       ', N'DRISS                         ', N'Male', 1, CAST(N'2001-06-13T00:00:00.0000000' AS DateTime2), N'CB338650  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (5, 6, N'10334/19            ', N'SAMIR                         ', N'BRAHIM                        ', N'Male', 1, CAST(N'1999-12-13T00:00:00.0000000' AS DateTime2), N'EE862648  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (6, 6, N'2/1                 ', N'GOUTOU                        ', N'HAMZA                         ', N'Male', 1, CAST(N'1999-02-14T00:00:00.0000000' AS DateTime2), N'XA124818  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (7, 6, N'3/1                 ', N'BELLAINE                      ', N'AYOUB                         ', N'Male', 1, CAST(N'1999-01-29T00:00:00.0000000' AS DateTime2), N'EE890113  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (8, 6, N'352/17              ', N'DABICH                        ', N'SAMIRA                        ', N'Male', 1, CAST(N'1999-01-19T00:00:00.0000000' AS DateTime2), N'JH44596   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (9, 6, N'1/1                 ', N'FENZAR                        ', N'MOHAMED AMINE                 ', N'Male', 1, CAST(N'1998-06-13T00:00:00.0000000' AS DateTime2), N'EE825574  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (10, 6, N'516/16              ', N'AGAROUNI                      ', N'ISMAIL                        ', N'Male', 1, CAST(N'1998-03-01T00:00:00.0000000' AS DateTime2), N'X386788   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (11, 6, N'01/16               ', N'BENHEMMOUCHE                  ', N'MOUAD                         ', N'Male', 1, CAST(N'1997-10-05T00:00:00.0000000' AS DateTime2), N'G704260   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (12, 6, N'467/15              ', N'EL HENNOUNI                   ', N'BADR EDDINE                   ', N'Male', 1, CAST(N'1997-08-14T00:00:00.0000000' AS DateTime2), N'D913096   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (13, 6, N'463/15              ', N'GUERRI                        ', N'OMAR                          ', N'Male', 1, CAST(N'1997-07-24T00:00:00.0000000' AS DateTime2), N'CN21925   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (14, 6, N'11/14               ', N'FOUZI                         ', N'SALMANE                       ', N'Male', 1, CAST(N'1997-07-24T00:00:00.0000000' AS DateTime2), N'EE561421  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (15, 6, N'217/15              ', N'EL JADID                      ', N'IMAD                          ', N'Male', 1, CAST(N'1997-05-06T00:00:00.0000000' AS DateTime2), N'T253988   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (16, 6, N'10/14               ', N'SAHAB                         ', N'EL MEHDI                      ', N'Male', 1, CAST(N'1997-03-16T00:00:00.0000000' AS DateTime2), N'BB147309  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (17, 6, N'14782/20            ', N'OUGAHI                        ', N'RADOUANE                      ', N'Male', 1, CAST(N'1997-02-10T00:00:00.0000000' AS DateTime2), N'CN25373   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (18, 6, N'9/13                ', N'ALOUANE                       ', N'HICHAM                        ', N'Male', 1, CAST(N'1997-01-01T00:00:00.0000000' AS DateTime2), N'BB130146  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (19, 6, N'2078/15             ', N'NAJIB                         ', N'YASSINE                       ', N'Male', 1, CAST(N'1996-09-21T00:00:00.0000000' AS DateTime2), N'DC149499  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (20, 6, N'5/13                ', N'BILALI                        ', N'MOHAMED SAIF                  ', N'Male', 1, CAST(N'1996-07-17T00:00:00.0000000' AS DateTime2), N'G690609   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (21, 6, N'2033/15             ', N'LAGRINI                       ', N'AMINE                         ', N'Male', 1, CAST(N'1996-07-01T00:00:00.0000000' AS DateTime2), N'DO41490   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (22, 6, N'144/14              ', N'SLIMANI                       ', N'SOUFIANE                      ', N'Male', 1, CAST(N'1996-06-14T00:00:00.0000000' AS DateTime2), N'F543432   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (23, 6, N'14976/19            ', N'SALHI                         ', N'BADR                          ', N'Male', 1, CAST(N'1996-03-02T00:00:00.0000000' AS DateTime2), N'D946429   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (24, 6, N'4/15                ', N'OUBOUJEMAA                    ', N'MOHAMMED                      ', N'Male', 1, CAST(N'1996-01-15T00:00:00.0000000' AS DateTime2), N'CD636826  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (25, 6, N'158/13              ', N'BAKKASS                       ', N'ZAKARIA                       ', N'Male', 1, CAST(N'1996-01-01T00:00:00.0000000' AS DateTime2), N'T262925   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (26, 6, N'4479/15             ', N'LAASRI                        ', N'AYOUB                         ', N'Male', 1, CAST(N'1995-12-17T00:00:00.0000000' AS DateTime2), N'GY33008   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (27, 6, N'168/13              ', N'FILALI                        ', N'NIZAR                         ', N'Male', 1, CAST(N'1995-08-06T00:00:00.0000000' AS DateTime2), N'CD622311  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (28, 6, N'139/13              ', N'KHADIRI                       ', N'YAHYA                         ', N'Male', 1, CAST(N'1995-05-12T00:00:00.0000000' AS DateTime2), N'BB116431  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (29, 6, N'1/15                ', N'TOUTATE                       ', N'ZAKARIA                       ', N'Male', 1, CAST(N'1995-03-29T00:00:00.0000000' AS DateTime2), N'VM4481    ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (30, 6, N'142/13              ', N'SAROUTI                       ', N'BADR                          ', N'Male', 1, CAST(N'1995-03-25T00:00:00.0000000' AS DateTime2), N'EE617571  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (31, 6, N'160/13              ', N'BIHICH                        ', N'MOHAMMED AYOUB                ', N'Male', 1, CAST(N'1995-02-28T00:00:00.0000000' AS DateTime2), N'G688002   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (32, 6, N'1106/13             ', N'DAOU                          ', N'ISSAM                         ', N'Male', 1, CAST(N'1995-01-01T00:00:00.0000000' AS DateTime2), N'DO28396   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (33, 6, N'211/12              ', N'KANDROUCH                     ', N'NABIL                         ', N'Male', 1, CAST(N'1994-11-06T00:00:00.0000000' AS DateTime2), N'V311749   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (34, 6, N'201/12              ', N'BADIRI                        ', N'GHASSANE                      ', N'Male', 1, CAST(N'1994-10-24T00:00:00.0000000' AS DateTime2), N'Y380183   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (35, 6, N'4689/14             ', N'SALMI                         ', N'MOHAMMED                      ', N'Male', 1, CAST(N'1994-10-02T00:00:00.0000000' AS DateTime2), N'D7734     ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (36, 6, N'203/12              ', N'ZOUINATI                      ', N'SOUHAIB                       ', N'Male', 1, CAST(N'1994-09-10T00:00:00.0000000' AS DateTime2), N'K493029   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (37, 6, N'3411/13             ', N'ZYADI                         ', N'HAMZA                         ', N'Male', 1, CAST(N'1994-09-10T00:00:00.0000000' AS DateTime2), N'D85960    ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (38, 6, N'44/12               ', N'RAHOUTI                       ', N'ZAKARIAE                      ', N'Male', 1, CAST(N'1994-07-16T00:00:00.0000000' AS DateTime2), N'LC252128  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (39, 6, N'200/12              ', N'ZAD                           ', N'TAOUFIQ                       ', N'Male', 1, CAST(N'1994-06-09T00:00:00.0000000' AS DateTime2), N'Y378015   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (40, 6, N'1568/10             ', N'BELHAJ                        ', N'YOUSSEF                       ', N'Male', 1, CAST(N'1993-10-24T00:00:00.0000000' AS DateTime2), N'LA20159   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (41, 6, N'11090/17            ', N'EL MAKAOUI                    ', N'YOUNES                        ', N'Male', 1, CAST(N'1993-10-23T00:00:00.0000000' AS DateTime2), N'AD228166  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (42, 6, N'9123/15             ', N'EL FATTACH                    ', N'AMINE                         ', N'Male', 1, CAST(N'1993-09-18T00:00:00.0000000' AS DateTime2), N'EA170632  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (43, 6, N'431/12              ', N'GUAMIL                        ', N'ISSAM                         ', N'Male', 1, CAST(N'1993-08-08T00:00:00.0000000' AS DateTime2), N'GA170761  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (44, 6, N'589/11              ', N'BENMIRA                       ', N'YOUSSEF                       ', N'Male', 1, CAST(N'1993-02-26T00:00:00.0000000' AS DateTime2), N'          ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (45, 6, N'1171/12             ', N'LAAMIMICH                     ', N'SOUFIAN                       ', N'Male', 1, CAST(N'1993-01-22T00:00:00.0000000' AS DateTime2), N'V296437   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (46, 6, N'7452/13             ', N'DALYA                         ', N'RADOUANE                      ', N'Male', 1, CAST(N'1992-12-10T00:00:00.0000000' AS DateTime2), N'CD560057  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (47, 6, N'97/10               ', N'EL MALOULI                    ', N'YASSER                        ', N'Male', 1, CAST(N'1992-07-16T00:00:00.0000000' AS DateTime2), N'EE507614  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (48, 6, N'1748/11             ', N'EL MESSAOUDI                  ', N'SEDDIK                        ', N'Male', 1, CAST(N'1992-07-03T00:00:00.0000000' AS DateTime2), N'AE21080   ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (49, 6, N'8286/14             ', N'AHMID                         ', N'MOHAMED                       ', N'Male', 1, CAST(N'1992-04-03T00:00:00.0000000' AS DateTime2), N'AE636936  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (50, 6, N'578/10              ', N'MJIDOU                        ', N'JIHAD                         ', N'Male', 1, CAST(N'1991-08-03T00:00:00.0000000' AS DateTime2), N'CB263450  ', N'Pilot', NULL, N'MAROC', 1, NULL, NULL)
GO
INSERT [dbo].[Persons] ([Id], [RankId], [Matricule], [FirstName], [LastName], [Gender], [SubDepartmentId], [DateOfBirth], [NationalId], [Speciality], [City], [Country], [Active], [PatrimonialStatus], [Photo]) VALUES (51, 6, N'577/10              ', N'EL KHAZARJI                   ', N'ALI                           ', N'Male', 1, CAST(N'1991-05-31T00:00:00.0000000' AS DateTime2), N'EE497728  ', N'Pilot', N'Meknes', N'MAROC', 1, N'M', NULL)
GO
SET IDENTITY_INSERT [dbo].[Persons] OFF
GO
SET IDENTITY_INSERT [dbo].[Qualifications] ON 
GO
INSERT [dbo].[Qualifications] ([Id], [Name], [Description], [QualificationType], [Active]) VALUES (1, N'CP', N'CP', N'Military', 1)
GO
INSERT [dbo].[Qualifications] ([Id], [Name], [Description], [QualificationType], [Active]) VALUES (2, N'SP', N'SP', N'Military', 1)
GO
SET IDENTITY_INSERT [dbo].[Qualifications] OFF
GO
SET IDENTITY_INSERT [dbo].[CrewMembers] ON 
GO
INSERT [dbo].[CrewMembers] ([Id], [SequenceNo], [Captain], [NickName], [Role], [Photo], [Active], [Mobile], [Status], [AllowedToSign], [CrewMemberType], [SquadronId], [PersonId], [PrimaryQualificationId], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (1, 10, N'Dadda', N'DADDA', N'Pilot', N'/uploads/crewmembers/4c6fb8fe3a6148b2947f85e95b0ef359.jpg', 1, N'0667082169', N'Ready', 1, N'Pilot', 1, 1, 1, CAST(N'2025-12-27T21:27:14.0172283' AS DateTime2), CAST(N'2026-01-13T21:37:03.5327721' AS DateTime2))
GO
INSERT [dbo].[CrewMembers] ([Id], [SequenceNo], [Captain], [NickName], [Role], [Photo], [Active], [Mobile], [Status], [AllowedToSign], [CrewMemberType], [SquadronId], [PersonId], [PrimaryQualificationId], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (2, 10, N'ELA KHAZARJI', N'KHAZARJI', N'Pilot', N'/uploads/crewmembers/86de1121dc7446f794b981212b166f56.jpg', 1, N'0667082169', N'Ready', 1, N'Pilot', 1, 51, 1, CAST(N'2025-12-28T20:13:50.2502027' AS DateTime2), CAST(N'2025-12-28T21:41:29.8280431' AS DateTime2))
GO
INSERT [dbo].[CrewMembers] ([Id], [SequenceNo], [Captain], [NickName], [Role], [Photo], [Active], [Mobile], [Status], [AllowedToSign], [CrewMemberType], [SquadronId], [PersonId], [PrimaryQualificationId], [CreatedAtUtc], [UpdatedAtUtc]) VALUES (3, 11, N'SAROUTI', N'SAROUTI', N'Pilot', N'/uploads/crewmembers/16f77465ffe540a188071066dffcef9e.jpg', 1, N'0667082169', N'Ready', 1, N'Pilot', 1, 30, 1, CAST(N'2025-12-28T20:14:43.1040914' AS DateTime2), NULL)
GO
SET IDENTITY_INSERT [dbo].[CrewMembers] OFF
GO
SET IDENTITY_INSERT [dbo].[CrewMemberQualifications] ON 
GO
INSERT [dbo].[CrewMemberQualifications] ([Id], [CrewMemberId], [QualificationId], [ValidFrom], [ValidUntil], [IssuedBy], [Remarks], [Status]) VALUES (1, 2, 1, CAST(N'2025-12-28T00:00:00.0000000' AS DateTime2), NULL, N'CDT GAC', N'OPT', N'Active')
GO
SET IDENTITY_INSERT [dbo].[CrewMemberQualifications] OFF
GO
SET IDENTITY_INSERT [dbo].[MedicalChecks] ON 
GO
INSERT [dbo].[MedicalChecks] ([Id], [CrewMemberId], [BaseId], [CheckType], [CheckDate], [Decision], [DecisionText], [Derogation], [NextDueDate], [NextVuDate], [LateCheckReason], [OBESITE], [C_Optique], [CreatedAtUtc], [UpdatedAtUtc], [CreatedBy], [UpdatedBy], [DurationYears], [DurationDays], [DurationMonths]) VALUES (3, 1, 2, 0, CAST(N'2026-01-02T00:00:00.0000000' AS DateTime2), 0, NULL, 1, NULL, NULL, NULL, 0, 0, CAST(N'2026-01-01T23:31:46.3032088' AS DateTime2), NULL, N'admin@example.com', NULL, 0, 0, 3)
GO
INSERT [dbo].[MedicalChecks] ([Id], [CrewMemberId], [BaseId], [CheckType], [CheckDate], [Decision], [DecisionText], [Derogation], [NextDueDate], [NextVuDate], [LateCheckReason], [OBESITE], [C_Optique], [CreatedAtUtc], [UpdatedAtUtc], [CreatedBy], [UpdatedBy], [DurationYears], [DurationDays], [DurationMonths]) VALUES (4, 3, 2, 0, CAST(N'2026-01-02T00:00:00.0000000' AS DateTime2), 0, NULL, 0, NULL, NULL, NULL, 1, 1, CAST(N'2026-01-01T23:32:27.7620468' AS DateTime2), NULL, N'admin@example.com', NULL, 1, 0, 0)
GO
INSERT [dbo].[MedicalChecks] ([Id], [CrewMemberId], [BaseId], [CheckType], [CheckDate], [Decision], [DecisionText], [Derogation], [NextDueDate], [NextVuDate], [LateCheckReason], [OBESITE], [C_Optique], [CreatedAtUtc], [UpdatedAtUtc], [CreatedBy], [UpdatedBy], [DurationYears], [DurationDays], [DurationMonths]) VALUES (5, 2, 2, 0, CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), 0, NULL, 1, NULL, NULL, N'planned', 0, 0, CAST(N'2026-01-02T20:16:56.9179280' AS DateTime2), NULL, N'admin@example.com', NULL, 0, 0, 6)
GO
SET IDENTITY_INSERT [dbo].[MedicalChecks] OFF
GO
SET IDENTITY_INSERT [dbo].[SortieCrews] ON 
GO
INSERT [dbo].[SortieCrews] ([Id], [SortieId], [CrewMemberId], [Seat], [Role], [IsPrimary], [Remarks], [AircraftRole]) VALUES (1, 3, 1, 1, N'Pilot', 1, NULL, N'Captain')
GO
INSERT [dbo].[SortieCrews] ([Id], [SortieId], [CrewMemberId], [Seat], [Role], [IsPrimary], [Remarks], [AircraftRole]) VALUES (3, 3, 3, 2, NULL, 0, NULL, N'Copilot')
GO
INSERT [dbo].[SortieCrews] ([Id], [SortieId], [CrewMemberId], [Seat], [Role], [IsPrimary], [Remarks], [AircraftRole]) VALUES (4, 5, 1, 1, N'Pilot', 1, NULL, N'Captain')
GO
SET IDENTITY_INSERT [dbo].[SortieCrews] OFF
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
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BaseId], [WingId], [DepartmentId], [SquadronId], [AcMainGroupId], [JobTitle], [EmployeeNumber], [TimeZone], [Locale], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [HireDate], [TerminationDate], [LastLoginUtc], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'FADWA', N'DADDA', 2, 2, 2, 2, 2, NULL, NULL, NULL, NULL, 1, CAST(N'2025-12-26T18:29:39.2531495' AS DateTime2), NULL, NULL, NULL, NULL, N'fadwa@example.com', N'FADWA@EXAMPLE.COM', N'fadwa@example.com', N'FADWA@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAEPQeN0yZJ6FgEv/7IFPsY1vH35NBX7IBxpOowIWwNj2QrekQwHAcjZkymF4v6wY3fg==', N'5JOC5DOZKDIWK5GJNDB46MTYEXBCVWFV', N'2560f01b-3e19-45fe-8213-76fa64ed7472', N'0667082117', 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BaseId], [WingId], [DepartmentId], [SquadronId], [AcMainGroupId], [JobTitle], [EmployeeNumber], [TimeZone], [Locale], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [HireDate], [TerminationDate], [LastLoginUtc], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'c4e38a7b-9745-426e-9fec-196482f2a290', N'Admin', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, CAST(N'2025-12-15T21:00:30.1085150' AS DateTime2), NULL, NULL, NULL, NULL, N'admin@example.com', N'ADMIN@EXAMPLE.COM', N'admin@example.com', N'ADMIN@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAEMVCqNy++pQo3U1SZEREa+OW4iny8KDi9JI8o5MlKysHznxtlOJg6uGJjMBmCWYxOg==', N'O4OCFXVYTIS4VKFGDASS2A6QAVHHZ2XK', N'9d800e40-bdc0-490a-b8c2-08173845ff74', NULL, 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BaseId], [WingId], [DepartmentId], [SquadronId], [AcMainGroupId], [JobTitle], [EmployeeNumber], [TimeZone], [Locale], [IsActive], [CreatedAtUtc], [UpdatedAtUtc], [HireDate], [TerminationDate], [LastLoginUtc], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'ZIYAD', N'Dadda', 2, 2, 2, 1, 1, NULL, NULL, NULL, NULL, 1, CAST(N'2025-12-26T18:26:51.6395214' AS DateTime2), NULL, NULL, NULL, NULL, N'ziyad@example.com', N'ZIYAD@EXAMPLE.COM', N'ziyad@example.com', N'ZIYAD@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAEI9JbNibE9AS+wHk0lyyLN9vthVuJIvJ6uOdkP3gsAAOp3deRSlHvMK0cBBT6hVD2w==', N'B7OQ5KBMTT7GCC5GESJSO6VTYZ3O2YXQ', N'802f62aa-168c-4c8a-aebf-71216f10e9b6', N'0667082117', 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'36662047-5961-43f6-a08d-72a631f15d63')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'a15a7097-1e61-4e97-aaf0-efea9d157ecf')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'a15a7097-1e61-4e97-aaf0-efea9d157ecf')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c4e38a7b-9745-426e-9fec-196482f2a290', N'c3a8cd93-f2a2-47e1-9190-60935ae72242')
GO
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] ON 
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (1, N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'BaseId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (2, N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'WingId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (3, N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'DepartmentId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (4, N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'SquadronId', N'1')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (5, N'da4c9b83-2067-46c0-9b19-a47f3926e44f', N'AcMainGroupId', N'1')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (31, N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'BaseId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (32, N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'WingId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (33, N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'DepartmentId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (34, N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'SquadronId', N'2')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (35, N'4f42f778-02d4-43a9-8d55-f61927fd881f', N'AcMainGroupId', N'2')
GO
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] OFF
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251215201110_Initial_Create', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251225203459_Initial_Create', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251227211006_AddSortieCrew', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251228213119_AddCrewMemberQualificationsController', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251229193734_MedicalCheck_and_Bilan', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251231223238_InitialCreate', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260101222535_MedicaCheckRefined', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260112172531_Area', N'9.0.10')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260419222752_Baseline', N'9.0.10')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260419232548_AddUniqueIndex_AircraftDocumentTypes_Code', N'9.0.10')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260419233224_AddUniqueIndex_AircraftDocumentTypes_Code', N'9.0.10')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260420205900_Settings_Ac_Version_Manufacturer', N'9.0.10')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260421193614_AddGeoCoordinatesToBase', N'9.0.10')
GO
SET IDENTITY_INSERT [dbo].[MenuItems] ON 
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (1, N'Squadron', N'fa fa-fighter-jet', NULL, NULL, NULL, NULL, 100, NULL, NULL, NULL, N'SquadronOps')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (2, N'CrewChief', N'fas fa-user-cog', NULL, NULL, NULL, NULL, 200, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (3, N'Aircraft', N'fa fa-fighter-jet', NULL, NULL, NULL, NULL, 300, NULL, NULL, NULL, N'AircraftMaintenance')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (4, N'Create ODV', N'fa fa-fighter-jet', N'OdvPlanning', N'Index', NULL, 1, 10, NULL, NULL, N'Admin', N'SquadronOps')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (5, N'Crew Member', N'fa fa-fighter-jet', N'CrewMembers', N'Index', NULL, 1, 20, NULL, NULL, N'Admin', N'SquadronOps')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (6, N'Update Sortie', NULL, N'Sortie', N'Edit', NULL, 1, 30, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (7, N'Rank', N'fas fa-dot-circle', N'Rank', N'Index', NULL, 12, 10, NULL, NULL, N'Admin', NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (8, N'Persons List', N'fas fa-dot-circle', N'Person', N'Index', NULL, 12, 5, NULL, NULL, NULL, N'HR')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (9, N'Rank Type', N'fas fa-dot-circle', N'RankType', N'Index', NULL, 12, 30, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (10, N'List', NULL, N'Aircraft', N'Index', NULL, 3, 10, NULL, NULL, NULL, N'AircraftMaintenance')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (11, N'Create', NULL, N'Aircraft', N'Create', NULL, 3, 20, NULL, NULL, NULL, N'AircraftMaintenance')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (12, N'HR', N'fas fa-user-cog', N'Person', N'Index', NULL, NULL, 600, NULL, NULL, N'Admin', N'HR')
GO
INSERT [dbo].[MenuItems] ([Id], [Title], [IconClass], [Controller], [Action], [Url], [ParentId], [SortOrder], [DepartmentId], [BaseId], [Roles], [Area]) VALUES (13, N'Ac Main Group (F-16)', N'fa fa-fighter-jet', N'AcMainGroup', N'Index', NULL, 3, 10, NULL, NULL, N'Admin', N'AircraftMaintenance')
GO
SET IDENTITY_INSERT [dbo].[MenuItems] OFF
GO
