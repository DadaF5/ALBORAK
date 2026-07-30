# AL-BORAK / FRAProject — Project Status & Next Steps
> Reconciled against the actual VS2022 solution (`FRAContext.cs`, solution tree, and live files) on 31 July 2026.
> This supersedes `Session_Summary_Settings_Module.md` and `ALBORAK_Handoff_Phase2.md` wherever they conflict with what's below — those two documents contain planned/stale information that doesn't match the current codebase.

---

## 1. How to use this document

Three earlier documents (`Session_Summary_Settings_Module.md`, `ALBORAK_Handoff_Phase2.md`, and the Inspection Process Guide/Checklist) each described the project at different points in time, and had drifted from each other and from the real solution. This file is the reconciled result — verified directly against `FRAContext.cs`, the solution's folder tree, and uploaded source files, not against what earlier docs *claimed* was built. Treat this as the current source of truth; treat the three older files as historical record only.

---

## 2. Stack (unchanged, confirmed)

| Item | Value |
|---|---|
| Framework | ASP.NET Core MVC (.NET 6+) |
| Database | SQL Server · `DB2BAFRA` |
| ORM | EF Core, Fluent API configs in `Data/EntityConfigurations` |
| UI | AdminLTE dark theme · French UI · English code |
| Auth | ASP.NET Identity, role `"Admin"` (not `"Administrators"`) |
| Layout convention | Explicit `Layout = "~/Views/Shared/_Layout.cshtml";` in every view going forward |

---

## 3. Domain / Area map (confirmed against `FRAContext.cs` and the solution tree)

```
Areas/
  Settings/            Lookups, Aircraft, Dossier — mature, many CRUDs
  AircraftMaintenance/ Certificates, Restrictions, DAM Dashboard, Inspection Process (new)
  HR/                  Person, Rank, RankType, Department, SubDepartment, Wing, Squadron
  SquadronOps/         Sorties, Missions, Crew, Qualifications, Wings (views only — model is in HR)
  Medical/             MedicalCheck, MedicalBilan, MedicalDashboard  (this is "Healthcare" from the old handoff — same thing, different name)
  Identity/            Login pages (Razor Pages, not MVC)
  Admin/               stub only
```

`DOMAIN_ARCHITECTURE.md` (in "Other Documents") is the accurate high-level domain reference — it describes 4 domains (HR, Squadron Ops, Aircraft Maintenance, Medical Care) and is consistent with what's actually built. Use it over the ALBORAK handoff for domain-level questions.

---

## 4. Known inconsistencies — decide, don't just note

These are real gaps between documentation and code, confirmed by reading source files. Each needs an owner decision, not just a mention.

### 4.1 User Management / Scoping module — designed, never built
The ALBORAK handoff describes a full system: `UserProfile`, `Module`, `ModuleRole`, `UserAssignment`, `IScopeService`, session-based scope caching, wing career-movement tracking. **None of this exists in `FRAContext.cs`.** No matching DbSets, no matching migrations. `DOMAIN_ARCHITECTURE.md` (the more current doc) doesn't mention it either.
**Decision needed:** is this still wanted? If yes, it's new work, not "already done." If no, strike it from planning docs entirely.

### 4.2 `Wing` model location contradicts the ALBORAK "locked" rule
Handoff says `Wing → SquadronOps.Models`. Actual file: `Areas/HR/Models/Wing.cs`. Confirmed via `FRAContext.cs` using-statements and DbSet placement. Compiles fine either way — this is a documentation problem only.
**Decision needed:** update the doc, or move the file. Pick one.

### 4.3 Two competing repository patterns
- **Settings area** has its own `IBaseRepository`/`BaseRepository` + one specialist (`AcMainGroupRepository`) — thin, only 2 of the 4 modules the Session Summary claimed were "done" actually exist.
- **Root `Infrastructure`** has a separate `IGenericRepository`/`GenericRepository` + `IUnitOfWork`/`UnitOfWork` — this is the pattern actively being used for new work (confirmed: `InspectionType` uses it).
- There's also a second, apparently unused `FRAProjectDbContext.cs` alongside the real `FRAContext.cs`.
**Decision needed:** confirm `Infrastructure`'s `UnitOfWork` pattern is the one going forward (it appears to be, based on recent work), and clean up or delete the unused `FRAProjectDbContext.cs` and the thin Settings-local repository interfaces to avoid confusion.

