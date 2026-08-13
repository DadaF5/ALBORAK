# Session Summary — Aircraft Maintenance Module: WorkOrder & Formule 12/13 Subsystem

**Scope of this session:** Built out the entire `AircraftMaintenance` execution
pipeline from `InspectionType` through a complete `WorkOrder` lifecycle,
including the real Formule 12/13 paper-form subsystem, grounded in actual
scanned reference documents (F-5E TO manual, real Formule 12/13 forms from
2ème BAFRA / SGMA1). This picks up directly after the earlier
`InspectionType`/`JobCard` foundational work.

---

## 1. What's fully built, migrated, and working

### Lookup data (seeded, idempotent)
- 11+ base lookup tables (`AcCategory`, `AcStatusType`, `EmployingAuthority`,
  `Country`, `CdnDocType`, `MissionRole`, `ImmatriculationDocType`,
  `AircraftManufacturer`, `AcMainGroup`, `Base`) — all field-shapes
  **confirmed against real model files** this session (no more guessed
  "ASSUMED SHAPE" seeders remaining).
- `AcType` — refactored from a table-wide `AnyAsync()` guard to **per-code
  idempotent** seeding (critical fix — the old guard silently blocked
  adding new types once any row existed). Now seeds: `F16C`, `F16D`,
  `C130H`, `F5E`, `F5F`, `AJET`. F16D and F5F/F5E are sibling variants
  sharing the same family (confirmed by user: single-seat vs dual-seat).
- `AircraftManufacturerSeeder` — same idempotent refactor, includes
  `DASSAULT` (Dassault-Dornier, for Alpha Jet).
- `Aircraft` — refactored to per-registration idempotent seeding; includes
  real reference test aircraft `CN-AOG`/`LM 4713` (from original handoff
  doc) plus new F5E/F5F/AJET aircraft (placeholder registrations, flagged
  as such).

### InspectionType (pre-existing, extended this session)
- Fixed broken Bootstrap-collapse "Options avancées" toggle (removed
  entirely — same fix pattern as JobCard/MaintenanceProgram).
- **`ManagePrograms`** — new page to add/remove linked `MaintenanceProgram`s
  via the `InspectionTypeProgram` junction (was previously read-only/empty).
- Index now shows a "Programmes" column (badge codes) + direct "Programmes"
  action button per row, plus filter-by-AcType dropdown (sourced from the
  **full** AcType list, not just types with existing data — this was a
  recurring bug pattern, fixed here and in WorkSections).
- Real PE1–PE6 seeded for **F5E** (corrected — originally mis-attributed to
  C130H, since Table 2-1 data is from the F-5E manual `TO XX1F-5E-6WC-3`).
  Intervals: 300/600/900/1200/1500/1800h. PE7/PE8 intervals not seeded
  (partially obscured in source photo).

### MaintenanceProgram
- Full CRUD. Seeded: `SP-9`/`SP-29` (C-130H, from original handoff doc's
  dual-threshold example) + `PE1`–`PE6` (F5E, new, linked via
  `InspectionTypeProgram` 1:1 to their matching `InspectionType`).

### JobCard
- Full CRUD, extended with real-world fields discovered from an actual
  F-5E job card photo: `WorkAreas`, `MechNo`, `ElectricalPowerRequired`,
  `FigureRef`. `Specialty` dropdown includes `APG` (real code found on
  the card, not in original guessed list).
- **`AtaId`** — real FK to `Ata` lookup (replaced free-text `AtaCode`,
  which is kept but no longer written to, for backward compat).

### Ata / AtaCategory
- `Ata` — standard `LookupBase` lookup, table `ATA`, 68 real chapters
  seeded from ATA iSpec 2200 (not ICAO — corrected earlier misconception),
  including helicopter chapters (62–67) since fleet includes Apache/Puma.
- `AtaCategory` — 4 categories (Aircraft General, Airframe Systems,
  Structure, Power Plant), FK on `Ata`.
- **Sub-ATA deliberately NOT built** — confirmed chapter-level is
  sufficient for real job cards.

