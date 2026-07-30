# FRAProject - Domain-Driven Architecture

## Overview

This ASP.NET Core MVC application follows Domain-Driven Design principles with clear separation between four major operational domains. The architecture supports scalability and maintainability through modular organization and well-defined domain boundaries.

## Domain Architecture

### 1. HR Domain (Human Resources)
**Purpose**: Manages organizational structure, personnel records, and hierarchical relationships.

**Key Entities**:
- `Person` - Employee records with rank, department assignment, and personal information
- `Base` - Military bases/locations
- `Wing` - Wing-level organizational units
- `Squadron` - Squadron-level operational units
- `Department` - Administrative departments within bases
- `SubDepartment` - Sub-organizational units
- `Rank` / `RankType` - Military/organizational rank system

**Relationships**:
```
Base → Department → SubDepartment → Person
Wing → Squadron
Person ↔ CrewMember (1:1 optional for flight personnel)
```

### 2. Squadron Operations Domain
**Purpose**: Manages flight operations, mission planning, crew assignments, and daily flight scheduling.

**Key Entities**:
- `Sortie` - Individual flight missions
- `Odv` - Operational Daily Flight schedule
- `SortieCrew` - Crew assignments to sorties
- `CrewMember` - Flight crew operational profiles (linked 1:1 to Person)
- `Qualification` / `CrewMemberQualification` - Flight certifications
- `Mission` - Mission types (training, combat, etc.)
- `Phase` - Mission phases
- `CallSign` - Radio call signs for flights

**Relationships**:
```
Person → CrewMember (flight personnel)
CrewMember → MedicalCheck (fitness requirement)
Sortie → Aircraft (aircraft assignment)
Sortie → SortieCrew → CrewMember (crew assignments)
Squadron → Sortie (operational ownership)
```

**Critical Integration**: CrewMembers must have valid medical checks (MedicalDecision = FIT) to be assigned to sorties.

### 3. Aircraft Maintenance Domain
**Purpose**: Tracks aircraft inventory, maintenance status, and ensures flight safety through component tracking.

**Key Entities**:
- `Aircraft` - Aircraft inventory
- `AcType` - Aircraft models/types
- `AcCategory` / `AcMainGroup` - Aircraft classification
- `AcStatusType` - Operational status (Mission Capable, etc.)
- `MaintenanceComponent` - Trackable aircraft components
- `MaintenanceThreshold` - Maintenance interval thresholds
- `MaintenanceWorkOrder` - Maintenance tasks and repairs
- `FlightLog` - Flight hours and cycle tracking

**Relationships**:
```
Aircraft → AcType → AcMainGroup → AcCategory
Aircraft → MaintenanceComponent → MaintenanceThreshold
Aircraft → MaintenanceWorkOrder
Aircraft → Sortie (sortie assignments)
Aircraft.Serviceable gates Sortie assignment
```

**Critical Integration**: Aircraft availability (Serviceable flag and Status) directly impacts squadron operations scheduling.

### 4. Medical Care Center Domain
**Purpose**: Manages crew member medical fitness and determines flight eligibility.

**Key Entities**:
- `MedicalCheck` - Medical examination records
- `MedicalBilan` - Detailed examination results (lab work, physical exam, etc.)
- `MedicalFitnessResult` - Computed fitness status

**Relationships**:
```
CrewMember → MedicalCheck (1:Many)
MedicalCheck → MedicalBilan (1:Many detailed results)
MedicalCheck.Decision (FIT/UNFIT) gates Sortie assignment
```

**Critical Integration**: Medical fitness is REQUIRED for flight operations. The `MedicalFitnessService` evaluates the most recent medical check to determine if a crew member is FIT to fly.

## Shared Infrastructure

### Database Context
`FRAContext` (in `/Data/FRAContext.cs`) is the shared Entity Framework DbContext that provides access to all domain entities. The DbSets are organized with domain-specific comments for clarity.

### Dependency Injection
Services are registered in `Program.cs` with domain-specific organization:
```csharp
// Squadron Operations Domain Services
builder.Services.AddScoped<SquadronActivityService>();

// Medical Care Center Domain Services
builder.Services.AddScoped<IMedicalFitnessService, MedicalFitnessService>();

// UI/Menu Services (Cross-cutting)
builder.Services.AddScoped<IMenuService, MenuService>();
```