### 4.4 Maintenance Phase 2 schema differs from the original ALBORAK plan
The ALBORAK handoff's 13-entity schema (`PE`, `JobCardApplicability`, `Snag`, `JobCardTool`, `JobCardPart`, etc.) was **not** what got built. What's actually implemented — confirmed in `FRAContext.cs` and the Inspection Process Guide/Checklist — is a cleaner, renamed schema: `MaintenanceProgram` (replaces `PE`), `JobCardPlanningRule` (replaces `JobCardApplicability`), `AircraftJobCardState` (new, not in the original plan), no `Snag` entity at all yet.
**This is fine** — the Inspection Process Guide explicitly documents this as an intentional redesign ("start from ground zero," "avoid copying the legacy schema blindly"). Just make sure nothing downstream is still planning against the old ALBORAK schema names.

---

## 5. Inspection Process module — current real status

This is the most current, most actively-developed part of the solution. Status below is verified against actual files, not the Checklist doc (which undercounts progress in a few places — see §5.1).

| Layer | Status |
|---|---|
| Models (12 entities) | ✅ Done |
| `FRAContext` DbSets | ✅ Done — all 12 registered |
| EntityConfigurations | ✅ Done — all 12, cascade-path issues resolved |
| Migration + DB update | ✅ Done (`AddInspectionProcessFoundation`) |
| **`InspectionType` full vertical slice** | ✅ **Done and smoke-tested** — model, repository, UnitOfWork wiring, controller, all 5 views |
| `MaintenanceProgram`, `JobCard`, `JobCardPlanningRule`, `WorkOrder` | ⬜ Not started (repository/controller/views) |
| Business logic services (due calculation, work-order generation, signoff validation, etc.) | ⬜ Not started — deferred by design until CRUD layer is complete |

### 5.1 Correction to the Checklist doc
`Inspection-Process-Implementation-Checklist.md` marks `IInspectionTypeRepository`, UnitOfWork wiring, and the ViewModels as pending/partial. **They are actually done** — confirmed by reading the real files. Only the controller and views were genuinely missing, and those are now built too. Update or retire that checklist; it's now fully behind reality for `InspectionType`.

### 5.2 `InspectionType` — reference implementation for everything that follows
Delivered this session, present in `/mnt/user-data/outputs`:
- `Controllers/InspectionTypesController.cs`
- `Views/InspectionTypes/Index.cshtml`
- `Views/InspectionTypes/_Form.cshtml`
- `Views/InspectionTypes/Create.cshtml`
- `Views/InspectionTypes/Edit.cshtml`
- `Views/InspectionTypes/Details.cshtml`
- `Views/InspectionTypes/Delete.cshtml`

Repository method contract confirmed: `IGenericRepository<T>` has `AddAsync` (async), but `Update(entity)` and `Delete(entity)` are **synchronous void** methods — commit happens via `_uow.CompleteAsync()`. This is now correctly reflected in the controller; keep this in mind when writing the next repositories so the pattern stays consistent.

---

## 6. UI/UX conventions established this session — reuse these going forward

A few real usability problems surfaced and got fixed on `InspectionType`; apply the same pattern to every new module rather than rediscovering them each time.