### ProgramJobCard
- Bulk range-assign junction (`MaintenanceProgram` ↔ `JobCard`) — critical
  because PE1 alone spans 85 cards (`1-001`→`1-085` per real Table 2-2
  data). "Manage" page has a `[ResponseCache(NoStore)]` fix for a real
  browser-caching bug (newly-added JobCards not showing in the dropdown
  until hard refresh).

### WorkOrder — full lifecycle
- **Multi-InspectionType support**: `WorkOrderInspectionType` junction
  (NOT a singular FK) — critical fix, since Table 2-1 shows multiple
  InspectionTypes coinciding at the same hour milestone (e.g. 1200h =
  PE1+PE2+PE4 together). Old `InspectionTypeId` kept nullable for
  backward compat, no longer source of truth.
- **Create → Open → PopulateJobCards → Execute → Close** workflow, all
  built and tested.
- **Job cards auto-resolved** (not manually picked): the system walks
  `WorkOrderInspectionType` → `InspectionTypeProgram` → `MaintenanceProgram`
  → `ProgramJobCard` → `JobCard` automatically, presents a pre-checked
  checkbox list grouped by program. User confirmed this design directly
  from a real 1200h/PE1+PE2+PE4 example.
- **AcType-mismatch validation**: server-side + client-side (AJAX-driven
  checkbox filtering), prevents selecting an InspectionType that doesn't
  match the chosen aircraft's type.
- **Duplicate-scheduling prevention**: can't schedule the same
  InspectionType twice while an earlier active (non-CLOSED) WorkOrder
  already covers it for the same aircraft.
- **Due-status-aware Create screen**: new `GetSelectableInspectionTypes`
  AJAX endpoint excludes InspectionTypes with computed status `OK` (not
  yet due) — only shows `OVERDUE`/`ALERT`/`UNKNOWN`. Uses
  `InspectionStatusCalculator` (see below).
- **Landings snapshot** added (`OpenLandings`/`CloseLandings`), matching
  existing Hours/Cycles pattern.