### Navigation Properties
The domain entities use EF Core navigation properties to establish relationships:
- **One-to-One**: Person ↔ CrewMember (optional)
- **One-to-Many**: Squadron → CrewMember, Aircraft → MaintenanceWorkOrder
- **Many-to-Many**: Sortie ↔ CrewMember (via SortieCrew join entity)

## Adding Sample Data (Seeding)

### Existing Seeders
The application includes several pre-configured seeders:
- **IdentitySeed**: Creates user roles and admin account
- **PhaseSeeder**: Seeds mission phases
- **MissionSeeder**: Seeds mission types
- **MenuSeeder**: Seeds application menu structure

### Adding Domain-Specific Sample Data

To add sample data for development/testing, create seeder classes in the `/Data` folder following this pattern:

```csharp
public class DomainSeeder
{
    public static async Task SeedAsync(FRAContext context)
    {
        // Skip if data already exists
        if (context.YourEntities.Any())
            return;

        // Create sample data
        var sampleData = new List<YourEntity>
        {
            new YourEntity { /* properties */ },
            // ... more entities
        };

        context.YourEntities.AddRange(sampleData);
        await context.SaveChangesAsync();
    }
}
```

**IMPORTANT: Respect Foreign Key Dependencies**

When seeding multiple domains, follow this order:
1. **HR Domain** - Organizational structure and personnel (foundation)
2. **Aircraft Maintenance** - Aircraft inventory
3. **Squadron Operations** - Flight operations (depends on HR and Aircraft)
4. **Medical Care** - Medical checks (depends on CrewMembers)

Register your seeders in `Program.cs`:
```csharp
// In the database seeding section
await HRSeeder.SeedAsync(context);
await AircraftMaintenanceSeeder.SeedAsync(context);
await SquadronOpsSeeder.SeedAsync(context);
await MedicalCareSeeder.SeedAsync(context);
```

### Example: Seeding HR Data

```csharp
// Seed Bases
var bases = new List<Base>
{
    new Base { Id = 1, BaseName = "Air Base Alpha" },
    new Base { Id = 2, BaseName = "Air Base Bravo" }
};
context.Bases.AddRange(bases);
await context.SaveChangesAsync();

// Seed Departments
var departments = new List<Department>
{
    new Department { Id = 1, Name = "Operations", BaseId = 1 },
    new Department { Id = 2, Name = "Maintenance", BaseId = 1 }
};
context.Departments.AddRange(departments);
await context.SaveChangesAsync();

// Seed SubDepartments
var subDepartments = new List<SubDepartment>
{
    new SubDepartment { Id = 1, Name = "Flight Operations", DepartmentId = 1 }
};
context.SubDepartments.AddRange(subDepartments);
await context.SaveChangesAsync();

// Seed Ranks and Persons (employees)
// ... (continue with related entities)
```

## Configuration

### Database Connection String
Update `appsettings.json` with your SQL Server connection:
```json
{
  "ConnectionStrings": {
    "FRAConString": "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Running Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Key Design Principles

1. **Domain Separation**: Each domain has its own entities, services, and business logic
2. **Shared Context**: Single DbContext for simplified transactions and relationship management
3. **Navigation Properties**: EF Core navigation properties enforce referential integrity
4. **Dependency Injection**: Services are registered per domain for clear responsibility
5. **Educational Comments**: Code includes comments explaining domain relationships and business rules

## Cross-Domain Integration Points

### Person → CrewMember
HR employees who are flight crew have an optional 1:1 relationship to CrewMember for operational tracking.

### CrewMember → MedicalCheck → Sortie
Medical fitness (FIT status) is required before crew can be assigned to flights.

### Aircraft → Sortie
Aircraft must be serviceable (Serviceable = true, Status = Available) to be assigned to sorties.

### Squadron → Sortie → CrewMember
Squadrons plan sorties and assign their crew members based on qualifications and medical fitness.

## Best Practices

1. **Always check medical fitness** before assigning crew to sorties using `IMedicalFitnessService`
2. **Verify aircraft availability** before sortie assignment
3. **Respect organizational hierarchy** when querying personnel (Base → Department → SubDepartment → Person)
4. **Use navigation properties** instead of manual joins for better performance
5. **Follow seeding order** to avoid foreign key constraint violations

## Future Extensibility

The modular domain structure supports easy addition of new domains:
1. Create domain-specific models in `/Models`
2. Add DbSets to `FRAContext` with domain comments
3. Create domain services in `/Services`
4. Register services in `Program.cs`
5. Create corresponding controllers and views

This architecture ensures that adding new functionality to one domain doesn't impact others, promoting maintainability and scalability.
