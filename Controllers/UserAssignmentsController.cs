using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels;

namespace FRAProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserAssignmentsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserAssignmentService _assignmentService;

        public UserAssignmentsController(IUnitOfWork uow, UserManager<ApplicationUser> userManager, IUserAssignmentService assignmentService)
        {
            _uow = uow;
            _userManager = userManager;
            _assignmentService = assignmentService;
        }

        // GET: UserAssignments?userId=xxx
        public async Task<IActionResult> Index(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var assignments = await _uow.UserAssignments.GetActiveByUserIdAsync(userId);

            var vm = new List<UserAssignmentListItemVm>();
            foreach (var a in assignments)
            {
                string? grantedByLabel = null;
                if (!string.IsNullOrEmpty(a.GrantedByUserId))
                {
                    var grantedBy = await _userManager.FindByIdAsync(a.GrantedByUserId);
                    grantedByLabel = grantedBy?.FullLabel ?? grantedBy?.UserName;
                }

                vm.Add(new UserAssignmentListItemVm
                {
                    Id = a.Id,
                    RoleLabel = a.IsBaseAdmin
                        ? "Base Admin (accès transversal)"
                        : $"{a.ModuleRole?.ModuleCode} / {a.ModuleRole?.RoleName}",
                    BaseName = a.Base?.BaseName ?? "—",
                    AcMainGroupLabel = a.AcMainGroup?.DisplayLabel,
                    WingName = a.Wing?.Name,
                    GrantedAtUtc = a.GrantedAtUtc,
                    GrantedByLabel = grantedByLabel
                });
            }

            ViewBag.UserId = userId;
            ViewBag.UserLabel = user.FullLabel ?? user.UserName;
            return View(vm);
        }

        // GET: UserAssignments/Create?userId=xxx
        public async Task<IActionResult> Create(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var dto = new UserAssignmentFormDto { UserId = userId, UserLabel = user.FullLabel ?? user.UserName ?? userId };
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // POST: UserAssignments/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAssignmentFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var grantedByUserId = _userManager.GetUserId(User)!;

            // Explicitly the Services DTO — unambiguous now that the
            // ViewModels one has been renamed to UserAssignmentFormDto.
            var result = await _assignmentService.GrantAsync(new UserAssignmentGrantDto
            {
                UserId = dto.UserId,
                IsBaseAdmin = dto.IsBaseAdmin,
                ModuleRoleId = dto.IsBaseAdmin ? null : dto.ModuleRoleId,
                BaseId = dto.BaseId,
                AcMainGroupId = dto.AcMainGroupId,
                WingId = dto.WingId
            }, grantedByUserId);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateDropdowns(dto);
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index), new { userId = dto.UserId });
        }

        // POST: UserAssignments/Revoke/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(int id, string userId, string? reason)
        {
            var revokedByUserId = _userManager.GetUserId(User)!;
            var result = await _assignmentService.RevokeAsync(id, revokedByUserId, reason);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index), new { userId });
        }

        // GET: UserAssignments/GetAcMainGroupsByBase?baseId=1
        // AJAX endpoint — AcMainGroup.BaseId means the group list depends
        // on which Base is chosen, same cascading pattern as Aircraft's
        // Create form (loadVersions/GetVersions).
        [HttpGet]
        public async Task<IActionResult> GetAcMainGroupsByBase(int baseId)
        {
            var groups = await _uow.AcMainGroups.GetAllAsync();
            var result = groups
                .Where(g => g.BaseId == baseId && g.IsActive)
                .OrderBy(g => g.SortOrder)
                .Select(g => new { value = g.Id, text = g.DisplayLabel });
            return Json(result);
        }

        private async Task PopulateDropdowns(UserAssignmentFormDto dto)
        {
            var moduleRoles = await _uow.ModuleRoles.GetAllAsync();
            dto.ModuleRoleOptions = moduleRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.ModuleCode).ThenBy(r => r.SortOrder)
                .Select(r => new SelectListItem($"{r.ModuleCode} / {r.RoleName}", r.Id.ToString()));

            var bases = await _uow.Bases.GetAllAsync();
            dto.BaseOptions = bases
                .Where(b => b.IsActive)
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem(b.BaseName, b.Id.ToString()));

            // Wing has no CRUD screen yet (model exists, no controller/views),
            // but the raw table can still be listed via the generic
            // repository. ShowWingScope is only true for SquadronOps roles
            // (Pilot/Instructor/Scheduler) — irrelevant for the
            // Maintenance-focused roles built this session, so this field
            // stays optional and doesn't block anything.
            var wings = await _uow.Wings.GetAllAsync();
            dto.WingOptions = wings
                .Where(w => w.Active) // NOTE: Wing uses "Active", not "IsActive"
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem(w.Name, w.Id.ToString()));
        }
    }
}
