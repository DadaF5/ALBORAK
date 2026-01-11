using FRAProject.Data;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Authorize]
    [Area("AircraftMaintenance")]
    public class AircraftDashboardController : Controller

    {
        private readonly FRAContext _context;
        public AircraftDashboardController(FRAContext context) => _context = context;

        // GET: Aircraft/Dashboard
        public async Task<IActionResult> AircraftDashboard()
        {
            // Load all aircrafts including hierarchy
            var aircrafts = await _context.Aircrafts
                .Include(a => a.AcType)
                    .ThenInclude(t => t.AcMainGroup)
                        .ThenInclude(mg => mg.Base)
                .Include(a => a.AcType.AcMainGroup.AcCategory)
                .Include(a => a.AcStatusType)
                .ToListAsync();

            // Group by Base → Category → MainGroup → Type → Status
            var dashboard = aircrafts
                .GroupBy(a => a.AcType.AcMainGroup.Base) // Base
                .Select(baseGroup => new AircraftStatusDashboardViewModel
                {
                    BaseId = baseGroup.Key.Id,
                    BaseName = baseGroup.Key.BaseName,
                    Categories = baseGroup
                        .GroupBy(a => a.AcType.AcMainGroup.AcCategory) // AcCategory
                        .Select(catGroup =>
                        {
                            var catEntity = catGroup.Key;
                            return new AcCategoryStatus
                            {
                                AcCategoryId = catEntity.Id,
                                CategoryName = catEntity.Name,
                                MainGroups = catGroup
                                    .GroupBy(a => a.AcType.AcMainGroup) // MainGroup directly
                                    .Select(mgGroup =>
                                    {
                                        var mgEntity = mgGroup.Key;
                                        return new AcMainGroupStatus
                                        {
                                            AcMainGroupId = mgEntity.Id,
                                            MainGroupName = mgEntity.Name,
                                            Types = mgGroup
                                                .GroupBy(a => a.AcType) // Type
                                                .Select(typeGroup =>
                                                {
                                                    var typeEntity = typeGroup.Key;
                                                    return new AcTypeStatus
                                                    {
                                                        AcTypeId = typeEntity.Id,
                                                        TypeName = typeEntity.Name,
                                                        StatusCounts = typeGroup
                                                            .GroupBy(a => a.AcStatusType.StatusName) // Status
                                                            .ToDictionary(g => g.Key, g => g.Count())
                                                    };
                                                }).ToList()
                                        };
                                    }).ToList()
                            };
                        }).ToList()
                }).ToList();

            return View(dashboard);
        }





        // GET: AircraftDashboard
        //    public async Task<IActionResult> AircraftDashboard()
        //    {
        //        // Get all aircrafts including hierarchy
        //        var aircrafts = await _context.Aircrafts
        //.Include(a => a.AcType)
        //    .ThenInclude(t => t.AcMainGroup)        
        //        .ThenInclude(mg => mg.Base)              
        //            .ThenInclude(c => c.BaseName)  // Base included properly
        //                .Include(a => a.AcStatusType)   // Include status type for counting later
        //                .ToListAsync();

        //        // Group by Base → Category → MainGroup → Type → Status
        //        var dashboard = aircrafts
        //            .GroupBy(a => a.AcType.AcMainGroup.Base)
        //            .Select(baseGroup => new AircraftStatusDashboardViewModel
        //            {
        //                BaseId = baseGroup.Key.Id,
        //                BaseName = baseGroup.Key.BaseName,
        //                Categories = baseGroup
        //                    .GroupBy(a => a.AcType.AcMainGroup.AcCategory)
        //                    .Select(catGroup => new AcCategoryStatus
        //                    {
        //                        AcCategoryId = catGroup.Key.Id,
        //                        CategoryName = catGroup.Key.Name,
        //                        MainGroups = catGroup
        //                            .GroupBy(a => a.AcType.AcMainGroup)
        //                            .Select(mgGroup => new AcMainGroupStatus
        //                            {
        //                                AcMainGroupId = mgGroup.Key.Id,
        //                                MainGroupName = mgGroup.Key.Name,
        //                                Types = mgGroup
        //                                    .GroupBy(a => a.AcType)
        //                                    .Select(typeGroup => new AcTypeStatus
        //                                    {
        //                                        AcTypeId = typeGroup.Key.Id,
        //                                        TypeName = typeGroup.Key.Name,
        //                                        StatusCounts = typeGroup
        //                                            .GroupBy(a => a.AcStatusType.StatusName)
        //                                            .ToDictionary(g => g.Key, g => g.Count())
        //                                    }).ToList()
        //                            }).ToList()
        //                    }).ToList()
        //            }).ToList();

        //        return View(dashboard);
        //    }

    }
}
