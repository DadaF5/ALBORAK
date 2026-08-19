# AL-BORAK — RBAC Rollout, WorkSection Fix, Account/Access Cleanup — Session Handoff
> Carry this file into the next chat session as the first message, alongside the earlier Phase 1/2, RBAC, and Snag Module handoffs.

---

## What this session covered

Started as "finish converting the remaining Admin-only controllers to policy-based RBAC" (left unfinished at the end of the RBAC session), expanded into: completing that conversion across both `AircraftMaintenance` and `SquadronOps`, a real architectural bug fix in `WorkSection`, and a cluster of account/access-control problems surfaced by actually live-testing the RBAC conversion for the first time.

---

## ✅ AircraftMaintenance — RBAC conversion completed

All remaining Admin-only controllers converted to `MaintenanceRead`/`MaintenanceWrite` policies with real `UserAssignment`-based scoping via `IUserScopeService`:

`AircraftCertificatesController`, `AircraftRestrictionsController`, `MaintenanceProgramsController`, `JobCardsController`, `WorkOrdersController`, `InspectionTypesController`, `WorkSectionsController`, `DueListController`, `ProgramJobCardsController`, `WorkOrderSectionsController`, `WorkOrderSectionPartsController`, `WorkOrderSectionTasksController`, `WorkOrderSectionSignOffsController`. `DamDashboardController` was already correctly converted in the prior session — verified, not touched.

**Two scoping shapes established and now applied consistently:**
- **Aircraft-instance data** (Base + AcMainGroup) — `IsAircraftInScopeAsync()` pattern, used by `WorkOrders`/`DueList`/the `WorkOrderSection*` family (resolved via parent `WorkOrder.AircraftId`)
- **AcType/AcMainGroup-level setup data** (AcMainGroup only, no Base dimension) — `IsAcTypeInScopeAsync()` pattern, used by `InspectionTypes`/`JobCards`/`MaintenancePrograms`/`ProgramJobCards`

**Real gaps closed, not just re-platformed:** several of these controllers had zero per-record scope checks even under the old `[Authorize(Roles="Admin")]` — `Details`/`Delete` on a specific record were previously unguarded beyond the coarse role check.

**Live-tested successfully** (this was the biggest open item from the prior session — never actually done before now): `test.f5tech@example.com` (F5-2B scope) confirmed to see only F5 aircraft in `FleetStatusController`; Admin confirmed to see the full fleet after logout/login — no scope leakage across the session boundary.

---

## ✅ SquadronOps — RBAC conversion built from scratch

This module had **no real RBAC at all** before this session — a mix of dead role checks, raw claims, and legacy `ApplicationUser` fields. Converted 18 controllers total across two batches:

**Batch 1:** `CallSignsController`, `PhasesController`, `MissionController`, `CrewMembersController`, `CrewMemberQualificationsController`, `OdvPlanningController`
**Batch 2:** `QualificationsController`, `SortiesController`, `SortieCrewsController`, `SquadronActivitiesController`, `SquadronController`, `WingsController`

**Foundational change:** extended `UserScope`/`UserScopeService` with `AllowedWingIds` (mirrors `AllowedAcMainGroupIds`), sourced from `UserAssignment.WingId` — this didn't exist before and was required for any of SquadronOps' Wing-scoped roles (Pilot/Instructor/Scheduler) to work correctly.

**Real gaps closed:** `PhasesController`, `CrewMembersController`, `SortieCrewsController`, `SortiesController`, `SquadronActivitiesController`, `SquadronController`, `WingsController`, `QualificationsController` had **zero `[Authorize]`** — reachable by any authenticated user regardless of module, only behind login at all because of the RBAC session's global `FallbackPolicy`. `OdvPlanningController.Edit` had no per-record scope check at all. `MissionController`/`OdvPlanningController`/`SortiesController` were gated on fictional role names (`"SquadronOps"`, `"Administrator"`, `"SuperAdmin"`, `"MaintenanceSupervisor"`, `"FlightOpsManager"`) that don't exist in the real seeded role system — dead checks.

**Behavioral change worth re-confirming with real users:** the old code forced every non-Admin to exactly one squadron/AcMainGroup pulled from legacy `ApplicationUser` fields. Since `UserAssignment` has no `SquadronId` dimension (only Base/AcMainGroup/Wing), squadron-scope is now resolved by traversing Squadron→Wing→`Wing.BaseId` and checking against the assignment's Base+Wing. A user with a Wing-level assignment can now see/act on **every squadron in that wing**, not just one "home" squadron — matches the documented design intent, but is a real widening of behavior versus what shipped before.

