# Cross-Area Authorization and Scope Design

## Why this design is needed

The repository already has the first pieces of scoping:

- `ApplicationUser` stores single-value organization hints such as `BaseId` and `AcMainGroupId` (`/Models/ApplicationUser.cs`)
- the claims factory emits single `BaseId` and `SquadronId` claims (`/Infrastructure/Identity/AppClaimsPrincipalFactory.cs`)
- current policies are route-based and assume one base or one squadron at a time (`/Infrastructure/Authorization/*.cs`)

That baseline is useful, but it is too narrow for:

- cross-area authorization (`Maintenance`, `SquadronOps`, `HR`, `Healthcare`, ...)
- reassignment history
- temporary multi-scope assignments
- read-only oversight roles
- “same area, different interpretation of scope” rules

The cleanest platform design is to make **area assignment** the source of truth, and make **scope values** generic metadata attached to each assignment.

---

## Design goals

1. Keep authorization generic at platform level
2. Let each area define and interpret its own scope dimensions
3. Preserve assignment history instead of overwriting it
4. Support multiple concurrent assignments for one user
5. Support read-only vs read-write access
6. Allow future areas to plug in without changing the core model

---

## Proposed generic platform models

```csharp
public enum AreaAccessLevel
{
    ReadOnly = 1,
    ReadWrite = 2
}

public class AppArea
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;      // MAINTENANCE, SQUADRON_OPS, HR, HEALTHCARE
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Optional resolver/service key used by the area at runtime.
    public string? ScopeResolverKey { get; set; }
}

public class AreaRole
{
    public int Id { get; set; }
    public int AppAreaId { get; set; }
    public AppArea AppArea { get; set; } = default!;

    public string Key { get; set; } = string.Empty;      // TECHNICIAN, BASE_SUPERVISOR, MASTER_SUPERVISOR
    public string Name { get; set; } = string.Empty;
    public AreaAccessLevel AccessLevel { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserAreaAssignment
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;

    public int AppAreaId { get; set; }
    public AppArea AppArea { get; set; } = default!;

    public int AreaRoleId { get; set; }
    public AreaRole AreaRole { get; set; } = default!;

    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }

    public bool IsPrimary { get; set; }
    public string? GrantedByUserId { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserAreaAssignmentScope> Scopes { get; set; } = new List<UserAreaAssignmentScope>();
}

public class AreaScopeDefinition
{
    public int Id { get; set; }
    public int AppAreaId { get; set; }
    public AppArea AppArea { get; set; } = default!;

    public string ScopeKey { get; set; } = string.Empty;     // Base, AcMainGroup, Squadron, Department, Person
    public string Name { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;    // EntityId, String, Number, Code
    public string? ReferenceEntityName { get; set; }         // Base, AcMainGroup, Department, ...
    public bool AllowsMultipleValues { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserAreaAssignmentScope
{
    public long Id { get; set; }
    public long UserAreaAssignmentId { get; set; }
    public UserAreaAssignment UserAreaAssignment { get; set; } = default!;

    public int AreaScopeDefinitionId { get; set; }
    public AreaScopeDefinition AreaScopeDefinition { get; set; } = default!;

    // Canonical value stored generically; interpreted by the area's resolver.
    public string ScopeValue { get; set; } = string.Empty;
    public string? ScopeLabel { get; set; }
}
```

### Why this is the cleanest supporting scope design

- `AppArea`, `AreaRole`, and `UserAreaAssignment` stay generic and stable
- scope is **not** hard-coded into user columns like `BaseId`, `DepartmentId`, `AcMainGroupId`
- scope is **not** hidden in one JSON blob, so it remains queryable, auditable, and time-bound
- each area can add new scope dimensions by inserting `AreaScopeDefinition` rows instead of changing platform tables

---

## Core runtime rules

### 1. Active assignments

At request time, an assignment is active when:

- `IsActive = true`
- `EffectiveFromUtc <= now`
- `EffectiveToUtc` is null or `EffectiveToUtc >= now`

### 2. Reassignment history

Never update old assignments in place when a user changes area role or scope.

Instead:

1. end-date the old `UserAreaAssignment`
2. create a new `UserAreaAssignment`
3. attach the new scope rows to the new assignment

This preserves historical auditability.

### 3. Temporary multi-scope access

A user may hold multiple overlapping active assignments in the same area.

Example:

- permanent `Maintenance / Technician / Base 1 / C130`
- temporary `Maintenance / Technician / Base 1 / Puma` for two weeks

The runtime authorization layer unions the readable scopes from active assignments, while write authorization must still match a specific active assignment that has `ReadWrite`.

### 4. Read-only vs write

`AreaRole.AccessLevel` is the platform decision point:

- `ReadOnly`: may query/list/open details inside allowed scope
- `ReadWrite`: may create/update/close/approve inside allowed scope

If the application later needs finer permissions, add an optional area-specific permission table on top of `AreaRole` without changing the assignment model.

---

