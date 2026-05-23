# FRAProject

Squadron flying program management application built with ASP.NET Core Razor Pages (.NET 8).

Provides a simple UI to plan and manage daily ODV (Operational Daily Flight) entries, including missions, call signs, squadrons, and aircraft groups.

## Key features
- Razor Pages / MVC-style controllers for UI pages
- Role-based behavior (Admin vs non-Admin)
- ViewModels to drive page forms and lists
- Server-side validation and antiforgery protection

## Quick start
Prerequisites:
- .NET 8 SDK
- SQL Server (or a supported database) if the app uses one

Run locally:
1. Clone the repository
2. Update `appsettings.json` with your connection string and secrets
3. Run:

```bash
dotnet restore
dotnet build
dotnet run --project FRAProject
```

Open `https://localhost:5001` (or the URL reported by the run output).

## Project layout
- `Controllers/` - MVC/Razor Page controllers (e.g., `OdvPlanningController`)
- `Views/` - Razor views for pages (e.g., `Views/OdvPlanning/Index.cshtml`)
- `ViewModels/` - page view models used to transfer data to views
- `wwwroot/` - static assets (CSS, JS)
- `appsettings.json` - configuration

## Documentation
- `DOMAIN_ARCHITECTURE.md` - current domain layout and integration points
- `docs/authorization-scope-design.md` - proposed cross-area authorization and scope design

## Contribution
Please review `docs/contributing.md` for contribution guidelines and PR process.
