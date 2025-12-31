using FRAProject.Data;
using FRAProject.Infrastructure.Authorization;
using FRAProject.Infrastructure.Identity;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.Services.Medical;
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

// custom claims factory
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
builder.Services.AddScoped<IMenuService,SampleMenuService>();
builder.Services.AddScoped<IMedicalFitnessService, MedicalFitnessService>();


// Authorization handlers & policies
builder.Services.AddSingleton<IAuthorizationHandler, SameSquadronHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SameBaseHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SquadronOrBaseMaintenanceHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameSquadron", p => p.Requirements.Add(new SameSquadronRequirement()));
    options.AddPolicy("SameBase", p => p.Requirements.Add(new SameBaseRequirement()));
    options.AddPolicy("SquadronOrBaseMaintenance", p => p.Requirements.Add(new SquadronOrBaseMaintenanceRequirement()));
    options.AddPolicy("RequireCrewChiefOrAdmin", p => p.RequireRole("CrewChief", "Admin"));

    // IMPORTANT: require authentication for every endpoint by default.
    // Controllers/actions or pages that should be public must use [AllowAnonymous].
    //options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
});

// If you scaffold or use Identity UI, register Razor Pages
builder.Services.AddRazorPages();

// register application services
builder.Services.AddScoped<SquadronActivityService>();
// register other domain services here
//builder.Services.AddScoped<IMenuService, SampleMenuService>();
// Register DB-backed service (scoped so it gets FRAContext per-request)
builder.Services.AddScoped<IMenuService, MenuService>();

// MVC + JSON options
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// Seed Roles/Admin (run once at startup)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeed.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seeding error: " + ex.Message);
    }

}
    // Seed Phases/Missions (run once at startup)
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<FRAContext>();
        // Ensure DB schema is up to date
        await context.Database.MigrateAsync();

        // Seed reference data
        await PhaseSeeder.SeedAsync(context);
        await MissionSeeder.SeedAsync(context);
    }
        catch (Exception ex)
        {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(
                    ex,
                    "Error occurred during application startup while seeding Phase/Mission data."
                );
    }
    }

    // after var app = builder.Build(); and after any role/user seeding

    using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // seed menu items if empty
        await MenuSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        // replace with your logger if you have one
        Console.WriteLine("Menu seeding error: " + ex.Message);
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages so Identity UI (if used) is reachable at /Identity/Account/Login etc.
app.MapRazorPages();

app.Run();