## Maintenance area: runtime scope model

Maintenance should not use generic scope rows directly inside controllers and repositories. It should translate them once into a runtime model.

```csharp
public sealed class MaintenanceRuntimeScope
{
    public bool CanRead { get; init; }
    public bool CanWrite { get; init; }

    public IReadOnlySet<int> BaseIds { get; init; } = new HashSet<int>();
    public IReadOnlySet<int> AcMainGroupIds { get; init; } = new HashSet<int>();

    public bool HasAllBases { get; init; }
    public bool HasAllAcMainGroups { get; init; }
}
```

### Maintenance interpretation rules

Supported maintenance scope definitions:

- `Base`
- `AcMainGroup`

Interpretation:

- if an assignment has `Base` scope rows, it is limited to those bases
- if an assignment has no `Base` scope rows, it is global across bases
- if an assignment has `AcMainGroup` scope rows, it is limited to those groups
- if an assignment has no `AcMainGroup` scope rows, it can access all groups inside the already-allowed base set

This directly supports the requested operating modes.

### Requested role examples

#### Level 1 — Technician

Assignment:

- `AppArea = Maintenance`
- `AreaRole = Technician`
- `AccessLevel = ReadWrite`
- scope rows:
  - `Base = 1`
  - `AcMainGroup = C130`

Effective behavior:

- sees only C130 maintenance at Base 1
- cannot see C127, F16, F1, or Base 2
- can write only within Base 1 + C130

#### Level 2 — Base Supervisor

Assignment:

- `AppArea = Maintenance`
- `AreaRole = BaseSupervisor`
- `AccessLevel = ReadOnly`
- scope rows:
  - `Base = 1`
- no `AcMainGroup` rows

Effective behavior:

- sees all maintenance activity at Base 1
- cannot see Base 2
- cannot write anywhere

#### Level 3 — Master Supervisor

Assignment:

- `AppArea = Maintenance`
- `AreaRole = MasterSupervisor`
- `AccessLevel = ReadOnly`
- no `Base` rows
- no `AcMainGroup` rows

Effective behavior:

- sees all bases and all aircraft groups
- may filter by base or aircraft group in the UI
- cannot write anywhere

---

## How Maintenance should consume the framework

### 1. Resolve runtime scope once per request

Create an area service such as:

```csharp
public interface IMaintenanceScopeResolver
{
    Task<MaintenanceRuntimeScope> GetCurrentAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
```

The resolver:

1. loads active `UserAreaAssignment` rows for `AppArea = Maintenance`
2. loads attached `UserAreaAssignmentScope` rows
3. translates them into `MaintenanceRuntimeScope`

### 2. Filter queries by runtime scope

Maintenance repositories/services should apply:

```csharp
row.BaseId in scope.BaseIds || scope.HasAllBases
row.AcMainGroupId in scope.AcMainGroupIds || scope.HasAllAcMainGroups
```

For base-limited supervisors with no `AcMainGroup` rows:

- `HasAllAcMainGroups = true`
- `BaseIds = { 1 }`

### 3. Enforce writes separately

Write operations must require:

- authenticated user
- active maintenance assignment
- `CanWrite = true`
- target row within the assignment's effective base/group scope

This keeps read-only oversight roles safe even when they can see broad data.

### 4. Keep row ownership explicit in maintenance tables

Maintenance entities should continue to store their runtime ownership explicitly, for example:

- `BaseId`
- `AcMainGroupId`

That avoids leaking data through indirect joins and keeps filtering, indexing, and auditing straightforward.

---

## How other areas plug in

The same platform model can support other areas without changing the core tables.

### SquadronOps

Possible scope keys:

- `Base`
- `Wing`
- `Squadron`

### HR

Possible scope keys:

- `Base`
- `Department`
- `SubDepartment`

### Healthcare

Possible scope keys:

- `Base`
- `Department`
- `Person`

Each area provides its own resolver that interprets the same generic `UserAreaAssignmentScope` rows.

---

## Recommended migration path for this repository

1. keep current single-value user fields (`BaseId`, `AcMainGroupId`, `SquadronId`) as transitional profile/default fields
2. add the generic authorization tables above
3. move new authorization decisions to area assignment resolution
4. update claims creation so claims are derived from active area assignments instead of a single user column
5. replace route-only policies with resource-aware area policies

This keeps the current application working while creating a clean forward path.

---

## Final recommendation

For `ALBORAK`, the source of truth for authorization should be:

- **area** (`AppArea`)
- **role within that area** (`AreaRole`)
- **time-bound user assignment** (`UserAreaAssignment`)
- **generic assignment scope rows** (`UserAreaAssignmentScope`)

Maintenance then projects those generic rows into a `MaintenanceRuntimeScope` built from `Base` and `AcMainGroup`.

That gives the repository:

- clean cross-area consistency
- correct base/type isolation
- read-only oversight roles
- temporary multi-scope coverage
- preserved reassignment history
- extensibility for future modules without redesigning the platform again
