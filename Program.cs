using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Repositories;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.Settings.Interfaces;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.Repositories;
using FRAProject.Authorization;
using FRAProject.Data;
using FRAProject.Data.Seeders;
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
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add DB Context
builder.Services.AddDbContext<FRAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FRAConString")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FRAContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Custom claims factory for user identity customization
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
builder.Services.AddScoped<IUserAssignmentService, UserAssignmentService>();
builder.Services.AddScoped<IUserScopeService, UserScopeService>();

// Authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, ModuleAccessHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SameSquadronHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SameBaseHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SquadronOrBaseMaintenanceHandler>();

// Policies (single AddAuthorization call)
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

    options.AddPolicy("SameSquadron", p => p.Requirements.Add(new SameSquadronRequirement()));
    options.AddPolicy("SameBase", p => p.Requirements.Add(new SameBaseRequirement()));
    options.AddPolicy("SquadronOrBaseMaintenance", p => p.Requirements.Add(new SquadronOrBaseMaintenanceRequirement()));
    options.AddPolicy("RequireCrewChiefOrAdmin", p => p.RequireRole("CrewChief", "Admin"));

    // NEW (Batch 11, 2026-08-29) — action-level gates for the two other
    // SquadronOps actor roles, same pattern as RequireCrewChiefOrAdmin
    // above (which existed already but was unused until this batch wired
    // it onto SortiesController.AssignAircraft). Roles are the ones
    // already seeded by IdentitySeed.cs. "Tower" IS "ATC" — confirmed by
    // Dadda, same real-world role, just named differently in
    // IdentitySeed.cs than in Dadda's own description.
    options.AddPolicy("RequireTowerOrAdmin", p => p.RequireRole("Tower", "Admin"));
    options.AddPolicy("RequireSquadronPlannerOrAdmin", p => p.RequireRole("SquadronPlanner", "Admin"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// support for snags/errors/bugs/issues
builder.Services.AddScoped<IBugReportRepository, BugReportRepository>();

// Register repositories and unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAcMainGroupRepository, AcMainGroupRepository>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IDossierService, DossierService>();

// Maintenance services
builder.Services.AddScoped<ISnagService, SnagService>();
builder.Services.AddScoped<ISnagStatisticsService, SnagStatisticsService>();

// Aircraft Maintenance domain services
builder.Services.AddScoped<IAircraftReadingProvider, AircraftReadingProvider>();
builder.Services.AddScoped<IComponentLifeStatusCalculator, ComponentLifeStatusCalculator>();
builder.Services.AddScoped<IComponentScopeHelper, ComponentScopeHelper>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();
builder.Services.AddScoped<IComponentLifeLimitProfileService, ComponentLifeLimitProfileService>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IComponentDerogationService, ComponentDerogationService>();

// Razor Pages for Identity UI
builder.Services.AddRazorPages();

// Domain services
// SquadronActivityService REMOVED (Batch 11, 2026-08-29) — confirmed by
// Dadda to be dead/legacy code (pre-restart leftover, same treatment as
// OdvsController.cs): computed DurationMinutes instead of using the locked
// manual-entry value, hardcoded Cycles=1, and its Aircraft/Component
// increment logic referenced nonexistent Maintenance model classes
// (MaintenanceComponent/MaintenanceThreshold/MaintenanceWorkOrder). Not
// referenced by anything real — safe to stop registering. Delete
// Areas/SquadronOps/Services/SquadronActivityService.cs (or wherever it
// lives) once confirmed nothing else still references the class directly.
// builder.Services.AddScoped<SquadronActivityService>();
builder.Services.AddScoped<IMedicalFitnessService, MedicalFitnessService>();
builder.Services.AddScoped<IMenuService, MenuService>();

// MVC + JSON options
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure Authentication Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Settings/Home/AccessDenied");
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// local helper for robust per-seeder execution
static async Task RunSeederAsync(string name, Func<Task> action, ILogger logger)
{
    try
    {
        await action();
        logger.LogInformation("{Seeder} OK", name);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "{Seeder} FAILED", name);
    }
}

// Seed Identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        await IdentitySeed.SeedRolesAndAdminAsync(services);
        logger.LogInformation("Identity seed completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred seeding Identity data (Roles/Admin).");
    }
}

