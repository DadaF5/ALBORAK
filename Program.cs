using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Repositories;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.Settings.Interfaces;
using FRAProject.Areas.Settings.Repositories;
using FRAProject.Authorization;
using FRAProject.Data;
using FRAProject.Infrastructure;
using FRAProject.Infrastructure.Authorization;
using FRAProject.Infrastructure.Identity;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.Services.Medical;
using FRAProject.Support.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add DB Context
builder.Services.AddDbContext<FRAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FRAConString")));

// Identity: use the "default identity" registration which includes the Identity UI.
// We add roles as well so role checks still work.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    // adjust password/lockout options for dev as needed
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FRAContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI(); // ensures /Identity/Account/Login etc are available

// Custom claims factory for user identity customization
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
builder.Services.AddScoped<IUserAssignmentService, UserAssignmentService>();

// Program.cs
builder.Services.AddScoped<IUserScopeService, UserScopeService>();

// registration
builder.Services.AddScoped<IAuthorizationHandler, ModuleAccessHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MaintenanceRead", p => p.Requirements.Add(new ModuleAccessRequirement("MAINTENANCE")));
    options.AddPolicy("MaintenanceWrite", p => p.Requirements.Add(new ModuleAccessRequirement("MAINTENANCE", requireWrite: true)));

    options.AddPolicy("SquadronOpsRead", p => p.Requirements.Add(new ModuleAccessRequirement("SQUADRONOPS")));
    options.AddPolicy("SquadronOpsWrite", p => p.Requirements.Add(new ModuleAccessRequirement("SQUADRONOPS", requireWrite: true)));

    options.AddPolicy("HRRead", p => p.Requirements.Add(new ModuleAccessRequirement("HR")));
    options.AddPolicy("HRWrite", p => p.Requirements.Add(new ModuleAccessRequirement("HR", requireWrite: true)));

    options.AddPolicy("HealthcareRead", p => p.Requirements.Add(new ModuleAccessRequirement("HEALTHCARE")));
    options.AddPolicy("HealthcareWrite", p => p.Requirements.Add(new ModuleAccessRequirement("HEALTHCARE", requireWrite: true)));

    // SETTINGS deliberately has no policy here — per Module.cs's own doc
    // comment ("← admin only"), it stays [Authorize(Roles = "Admin")].
});



// support for snags , errors, bugs, issues
builder.Services.AddScoped<IBugReportRepository, BugReportRepository>();


// Authorization handlers & policies
builder.Services.AddSingleton<IAuthorizationHandler, SameSquadronHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SameBaseHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SquadronOrBaseMaintenanceHandler>();

// Register repositories and unit of work if applicable UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAcMainGroupRepository, AcMainGroupRepository>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IDossierService, DossierService>();


// Program.cs DI — add alongside existing Maintenance Phase 2 registrations
builder.Services.AddScoped<ISnagService, SnagService>();
builder.Services.AddScoped<ISnagStatisticsService, SnagStatisticsService>();

// Aircraft Maintenance domain services
builder.Services.AddScoped<IAircraftReadingProvider, AircraftReadingProvider>();
builder.Services.AddScoped<IComponentLifeStatusCalculator, ComponentLifeStatusCalculator>();
builder.Services.AddScoped<IComponentScopeHelper, ComponentScopeHelper>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();
builder.Services.AddScoped<IComponentLifeLimitProfileService, ComponentLifeLimitProfileService>();
builder.Services.AddScoped<IComponentService, ComponentService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameSquadron", p => p.Requirements.Add(new SameSquadronRequirement()));
    options.AddPolicy("SameBase", p => p.Requirements.Add(new SameBaseRequirement()));
    options.AddPolicy("SquadronOrBaseMaintenance", p => p.Requirements.Add(new SquadronOrBaseMaintenanceRequirement()));
    options.AddPolicy("RequireCrewChiefOrAdmin", p => p.RequireRole("CrewChief", "Admin"));

    // IMPORTANT: require authentication for every endpoint by default.
    // Controllers/actions or pages that should be public must use [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// If you scaffold or use Identity UI, register Razor Pages
builder.Services.AddRazorPages();

// =====================================
// DOMAIN SERVICES REGISTRATION
// Register domain-specific services with Dependency Injection
// Services are scoped to get a new instance per HTTP request (with FRAContext)
// =====================================

// Squadron Operations Domain Services
builder.Services.AddScoped<SquadronActivityService>();

// Medical Care Center Domain Services
builder.Services.AddScoped<IMedicalFitnessService, MedicalFitnessService>();

// UI/Menu Services (Cross-cutting)
builder.Services.AddScoped<IMenuService, MenuService>();

// Additional domain services can be registered here following the same pattern:
// builder.Services.AddScoped<IHRService, HRService>();
// builder.Services.AddScoped<IAircraftMaintenanceService, AircraftMaintenanceService>();

// MVC + JSON options (without global authorization)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure Authentication Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    
    options.LoginPath = "/Identity/Account/Login";  // Fixed for Razor Pages
    options.LogoutPath = "/Identity/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    // Handle AccessDenied redirect to Area
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Settings/Home/AccessDenied");
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// =====================================
// DATABASE SEEDING ON STARTUP
// Seeds reference data for development/testing
// Educational Purpose: Demonstrates domain data initialization
// =====================================

// Seed Identity: Roles and Admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeed.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred seeding Identity data (Roles/Admin).");
    }
}

// Seed database schema and reference data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<FRAContext>();
        
        // Ensure database schema is up to date
        await context.Database.MigrateAsync();

        // Seed reference data (Phases, Missions, Menus)
        await PhaseSeeder.SeedAsync(context);
        await MissionSeeder.SeedAsync(context);
        await MenuSeeder.SeedAsync(services);
        // Aircraft document reference tables
        await AircraftDocumentTypeSeeder.SeedAsync(context);

        // Optional demo docs (only if you want development data)
        await AircraftDocumentSeeder.SeedAsync(context);
        // ── Lookup tables (independent) ──────────────────────────────
        await AcCategorySeeder.SeedAsync(context);
        await AcStatusTypeSeeder.SeedAsync(context);
        await EmployingAuthoritySeeder.SeedAsync(context);
        await CountrySeeder.SeedAsync(context);
        await CdnDocTypeSeeder.SeedAsync(context);
        await MissionRoleSeeder.SeedAsync(context);
        await ImmatriculationDocTypeSeeder.SeedAsync(context);
        await AircraftManufacturerSeeder.SeedAsync(context);
        await BaseSeeder.SeedAsync(context);

        // ── Aircraft hierarchy (each seeder pulls its own FK parents too,
        //    so calling only the leaf is enough — but calling all explicitly
        //    here is harmless since every SeedAsync is idempotent) ────────
        await AcMainGroupSeeder.SeedAsync(context);
        await AcTypeSeeder.SeedAsync(context);
        await AircraftVersionSeeder.SeedAsync(context);
        await AircraftSeeder.SeedAsync(context);

        // Ata categories and ATA codes
        await AtaSeeder.SeedAsync(context);
        await InspectionTypeSeeder.SeedAsync(context);

        // ── Inspection Process ───────────────────────────────────────
        await MaintenanceProgramSeeder.SeedAsync(context);
        await WorkSectionSeeder.SeedAsync(context);


        logger.LogInformation("Reference data seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred during database seeding.");
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Area route MUST come first
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route second
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages so Identity UI (if used) is reachable at /Identity/Account/Login etc.
app.MapRazorPages();

app.Run();