**Doc corrections surfaced along the way:** `WingsController.cs` already existed with full CRUD — the RBAC handoff's claim "Wing has no CRUD" was wrong. `Wing` has its own direct `BaseId` scalar (confirmed via `SquadronController`/`WingsController`), not just a path through `Department` — simpler than the Department-hop join used in Batch 1's controllers (flagged, not retroactively fixed).

**Explicitly parked, not touched:** `OdvsController.cs` (1390-line, no-`[Area]`, root-route controller managing the same `Odv`/`Sortie` data as `OdvPlanningController` under a *different* route and *different* fictional role set — unclear if live or dead, needs its own decision) and a full SquadronOps live-test pass. Both deferred to a dedicated future session.

---

## ✅ WorkSection: AcType → AcMainGroup restructure (real bug, fully fixed)

**Root cause:** `WorkSection` was originally keyed on `AcTypeId`, back when `AcMainGroup`'s seeded data had drifted (fixed in the earlier RBAC session) and wasn't safe to use. Never migrated back after that fix — result: F16C/F16D and F5E/F5F each had duplicate `WorkSection` rows (ELEC/ELECTRO/GTR/HYD ×2) instead of one shared row per real aircraft family.

**Fixed end-to-end:**
- Hand-run SQL migration: backfilled `AcMainGroupId`, repointed `WorkOrderSection.WorkSectionId` off the 8 losing duplicate rows onto their surviving counterparts, deleted duplicates, dropped `AcTypeId`+FK, added new FK — verified 24→16 rows, zero duplicate (AcMainGroup, Code) pairs
- `WorkSection.cs`, `IWorkSectionRepository`/`WorkSectionRepository`, `WorkSectionsController.cs` (scope check simplified — no more AcType lookup needed), `WorkSectionViewModels.cs` + new `AcMainGroupLookupViewModel.cs`, `_Form.cshtml`/`Index.cshtml`/`Delete.cshtml`, `WorkOrderSectionsController.PopulateDropdownsAsync` (resolves AcType→AcMainGroup), `WorkSectionSeeder.cs` (now seeds once per family, not per AcType)
- EF migration history synced via an intentionally-empty `SyncWorkSectionAcMainGroup` migration (DB already matched the target state from the SQL script; this migration exists only to regenerate `FRAContextModelSnapshot.cs`) — confirmed correct in the final snapshot

**Not another instance of the same bug:** `JobCard`/`InspectionType`/`MaintenanceProgram` remain correctly `AcTypeId`-scoped by design — confirmed with Dadda these differ genuinely per real variant (e.g. PE1-PE6 seeded specifically for F5E from the real TO manual, not shared with F5F).

---

## ✅ Account / access-control cleanup (surfaced by live-testing)

**Root problem found via live test:** `Users/Edit` had *three* overlapping "looks like access control but isn't" mechanisms — legacy `ApplicationUser.Base/Squadron/AcMainGroup/Department/Wing` fields (some fed into user *claims* too, unused), a 7-checkbox Roles list where only `Admin` was ever checked by `ModuleAccessHandler`, and the real system (`UserAssignment`, on a separate unlinked screen). A scoped F5 tech's Edit screen showed "AIRCRAFT MAIN GROUP: F-16" — real access was correctly F5 (via `UserAssignment`); the legacy field was just stale and misleading.

**Fixed, not just labeled:**
- `Department`/`Wing` removed entirely from `RegisterUserViewModel`/`EditUserViewModel`/`UsersController`/`Create.cshtml`/`Edit.cshtml`/`Index.cshtml` (dead everywhere — confirmed via solution-wide search)
- 7-checkbox Roles list collapsed to a single `IsAdmin` toggle (the only one that ever did anything)
- `Base`/`Squadron`/`AcMainGroup` kept but honestly labeled ("informational only" / "SquadronOps default only") — real, but only as Create-time convenience defaults, never authorization
- **New `IsLastAdminAsync()` guard** added to `Edit` (blocks removing Admin role or deactivating the last admin) and `Delete` (previously only blocked self-delete) — this was one of the "4 guard rules" documented back in Phase 1 that had never actually been implemented in the real file
- `Details.cshtml`/`Edit.cshtml` now link directly to the real `UserAssignments` screen
- `LastLoginUtc` was **never written anywhere** — fixed in `Login.cshtml.cs` (`PasswordSignInAsync` succeeding now actually stamps it)
- Stale-`ReturnUrl` bug: logging in as a fresh non-admin after an Admin session left a privileged `ReturnUrl` (e.g. `/Users`) bounced the non-admin straight to AccessDenied instead of Home. Fixed with a small, explicitly-scoped allowlist (`/Users`, `/Roles`, `/UserAssignments`, `/Settings` — the fixed "platform administration" bucket, distinct from per-module policy-gated areas which correctly still show AccessDenied) — both scenarios re-tested live and confirmed correct

