# Inspection Process Implementation Checklist
## FRAProject / ALBORAK
### File-by-file implementation tracker

---

# Status legend

- `[x]` Done
- `[ ]` Pending
- `[~]` Partially done / concept done, file not yet finalized in repo
- `N/A` Not required yet

---

# A. Models

## A.1 Core inspection models

### Master / setup
- [x] `Areas/AircraftMaintenance/Models/InspectionType.cs`
- [x] `Areas/AircraftMaintenance/Models/MaintenanceProgram.cs`
- [x] `Areas/AircraftMaintenance/Models/InspectionTypeProgram.cs`
- [x] `Areas/AircraftMaintenance/Models/JobCard.cs`
- [x] `Areas/AircraftMaintenance/Models/ProgramJobCard.cs`
- [x] `Areas/AircraftMaintenance/Models/JobCardPlanningRule.cs`
- [x] `Areas/AircraftMaintenance/Models/JobCardAttachment.cs`

### Runtime state
- [x] `Areas/AircraftMaintenance/Models/InspectionState.cs`
- [x] `Areas/AircraftMaintenance/Models/AircraftJobCardState.cs`

### Workflow
- [x] `Areas/AircraftMaintenance/Models/WorkOrder.cs`
- [x] `Areas/AircraftMaintenance/Models/WorkOrderJobCard.cs`
- [x] `Areas/AircraftMaintenance/Models/WorkOrderJobCardSignOff.cs`

---

## A.2 Existing shared/base models reused

- [x] `Areas/Settings/Models/LookupBase.cs`
- [x] `Areas/Settings/Models/AcType.cs`
- [x] `Areas/Settings/Models/Aircraft.cs`
- [x] `Models/ApplicationUser.cs`

---

## A.3 Model notes
- [x] `InspectionType` aligned to inherit from `LookupBase`
- [x] `MaintenanceProgram` aligned to inherit from `LookupBase`
- [x] Cleaned model architecture agreed
- [x] EntityConfiguration chosen as mapping source of truth

---

# B. FRAContext

## B.1 DbSet registrations

### Inspection process DbSets
- [x] `DbSet<InspectionType> InspectionTypes`
- [x] `DbSet<MaintenanceProgram> MaintenancePrograms`
- [x] `DbSet<InspectionTypeProgram> InspectionTypePrograms`
- [x] `DbSet<JobCard> JobCards`
- [x] `DbSet<ProgramJobCard> ProgramJobCards`
- [x] `DbSet<JobCardPlanningRule> JobCardPlanningRules`
- [x] `DbSet<JobCardAttachment> JobCardAttachments`
- [x] `DbSet<InspectionState> InspectionStates`
- [x] `DbSet<AircraftJobCardState> AircraftJobCardStates`
- [x] `DbSet<WorkOrder> WorkOrders`
- [x] `DbSet<WorkOrderJobCard> WorkOrderJobCards`
- [x] `DbSet<WorkOrderJobCardSignOff> WorkOrderJobCardSignOffs`

---

## B.2 `OnModelCreating` registrations

### Inspection process `ApplyConfiguration(...)`
- [x] `InspectionTypeConfiguration`
- [x] `MaintenanceProgramConfiguration`
- [x] `InspectionTypeProgramConfiguration`
- [x] `JobCardConfiguration`
- [x] `ProgramJobCardConfiguration`
- [x] `JobCardPlanningRuleConfiguration`
- [x] `JobCardAttachmentConfiguration`
- [x] `InspectionStateConfiguration`
- [x] `AircraftJobCardStateConfiguration`
- [x] `WorkOrderConfiguration`
- [x] `WorkOrderJobCardConfiguration`
- [x] `WorkOrderJobCardSignOffConfiguration`

---

# C. EntityConfiguration files