- **Dark theme contrast**: default AdminLTE dark styling makes `btn-outline-*` buttons and pure-white form inputs either invisible or harsh. Fix: solid buttons (`btn-secondary`/`btn-primary`/`btn-danger`, not outline), and form inputs styled dark-gray (`#2b2f36`) with a visible border instead of stark white.
- **French labels**: don't rely on `[Display(Name=...)]` in ViewModels (several were left in English) — hardcode French label text directly in the Razor views instead.
- **Required-field markers**: red `*` next to genuinely required fields (`Code`, `Name`, the parent FK) so the form doesn't imply every field is mandatory.
- **Group visually similar sections apart**: e.g. "Intervalle" vs "Tolérance" cards look identical by default — a colored left border (blue vs. amber) fixes the risk of mixing them up.
- **Hide setup/plumbing fields**: fields like `SortOrder` or a single-value enum (`Kind`) that mean nothing to the person filling the form belong in a collapsed "Options avancées" section, not front-and-center.
- **Delete pattern**: soft delete (`ToggleActive`, `IsActive = false`) offered as the recommended default, hard delete available with `DbUpdateException` caught and a friendly message — matches the existing `AcTypesController` convention and the project's own Golden Rule ("never `DELETE`, `IsActive = false` instead").
- **Explicit `Layout` assignment** in every new view, per your instruction.

---

## 7. Recommended next steps, in order

### Immediate — finish the Inspection Process CRUD layer
Per the Guide's own sequence (§10) and Checklist (§L), repeat the `InspectionType` pattern for:
1. **`MaintenanceProgram`** — repository (`IMaintenanceProgramRepository`/`MaintenanceProgramRepository`, same 3-method shape: `GetAllWithDetailsAsync`, `GetByIdWithDetailsAsync`, `ExistsByCodeAsync`), UnitOfWork wiring, controller, views
2. **`JobCard`**
3. **`JobCardPlanningRule`**
4. **`WorkOrder`**

Each one should reuse the exact conventions in §6 above rather than reinventing them.

### Before that work starts — small cleanup items worth doing now, cheaply
- Resolve the `Wing` namespace doc/code mismatch (§4.2) — five-minute fix
- Confirm and document which `DbContext`/`UnitOfWork` pattern is canonical, delete the unused one (§4.3)
- Retire or update `Inspection-Process-Implementation-Checklist.md` so it stops undercounting progress (§5.1)

### Later — once the CRUD layer for all 5 inspection entities is complete
- Business logic services: inspection due calculation, planning rule evaluation, work-order generation, signoff validation, inspection state refresh
- `DueList` view → unlocks the `DamDashboard.TotalDueSoon` stub that's currently hardcoded to 0
- Decide the fate of the User Management/scoping module (§4.1) — build it for real, or drop it from planning

### Not urgent, but flagged for awareness
- `IUnitOfWork`/`UnitOfWork` don't yet have entries for `AircraftCertificate`/`AircraftRestriction` as specialist repositories (currently generic) — fine for now, revisit if those need custom query methods like `InspectionType` has
- Settings area's repository/service layer is much thinner than the old Session Summary implied — only worth investing in if you're actively touching those modules again

---

## 8. Quick reference — what's genuinely done vs. what's still aspirational

| Claim source | What it says | What's actually true |
|---|---|---|
| Session Summary | Repository "done" for AcType, AircraftManufacturer, AircraftVersion, AcMainGroup | Only `AcMainGroup` + generic `Base` repository exist |
| ALBORAK Handoff | User Management module "complete" (Phase 1) | Doesn't exist in `FRAContext.cs` at all |
| ALBORAK Handoff | `Wing → SquadronOps.Models` (locked rule) | `Wing.cs` is actually in `HR.Models` |
| ALBORAK Handoff | 13-entity Phase 2 schema (`PE`, `Snag`, `JobCardApplicability`...) | Superseded by a different, intentionally redesigned schema |
| Inspection Checklist | `InspectionType` repository + UnitOfWork wiring "pending" | Both were already done |
| Inspection Checklist | `InspectionType` controller/views "pending" | Now done — built this session |

---

*Reconciled from: solution tree export, `FRAContext.cs`, `DOMAIN_ARCHITECTURE.md`, `Inspection-Process-Implementation-Guide.md`, `Inspection-Process-Implementation-Checklist.md`, `IUnitOfWork.cs`, `UnitOfWork.cs`, `InspectionType.cs` and related repository/ViewModel files, plus live smoke-testing confirmation.*