**Deliberately not deleted:** `ApplicationUser.DepartmentId`/`WingId` remain on the model — only removed from the admin UI. Confirmed with Dadda this is intentional: HR (Base+Department) and Healthcare (Base+HealthCenter) are on the roadmap after SquadronOps, and the right extension point when they're built is adding nullable columns to `UserAssignment` (same pattern as this session's `WingId` addition), not resurrecting the legacy single-value fields.

**Known loose end, not urgent:** `AppClaimsPrincipalFactory.cs` still mints a `WingId` claim from the legacy field on every login; nothing consumes it anymore. Safe to clean up whenever, not blocking.

---

## ✅ Snag module — bug audit

Of the "three bugs fixed after Snag delivery" flagged in the RBAC handoff as unconfirmed: **2 of 3 confirmed fixed** with visible before/after evidence still in the code (MTBF division producing false `0` instead of `null` when no flight-hour data exists; `NullReferenceException` for scoped users in `SnagsController.Index` from relying on an un-included nav property). **Third bug's identity unknown** — never identified, not re-discovered by inspection either.

**New gap found and fixed along the way:** `SnagService.CloseAsync` was missing the "already closed" guard that `DeferAsync`/`LinkToWorkOrderAsync` both had — re-closing a closed snag silently overwrote `ClosedAt`/`ClosedByUserId`, destroying the original closure record. Now returns `(false, "Snag déjà clôturé.")` consistently.

---

## ✅ BugReport module — polish pass

- Floating report button: confirmed already correctly placed (outside `<header>`, uses 🐛 emoji) — this item was already done, no action needed
- Triage status dropdown: fixed — `Html.GetEnumSelectList<BugStatus>()` wasn't marking the current status as selected (not `asp-for`-bound), so it silently defaulted to the first enum value every time
- Added `BugReports/Index` link to the Admin sidebar menu (appropriate placement — `Index`/triage is Admin-relevant; the floating button already covers "any user can report" via `Create`)
- `Index` extended: defaults to open-only (`NEW`/`CONFIRMED`/`IN_PROGRESS`) with a checkbox to include closed; free-text search by reporter name (Dadda's own stated reasoning: "maybe one user is happy to report normal behavior as a bug"); sortable columns (Severity/Status/Reporter/Date) — all done in-memory in the controller, consistent with the existing code's own approach, no repository changes

---

## Key lessons reinforced this session

- **"Documentation drift runs in both directions" struck again, twice**: the RBAC handoff's claim that `WingsController` didn't exist was wrong (it did, just needed auth); `UserAssignmentsController`'s own comment claiming "Wing has no CRUD screen yet" was the same stale claim baked into a different file.
- **Compiling clean ≠ working correctly.** Several real bugs (the `WorkSection` duplication, the misleading `Users/Edit` fields, the stale-`ReturnUrl` redirect) were only found by actually clicking through the app as a real scoped user — not by code review or successful builds. Live-testing earned its place as a first-class step, not an afterthought.
- **"Fix it for good, not patch it every time"** — Dadda's explicit framing for the `WorkSection` issue — is the right bar for anything AcType/AcMainGroup-shaped going forward; worth checking new modules against the same question before they ship.
- **Hand-run SQL + intentionally-empty EF migration** is now an established, working technique in this codebase for "DB altered out-of-band, need to resync the migration history" — documented in the `WorkSection` fix, reusable if it comes up again.

---

## On the horizon

- Component / Life-Limited Parts tracking — **next module, not yet started**
- Aircraft Dispatch / Airworthiness Status engine — deferred, needs Snag deferral limits + InspectionState overdue + MEL/SB compliance (unmodeled) + command-authority override
- Real user-manual screenshots — deliberately deferred until the whole Maintenance process is complete, then captured per-functionality, organized by user privilege
- SquadronOps `OdvsController` live-vs-dead decision + SquadronOps live-test pass — parked, dedicated session
- `AppClaimsPrincipalFactory.cs` dead `WingId` claim — low priority, whenever

---

*End of Handoff — AL-BORAK GMAO FRA · RBAC Rollout / WorkSection Fix / Account Cleanup Session*