## C.1 Completed configuration files
- [x] `Data/EntityConfigurations/InspectionTypeConfiguration.cs`
- [x] `Data/EntityConfigurations/MaintenanceProgramConfiguration.cs`
- [x] `Data/EntityConfigurations/InspectionTypeProgramConfiguration.cs`
- [x] `Data/EntityConfigurations/JobCardConfiguration.cs`
- [x] `Data/EntityConfigurations/ProgramJobCardConfiguration.cs`
- [x] `Data/EntityConfigurations/JobCardPlanningRuleConfiguration.cs`
- [x] `Data/EntityConfigurations/JobCardAttachmentConfiguration.cs`
- [x] `Data/EntityConfigurations/InspectionStateConfiguration.cs`
- [x] `Data/EntityConfigurations/AircraftJobCardStateConfiguration.cs`
- [x] `Data/EntityConfigurations/WorkOrderConfiguration.cs`
- [x] `Data/EntityConfigurations/WorkOrderJobCardConfiguration.cs`
- [x] `Data/EntityConfigurations/WorkOrderJobCardSignOffConfiguration.cs`

## C.2 Configuration quality status
- [x] Refactored to match cleaned models
- [x] Refactored to match `LookupBase`
- [x] SQL Server cascade-path issues resolved
- [x] Migration successfully applied

---

# D. Migrations / Database

## D.1 Migration files
- [x] Migration created for inspection foundation
- [x] Migration reviewed
- [x] SQL Server constraint errors resolved
- [x] `Update-Database` succeeded

### Files
- [x] `Migrations/<timestamp>_AddInspectionProcessFoundation.cs`
- [x] `Migrations/<timestamp>_AddInspectionProcessFoundation.Designer.cs`

## D.2 Database status
- [x] New inspection foundation tables created
- [x] Keys created
- [x] FK constraints created
- [x] Unique indexes created

---

# E. DTO / ViewModels

> Status note:
> We designed these in detail conceptually.
> Depending on your repo, some may still need to be physically created as files if not yet added.

## E.1 Shared helper ViewModels
- [~] `ViewModels/AircraftMaintenance/LookupOptionViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/UserLookupViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/AircraftLookupViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/AcTypeLookupViewModel.cs`

## E.2 InspectionType ViewModels
- [~] `ViewModels/AircraftMaintenance/InspectionTypeListItemViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/InspectionTypeFormViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/InspectionTypeDetailsViewModel.cs`

## E.3 MaintenanceProgram ViewModels
- [~] `ViewModels/AircraftMaintenance/MaintenanceProgramListItemViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/MaintenanceProgramFormViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/MaintenanceProgramDetailsViewModel.cs`

## E.4 JobCard ViewModels
- [~] `ViewModels/AircraftMaintenance/JobCardListItemViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/JobCardFormViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/JobCardDetailsViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/JobCardAttachmentItemViewModel.cs`

## E.5 JobCardPlanningRule ViewModels
- [~] `ViewModels/AircraftMaintenance/JobCardPlanningRuleListItemViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/JobCardPlanningRuleFormViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/JobCardPlanningRuleDetailsViewModel.cs`

## E.6 WorkOrder ViewModels
- [~] `ViewModels/AircraftMaintenance/WorkOrderListItemViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/WorkOrderFormViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/WorkOrderDetailsViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/WorkOrderJobCardItemViewModel.cs`
- [~] `ViewModels/AircraftMaintenance/WorkOrderJobCardSignOffItemViewModel.cs`

## E.7 Recommended grouping option
If grouped by feature instead of one file per class:

- [ ] `ViewModels/AircraftMaintenance/SharedLookupViewModels.cs`
- [ ] `ViewModels/AircraftMaintenance/InspectionTypeViewModels.cs`
- [ ] `ViewModels/AircraftMaintenance/MaintenanceProgramViewModels.cs`
- [ ] `ViewModels/AircraftMaintenance/JobCardViewModels.cs`
- [ ] `ViewModels/AircraftMaintenance/JobCardPlanningRuleViewModels.cs`
- [ ] `ViewModels/AircraftMaintenance/WorkOrderViewModels.cs`

> Choose either per-class files or grouped files, not both.

---

# F. Repositories

## F.1 Existing shared repository infrastructure
- [x] `Infrastructure/Interfaces/IGenericRepository.cs`
- [x] `Infrastructure/Repositories/GenericRepository.cs`