- **`InspectionState` auto-update at Close()`** — for each linked
  InspectionType, computes and writes `NextDueHours`/`NextDueCycles`/
  `NextDueDate` + `StatusSnapshot` via `InspectionStatusCalculator`.

### InspectionStatusCalculator (`Areas/AircraftMaintenance/Services/`)
- Shared, reusable service computing `OVERDUE`/`ALERT`/`OK`/`UNKNOWN`
  status and next-due values from an `InspectionType`'s interval/tolerance
  settings. Used by: `WorkOrder.Close()`, the Create-screen AJAX filter,
  `DueList`, and `DamDashboard.TotalDueSoon`. **This is the single source
  of truth for due-date math — reuse it, don't reimplement.**

### DueList
- New read-only view: one row per Aircraft × InspectionType (including
  never-done = `UNKNOWN`), filterable by AcType and status, summary badge
  counts.

### DamDashboard
- `Kpi.TotalDueSoon` wired to real count (OVERDUE+ALERT). **`DueSoon` the
  LIST is still `[]`** — needs `DamDashboardVm.cs`'s exact class shape to
  populate correctly (not provided this session).

### WorkSection + Formule 12/13 subsystem (the big new piece)
Built from **real scanned Formule 12/13 documents** (2ème BAFRA, unit
SGMA1, aircraft F5F "944"). Confirmed structure: **one Formule 12
(WorkOrder) → many Formule 13 (WorkOrderSection), one per responsible
section** (INST, RDR, GTR, etc.) — standard MRO "task card per trade"
pattern, confirmed by end-user feedback as matching international
practice.

- **`WorkSection`** — AcType-scoped lookup (sections differ per aircraft
  type/family). Starter seed: `ELEC`, `ELECTRO`, `HYD`, `GTR` across all
  6 AcTypes (F16C, F16D, C130H, F5E, F5F, AJET). Intentionally
  incomplete — user adds more via UI as identified from real forms.
- **`WorkOrderSection`** — the Formule 13 header (form number, organisme
  responsable, type de travail, dates, temps alloué/passé
  Systématique+Retouche, vieillissement, directives + T.O. reference).
- **`WorkOrderSectionPart`** — Tableau II (equipment exchange). Old/new
  part fields side-by-side in one row (relational shape, not the paper's
  alternating Ancien/Nouveau rows).
- **`WorkOrderSectionTask`** — Tableau III (travaux effectués). Free-text
  task description (embeds T.O. paragraph refs like `1F.SF.2.8.1.1`, not
  structurally parsed — format varies too much in real data).
- **`WorkOrderSectionSignOff`** — the REAL 4-level chain confirmed from
  scans: **Chef AT/SEP → Chef SCQ → Chef ST → Chef SGMA1** (NOT the
  originally-guessed `TECHNICIEN/APRS/NAVIGABILITÉ/COMMANDANT` from the
  old handoff doc — that's now understood to be a different, SEPARATE
  concept: `WorkOrderJobCardSignOff`, kept for the PE/JobCard-driven
  workflow). **Both sign-off systems are intentionally kept** — they
  serve genuinely different real workflows (structured PE/JobCard
  periodic work vs ad-hoc/directive-driven section work).
- Fixed 4 sign-off rows are auto-created (idempotent) whenever the Visas
  page — or Print — is opened for a section.
- Electronic attestation pattern used throughout Part/Task/SignOff:
  timestamp set automatically when a name field is filled — NOT real
  signature image capture (flagged clearly, in case that's a future ask).

### Print.cshtml — full rebuild
- Was a generic placeholder; now renders the REAL structure: Formule 12
  header page, then **one full Formule 13 page per WorkOrderSection**
  (Tableau I directives, Tableau II parts, Tableau III tasks, real 4-level
  visa table — signed rows green, unsigned rows blank lines).
- Verified against real scanned examples via a static HTML preview
  (rendered to PNG via `wkhtmltoimage`, embedded as real figures in the
  user manual).

### Documentation deliverables
- `Guide_Creation_OT_Planifie.md` — French end-user quick guide (planned
  inspection workflow), kept in sync as design changed (e.g. auto-resolved
  job cards).
- `Guide_Utilisateur_OT_ALBORAK.docx` — full 10-page Word manual, all 10
  phases (Create → Print, plus the Formule 12/13 subsystem), glossary,
  status table, **2 real embedded figures** (genuine renders of the actual
  print template with sample data) via `docx` npm library. TOC is
  **static** (not a Word auto-field — those render blank until manually
  refreshed, a real bug hit and fixed this session), page numbers
  hand-verified against the actual rendered PDF.

### UI/platform bugs fixed this session
- Global CSS: `.table-hover` text was invisible (black-on-black) on every
  table site-wide — Bootstrap's default hover style assumes light
  backgrounds. Fixed once in `_Layout.cshtml`, applies everywhere.
- `WorkOrder` Details page action buttons redesigned — was cramped/wrapped
  into 2 rows; now `justify-content: space-between` with Imprimer visually
  promoted (larger, blue, shadow) since it's a distinct important action.
- Razor `@section` reserved-keyword collision (loop variable named
  `section` broke `Print.cshtml` — renamed to `sec`). **General lesson:
  avoid naming Razor variables `section`/`page`/`layout`/`model`/etc.**

---

## 2. Explicitly deferred / open items (nothing forgotten, just not started)

| Item | Status |
|---|---|
| **`AircraftVersion`** (Serie 74/78/XI) | **Still waiting on user** — need the exact F5E-vs-F5F split before seeding |
| **`Snag` entity + `WorkOrderSnag` junction** | **Designed, not built.** Discussed: Snag needs ATA, severity, discovery phase, position-at-discovery snapshot, status lifecycle (OPEN→LINKED→CLOSED). Corrective WorkOrders should link via junction (many-to-many, same lesson as WorkOrderInspectionType — a WO might resolve multiple snags at once). Should auto-close linked Snags when WO closes. Corrective WOs likely execute via `WorkOrderSection`/Task (ad-hoc, no pre-defined JobCard), not the PopulateJobCards flow. |
| `DamDashboard.DueSoon` (the list) | Needs `DamDashboardVm.cs`'s real class shape |
| Role-based authorization | Real gap — everything is `[Authorize(Roles = "Admin")]` only, doesn't use the pre-existing `ModuleRole`/`UserAssignment` system from before this session |
| `UserAssignment`/`BaseId` scoping migration | Big, pre-existing parked project, predates this session entirely |
| Component/Life-Limited Parts tracking | Big, explicitly parked — engine hours/cycles, component S/N tracking, install/uninstall events. User's own reasoning: too big to bolt on mid-flow. |
| `JobCardStep` (per-line Work Unit Code breakdown) | Minor, low priority, real card photos show this exists but deferred |
| Real screenshots for the user manual | Only 2 figures done (genuine Print-template renders). Interactive screens (Create form, checkboxes, Visas page) need real screenshots from the live app — offered, not yet provided |
| `AcMainGroup` "family" semantics | Real observation: `AcMainGroup` was *originally* documented as "Aircraft Family" (e.g. one F-16 group containing both C/D variants), but actual seeded data (`CHASSE-2B`/`TRANS-2B`) drifted to mission/base-scoped categories. Flagged, NOT fixed — `WorkSection` uses simpler AcType-level duplication instead to avoid another migration. Revisit only if it becomes a real pain point. |

---

## 3. Known technical assumptions — verify if something doesn't compile

- **`_uow.Aircraft`** — assumed property name for the pre-existing (Phase 1,
  predates this session) Aircraft repository. Never confirmed against the
  real `IUnitOfWork.cs`.
- **`ApplicationUser`** — assumed to live in `FRAProject.Models` namespace
  (confirmed once via `WorkOrderJobCardSignOff.cs`, reused since).
- **`FullLabel`** on `ApplicationUser` — assumed to exist (per project's own
  memory notes about User Management catalog), used for
  `OpenedByUserName`/`ClosedByUserName` display.

All lookup-shape assumptions from earlier in the project (MissionRole,
ImmatriculationDocType, AircraftManufacturer) were **confirmed against
real files this session** — no longer flagged as risky.

---

## 4. Real reference data this session is grounded in

- **TO XX1F-5E-6WC-3** — F-5E maintenance manual, source of Table 2-1
  (periodicity: PE1=300h...PE6=1800h) and Table 2-2 (job card ranges per
  PE, e.g. PE1 = cards `1-001`–`1-085`).
- **Real F-5E job card photo** (Card 1-036) — source of JobCard's extended
  fields (WorkAreas, MechNo, ElectricalPower, FigureRef, Specialty=APG).
- **Real scanned Formule 12/13 forms** — 2ème BAFRA, organisme SGMA1,
  aircraft F5F "944", form number pattern `25128/INST`, real sign-off
  chain (Chef AT/SEP, SCQ, ST, SGMA1), real T.O. paragraph reference
  format (`1F.SF.2.8.1.1`).

---

## 5. Golden rules reinforced this session (in addition to pre-existing ones)

- **Seeders must be per-code/per-registration idempotent, never a
  table-wide `AnyAsync()` guard** — the latter silently blocks adding new
  rows once any row exists. Hit this bug twice (`AcTypeSeeder`,
  `MaintenanceProgramSeeder`) before establishing the pattern everywhere.
- **A "type" or "period" concept that can occur in combination with
  siblings needs a junction, never a singular FK** — proven twice now
  (`WorkOrderInspectionType`, and the `Snag`/`WorkOrderSnag` design
  applies the same lesson preemptively).
- **Filter dropdowns must source from the full authoritative lookup list,
  not just distinct values present in the current dataset** — hit this
  bug on `InspectionTypes` and `WorkSections` Index filters.
- **Don't guess a domain model's real-world field shape from a design doc
  when a real reference document/photo is available** — corrected the
  C130H/F5E misattribution this way, and it's why the whole Formule 12/13
  subsystem is grounded in actual scans rather than invented structure.
- **When user provides real domain photos/scans, always re-derive
  structure from them, even if it contradicts earlier assumptions** (e.g.
  the sign-off chain was completely wrong before the scans were shared).

---

## 6. Suggested next steps, in likely priority order

1. `Snag` + `WorkOrderSnag` — design already discussed, ready to build.
2. `AircraftVersion` seeding — just needs your Serie 74/78/XI split.
3. Real screenshots for the user manual (Create form, job card checklist,
   Visas page) to replace the "text-only phase" gaps.
4. `DamDashboardVm.cs` → finish `DueSoon` list.
5. Role-based authorization redesign (bigger, own conversation).