// Seed database schema and reference data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<FRAContext>();

    try
    {
        await context.Database.MigrateAsync();

        logger.LogInformation("Provider: {Provider}", context.Database.ProviderName);
        logger.LogInformation("DB Connection: {Db}", context.Database.GetConnectionString());

        // Reference seeders (isolated so one failure doesn't block others)
        await RunSeederAsync(nameof(PhaseSeeder), () => PhaseSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(MissionSeeder), () => MissionSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(MenuSeeder), () => MenuSeeder.SeedAsync(services), logger);

        await RunSeederAsync(nameof(AircraftDocumentTypeSeeder), () => AircraftDocumentTypeSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(AircraftDocumentSeeder), () => AircraftDocumentSeeder.SeedAsync(context), logger);

        await RunSeederAsync(nameof(AcCategorySeeder), () => AcCategorySeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(AcStatusTypeSeeder), () => AcStatusTypeSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(EmployingAuthoritySeeder), () => EmployingAuthoritySeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(CountrySeeder), () => CountrySeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(CdnDocTypeSeeder), () => CdnDocTypeSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(MissionRoleSeeder), () => MissionRoleSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(ImmatriculationDocTypeSeeder), () => ImmatriculationDocTypeSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(AircraftManufacturerSeeder), () => AircraftManufacturerSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(BaseSeeder), () => BaseSeeder.SeedAsync(context), logger);

        await RunSeederAsync(nameof(AcMainGroupSeeder), () => AcMainGroupSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(AcTypeSeeder), () => AcTypeSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(AircraftVersionSeeder), () => AircraftVersionSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(AircraftSeeder), () => AircraftSeeder.SeedAsync(context), logger);

        // ATA diagnostics + seeding
        var ataCatBefore = await context.Set<AtaCategory>().CountAsync();
        var ataBefore = await context.Set<Ata>().CountAsync();
        logger.LogInformation("ATA before seed: Categories={AtaCatBefore}, Chapters={AtaBefore}", ataCatBefore, ataBefore);

        await RunSeederAsync(nameof(AtaSeeder), () => AtaSeeder.SeedAsync(context), logger);

        var ataCatAfter = await context.Set<AtaCategory>().CountAsync();
        var ataAfter = await context.Set<Ata>().CountAsync();
        logger.LogInformation("ATA after seed: Categories={AtaCatAfter}, Chapters={AtaAfter}", ataCatAfter, ataAfter);

        await RunSeederAsync(nameof(InspectionTypeSeeder), () => InspectionTypeSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(MaintenanceProgramSeeder), () => MaintenanceProgramSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(WorkSectionSeeder), () => WorkSectionSeeder.SeedAsync(context), logger);

        await RunSeederAsync(nameof(ComponentPositionSeeder), () => ComponentPositionSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(ComponentReferenceBasisSeeder), () => ComponentReferenceBasisSeeder.SeedAsync(context), logger);
        await RunSeederAsync(nameof(ComponentLifeLimitDimensionTypeSeeder), () => ComponentLifeLimitDimensionTypeSeeder.SeedAsync(context), logger);

        // NEW (Batch 11, 2026-08-29) — SQUADRONOPS ModuleRole rows
        // (SQUADRON_PLANNER/ATC/CREWCHIEF), needed for UserScope data-scope
        // filtering. See Data/Seeders/SquadronOpsModuleRoleSeeder.cs.
        await RunSeederAsync(nameof(SquadronOpsModuleRoleSeeder), () => SquadronOpsModuleRoleSeeder.SeedAsync(context), logger);

        logger.LogInformation("Reference data seeding pipeline completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fatal error occurred during database migration/seeding block.");
    }
}

// Legacy import seeder (isolated)
using (var importScope = app.Services.CreateScope())
{
    var services = importScope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var uow = services.GetRequiredService<IUnitOfWork>();
        var componentService = services.GetRequiredService<IComponentService>();
        var importLog = await LegacyEngineImportSeeder.SeedAsync(uow, componentService);

        foreach (var line in importLog)
            logger.LogInformation("[LegacyEngineImport] {Line}", line);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "LegacyEngineImportSeeder failed.");
    }
}

// Fr date formatting

var fr = new CultureInfo("fr-FR");
fr.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";   // or "dd-MMM-yyyy"
fr.DateTimeFormat.DateSeparator = "-";

var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(fr),
    SupportedCultures = new[] { fr },
    SupportedUICultures = new[] { fr }
};

app.UseRequestLocalization(locOptions);

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