## F.2 Existing UnitOfWork infrastructure
- [x] `Infrastructure/Interfaces/IUnitOfWork.cs`
- [x] `Infrastructure/UnitOfWork.cs` or existing UnitOfWork implementation file

---

## F.3 New inspection repositories — recommended next

### InspectionType
- [ ] `Areas/AircraftMaintenance/Repositories/IInspectionTypeRepository.cs`
- [ ] `Areas/AircraftMaintenance/Repositories/InspectionTypeRepository.cs`

### MaintenanceProgram
- [ ] `Areas/AircraftMaintenance/Repositories/IMaintenanceProgramRepository.cs`
- [ ] `Areas/AircraftMaintenance/Repositories/MaintenanceProgramRepository.cs`

### JobCard
- [ ] `Areas/AircraftMaintenance/Repositories/IJobCardRepository.cs`
- [ ] `Areas/AircraftMaintenance/Repositories/JobCardRepository.cs`

### JobCardPlanningRule
- [ ] `Areas/AircraftMaintenance/Repositories/IJobCardPlanningRuleRepository.cs`
- [ ] `Areas/AircraftMaintenance/Repositories/JobCardPlanningRuleRepository.cs`

### WorkOrder
- [ ] `Areas/AircraftMaintenance/Repositories/IWorkOrderRepository.cs`
- [ ] `Areas/AircraftMaintenance/Repositories/WorkOrderRepository.cs`

---

# G. UnitOfWork updates

## G.1 `IUnitOfWork`
### Existing
- [x] `AcMainGroups` repository property exists

### Pending additions
- [ ] `IInspectionTypeRepository InspectionTypes { get; }`
- [ ] `IMaintenanceProgramRepository MaintenancePrograms { get; }`
- [ ] `IJobCardRepository JobCards { get; }`
- [ ] `IJobCardPlanningRuleRepository JobCardPlanningRules { get; }`
- [ ] `IWorkOrderRepository WorkOrders { get; }`

## G.2 `UnitOfWork` implementation
### Pending wiring
- [ ] instantiate `InspectionTypeRepository`
- [ ] instantiate `MaintenanceProgramRepository`
- [ ] instantiate `JobCardRepository`
- [ ] instantiate `JobCardPlanningRuleRepository`
- [ ] instantiate `WorkOrderRepository`

---

# H. Controllers

## H.1 Recommended first-wave controllers

### InspectionType
- [ ] `Areas/AircraftMaintenance/Controllers/InspectionTypesController.cs`

### MaintenanceProgram
- [ ] `Areas/AircraftMaintenance/Controllers/MaintenanceProgramsController.cs`

### JobCard
- [ ] `Areas/AircraftMaintenance/Controllers/JobCardsController.cs`

### JobCardPlanningRule
- [ ] `Areas/AircraftMaintenance/Controllers/JobCardPlanningRulesController.cs`

### WorkOrder
- [ ] `Areas/AircraftMaintenance/Controllers/WorkOrdersController.cs`

---

## H.2 Suggested controller action checklist

### InspectionTypesController
- [ ] `Index`
- [ ] `Details(int id)`
- [ ] `Create()`
- [ ] `Create(POST)`
- [ ] `Edit(int id)`
- [ ] `Edit(POST)`
- [ ] `Delete(int id)`
- [ ] `DeleteConfirmed(int id)`

### MaintenanceProgramsController
- [ ] `Index`
- [ ] `Details`
- [ ] `Create`
- [ ] `Edit`
- [ ] `Delete`

### JobCardsController
- [ ] `Index`
- [ ] `Details`
- [ ] `Create`
- [ ] `Edit`
- [ ] `Delete`

### JobCardPlanningRulesController
- [ ] `Index`
- [ ] `Details`
- [ ] `Create`
- [ ] `Edit`
- [ ] `Delete`

### WorkOrdersController
- [ ] `Index`
- [ ] `Details`
- [ ] `Create`
- [ ] `Edit`
- [ ] `Open / Close / Status actions` (later if needed)
- [ ] `Delete` (optional depending on business rule)

---

# I. Views

## I.1 InspectionType views
- [ ] `Areas/AircraftMaintenance/Views/InspectionTypes/Index.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/InspectionTypes/Create.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/InspectionTypes/Edit.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/InspectionTypes/Details.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/InspectionTypes/Delete.cshtml`

## I.2 MaintenanceProgram views
- [ ] `Areas/AircraftMaintenance/Views/MaintenancePrograms/Index.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/MaintenancePrograms/Create.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/MaintenancePrograms/Edit.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/MaintenancePrograms/Details.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/MaintenancePrograms/Delete.cshtml`

## I.3 JobCard views
- [ ] `Areas/AircraftMaintenance/Views/JobCards/Index.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCards/Create.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCards/Edit.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCards/Details.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCards/Delete.cshtml`

## I.4 JobCardPlanningRule views
- [ ] `Areas/AircraftMaintenance/Views/JobCardPlanningRules/Index.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCardPlanningRules/Create.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCardPlanningRules/Edit.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCardPlanningRules/Details.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/JobCardPlanningRules/Delete.cshtml`

## I.5 WorkOrder views
- [ ] `Areas/AircraftMaintenance/Views/WorkOrders/Index.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/WorkOrders/Create.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/WorkOrders/Edit.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/WorkOrders/Details.cshtml`
- [ ] `Areas/AircraftMaintenance/Views/WorkOrders/Delete.cshtml`

---

# J. Mapping / helper layer

## J.1 Entity ↔ ViewModel mapping
> Can be manual in controllers first, or later extracted.

### Pending
- [ ] `InspectionType` ↔ ViewModel mapping
- [ ] `MaintenanceProgram` ↔ ViewModel mapping
- [ ] `JobCard` ↔ ViewModel mapping
- [ ] `JobCardPlanningRule` ↔ ViewModel mapping
- [ ] `WorkOrder` ↔ ViewModel mapping

## J.2 Optional later refactor
- [ ] dedicated mapper class(es)
- [ ] service layer for orchestration

---

# K. Business logic services (later phase)

## K.1 Not yet started
- [ ] inspection due calculation service
- [ ] job-card planning rule evaluation service
- [ ] work-order generation service
- [ ] work-order closing service
- [ ] signoff validation service
- [ ] inspection state refresh service
- [ ] aircraft job-card state recomputation service

---

# L. Suggested implementation order from now

## Phase 1 — first working CRUD slice
1. [ ] `IInspectionTypeRepository`
2. [ ] `InspectionTypeRepository`
3. [ ] update `IUnitOfWork`
4. [ ] update `UnitOfWork`
5. [ ] create `InspectionTypesController`
6. [ ] create `InspectionTypes` views
7. [ ] test full CRUD

## Phase 2
8. [ ] `MaintenanceProgram` repository/controller/views
9. [ ] `JobCard` repository/controller/views

## Phase 3
10. [ ] `JobCardPlanningRule` repository/controller/views

## Phase 4
11. [ ] `WorkOrder` repository/controller/views

---

# M. High-level progress snapshot

## Completed
- [x] Models
- [x] `FRAContext` DbSets
- [x] EntityConfigurations
- [x] Migration
- [x] Database update

## Designed but may still need file creation
- [~] DTO/ViewModels

## Not yet implemented
- [ ] Repositories for inspection entities
- [ ] UnitOfWork extensions
- [ ] Controllers
- [ ] Views
- [ ] Mapping logic
- [ ] Business services

---

# N. Recommended immediate next file set

## Do next
- [ ] `Areas/AircraftMaintenance/Repositories/IInspectionTypeRepository.cs`
- [ ] `Areas/AircraftMaintenance/Repositories/InspectionTypeRepository.cs`
- [ ] update `Infrastructure/Interfaces/IUnitOfWork.cs`
- [ ] update `Infrastructure/UnitOfWork.cs`
- [ ] `Areas/AircraftMaintenance/Controllers/InspectionTypesController.cs`

---