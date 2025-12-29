using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    [Authorize]
    public class MedicalCheckController : Controller
    {
        private readonly FRAContext _context;

        public MedicalCheckController(FRAContext context)
        {
            _context = context;
        }

        // GET: MedicalCheck
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var medicalChecks = _context.MedicalChecks
                .Include(mc => mc.CrewMember)
                .Include(mc => mc.MedicalBilans)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                medicalChecks = medicalChecks.Where(mc =>
                    mc.CrewMember != null && (
                    mc.CrewMember.Captain.Contains(searchString) ||
                    mc.CrewMember.NickName.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "EXPIRÉ":
                        medicalChecks = medicalChecks.Where(mc => mc.IsExpired);
                        break;
                    case "À RENOUVELER":
                        medicalChecks = medicalChecks.Where(mc => mc.D_ToGo <= 30 && mc.D_ToGo > 0);
                        break;
                    case "VALIDE":
                        medicalChecks = medicalChecks.Where(mc => mc.D_ToGo > 30);
                        break;
                }
            }

            // Order by next due date (ascending)
            medicalChecks = medicalChecks.OrderBy(mc => mc.NextDueDate ?? DateTime.MaxValue);

            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            return View(await medicalChecks.ToListAsync());
        }

        // GET: MedicalCheck/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalCheck = await _context.MedicalChecks
                .Include(mc => mc.CrewMember)
                .Include(mc => mc.MedicalBilans)
                .FirstOrDefaultAsync(mc => mc.MedCheckID == id);

            if (medicalCheck == null)
            {
                return NotFound();
            }

            return View(medicalCheck);
        }

        // GET: MedicalCheck/Create
        public async Task<IActionResult> Create(int? crewMemberId)
        {
            var viewModel = new MedicalCheckCreateViewModel
            {
                CheckDate = DateTime.Today,
                MedCheckTypes = new List<string> { "CEMPN", "CONTROL", "VISITE A L'UNITE" },
                CaptainTypes = new List<string> { "PILOT", "CONTROLLER", "DRIVER", "TECHNICIAN" },
                Specialities = new List<string> { "PH", "PC", "MN", "CCA" },
                Aptitudes = new List<string> { "APTE", "APTE PAR DEROGATION", "INAPTE" }
            };

            if (crewMemberId.HasValue)
            {
                var crewMember = await _context.CrewMembers
                    .Include(cm => cm.Person)
                        .ThenInclude(p => p.Rank)
                    .Include(cm => cm.Squadron)
                    .FirstOrDefaultAsync(cm => cm.Id == crewMemberId.Value);

                if (crewMember != null)
                {
                    viewModel.CrewMemberId = crewMember.Id;
                    viewModel.Captain = crewMember.Captain;
                    viewModel.NickName = crewMember.NickName;
                    viewModel.Role = crewMember.Role;
                    viewModel.CrewMemberType = crewMember.CrewMemberType;

                    if (crewMember.Person != null)
                    {
                        viewModel.Matricule = crewMember.Person.Matricule;
                        viewModel.FullName = crewMember.Person.FullName;
                        viewModel.Grade = crewMember.Person.Rank?.Name;
                        viewModel.DateOfBirth = crewMember.Person.DateOfBirth;
                        viewModel.Speciality = crewMember.Person.Speciality;
                    }

                    if (crewMember.Squadron != null)
                    {
                        viewModel.Unit = crewMember.Squadron.Name;
                        viewModel.Squadron = crewMember.Squadron.Name;
                    }

                    // Set default speciality from person if available
                    if (!string.IsNullOrEmpty(crewMember.Person?.Speciality))
                    {
                        viewModel.Speciality = crewMember.Person.Speciality;
                    }

                    // Set default captain type from crew member type
                    viewModel.CaptainType = crewMember.CrewMemberType?.ToUpper();

                    ViewData["CrewMember"] = crewMember;
                }
            }

            // Get crew members for dropdown
            ViewData["CrewMemberSelectList"] = new SelectList(
                await _context.CrewMembers
                    .Include(cm => cm.Person)
                    .Where(cm => cm.Active)
                    .Select(cm => new
                    {
                        cm.Id,
                        DisplayName = $"{cm.Person!.LastName} {cm.Person.FirstName} ({cm.Person.Matricule}) - {cm.Captain}"
                    })
                    .ToListAsync(),
                "Id",
                "DisplayName",
                crewMemberId);

            return View(viewModel);
        }

        // POST: MedicalCheck/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalCheckCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Create MedicalCheck entity from viewModel
                var medicalCheck = new MedicalCheck
                {
                    CrewMemberId = viewModel.CrewMemberId,
                    MedCheckType = viewModel.MedCheckType,
                    CaptainType = viewModel.CaptainType,
                    CheckDate = viewModel.CheckDate,
                    DaysValid = viewModel.DaysValid,
                    Obs = viewModel.Obs,
                    Decision = viewModel.Decision,
                    NextDueDate = viewModel.NextDueDate,
                    Speciality = viewModel.Speciality,
                    Constatations = viewModel.Constatations,
                    OBESITE = viewModel.OBESITE,
                    C_Optique = viewModel.C_Optique,
                    Aptitude = viewModel.Aptitude,
                    Next_VU_Date = viewModel.Next_VU_Date,
                    VU_Date = viewModel.VU_Date,
                    LateCheckReason = viewModel.LateCheckReason,
                    VuLateCheckReason = viewModel.VuLateCheckReason
                };

                // Calculate NextDueDate if not provided but DaysValid is
                if (!medicalCheck.NextDueDate.HasValue && medicalCheck.DaysValid.HasValue && medicalCheck.CheckDate.HasValue)
                {
                    medicalCheck.NextDueDate = medicalCheck.CheckDate.Value.AddDays(medicalCheck.DaysValid.Value);
                }

                // Apply CEMPN logic for Next_VU_Date
                if (medicalCheck.MedCheckType == "CEMPN" && medicalCheck.CheckDate.HasValue)
                {
                    if (medicalCheck.DaysValid.HasValue)
                    {
                        if (medicalCheck.DaysValid >= 300) // 10 months in days (~300 days)
                        {
                            medicalCheck.Next_VU_Date = medicalCheck.CheckDate.Value.AddMonths(6);
                        }
                        else if (medicalCheck.DaysValid < 210) // <7 months in days (~210 days)
                        {
                            medicalCheck.Next_VU_Date = medicalCheck.CheckDate.Value.AddMonths(3);
                        }
                        else if (medicalCheck.Aptitude == "APTE PAR DEROGATION")
                        {
                            medicalCheck.Next_VU_Date = medicalCheck.CheckDate.Value.AddMonths(3);
                        }
                    }
                }

                _context.Add(medicalCheck);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Visite médicale créée avec succès.";
                return RedirectToAction(nameof(Details), new { id = medicalCheck.MedCheckID });
            }

            // If we got this far, something failed; re-populate dropdowns
            viewModel.MedCheckTypes = new List<string> { "CEMPN", "CONTROL", "VISITE A L'UNITE" };
            viewModel.CaptainTypes = new List<string> { "PILOT", "CONTROLLER", "DRIVER", "TECHNICIAN" };
            viewModel.Specialities = new List<string> { "PH", "PC", "MN", "CCA" };
            viewModel.Aptitudes = new List<string> { "APTE", "APTE PAR DEROGATION", "INAPTE" };

            // Re-populate crew member select list
            ViewData["CrewMemberSelectList"] = new SelectList(
                await _context.CrewMembers
                    .Include(cm => cm.Person)
                    .Where(cm => cm.Active)
                    .Select(cm => new
                    {
                        cm.Id,
                        DisplayName = $"{cm.Person!.LastName} {cm.Person.FirstName} ({cm.Person.Matricule}) - {cm.Captain}"
                    })
                    .ToListAsync(),
                "Id",
                "DisplayName",
                viewModel.CrewMemberId);

            return View(viewModel);
        }
        // GET: MedicalCheck/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalCheck = await _context.MedicalChecks.FindAsync(id);
            if (medicalCheck == null)
            {
                return NotFound();
            }

            ViewData["CrewMemberId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.CrewMembers, "Id", "FullName", medicalCheck.CrewMemberId);
            ViewBag.Specialities = new List<string> { "PH", "PC", "MN", "CCA" };
            ViewBag.MedCheckTypes = new List<string> { "CEMPN", "CONTROL", "VISITE A L'UNITE" };
            ViewBag.CaptainTypes = new List<string> { "PILOT", "CONTROLLER", "DRIVER", "TECHNICIAN" };
            ViewBag.Aptitudes = new List<string> { "APTE", "APTE PAR DEROGATION", "INAPTE" };

            return View(medicalCheck);
        }

        // POST: MedicalCheck/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MedCheckID,CrewMemberId,MedCheckType,CheckDate,DaysValid,Obs,Decision,NextDueDate,Speciality,Constatations,OBESITE,C_Optique,Aptitude,Next_VU_Date,VU_Date,LateCheckReason,CaptainType,VuLateCheckReason")] MedicalCheck medicalCheck)
        {
            if (id != medicalCheck.MedCheckID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalculate dates based on changes
                    if (medicalCheck.CheckDate.HasValue && medicalCheck.DaysValid.HasValue)
                    {
                        medicalCheck.NextDueDate = medicalCheck.CheckDate.Value.AddDays(medicalCheck.DaysValid.Value);
                    }

                    // Reapply CEMPN logic
                    if (medicalCheck.MedCheckType == "CEMPN" && medicalCheck.CheckDate.HasValue)
                    {
                        if (medicalCheck.DaysValid.HasValue)
                        {
                            if (medicalCheck.DaysValid >= 300) // 10 months
                            {
                                medicalCheck.Next_VU_Date = medicalCheck.CheckDate.Value.AddMonths(6);
                            }
                            else if (medicalCheck.DaysValid < 210) // <7 months
                            {
                                medicalCheck.Next_VU_Date = medicalCheck.CheckDate.Value.AddMonths(3);
                            }
                            else if (medicalCheck.Aptitude == "APTE PAR DEROGATION")
                            {
                                medicalCheck.Next_VU_Date = medicalCheck.CheckDate.Value.AddMonths(3);
                            }
                        }
                    }

                    _context.Update(medicalCheck);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Visite médicale mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalCheckExists(medicalCheck.MedCheckID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = medicalCheck.MedCheckID });
            }

            ViewData["CrewMemberId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.CrewMembers, "Id", "FullName", medicalCheck.CrewMemberId);
            ViewBag.Specialities = new List<string> { "PH", "PC", "MN", "CCA" };
            ViewBag.MedCheckTypes = new List<string> { "CEMPN", "CONTROL", "VISITE A L'UNITE" };
            ViewBag.CaptainTypes = new List<string> { "PILOT", "CONTROLLER", "DRIVER", "TECHNICIAN" };
            ViewBag.Aptitudes = new List<string> { "APTE", "APTE PAR DEROGATION", "INAPTE" };

            return View(medicalCheck);
        }

        // GET: MedicalCheck/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalCheck = await _context.MedicalChecks
                .Include(mc => mc.CrewMember)
                .FirstOrDefaultAsync(mc => mc.MedCheckID == id);

            if (medicalCheck == null)
            {
                return NotFound();
            }

            return View(medicalCheck);
        }

        // POST: MedicalCheck/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalCheck = await _context.MedicalChecks.FindAsync(id);
            if (medicalCheck != null)
            {
                _context.MedicalChecks.Remove(medicalCheck);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Visite médicale supprimée avec succès.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: MedicalCheck/AddBilan/5
        public async Task<IActionResult> AddBilan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalCheck = await _context.MedicalChecks.FindAsync(id);
            if (medicalCheck == null)
            {
                return NotFound();
            }

            var bilan = new MedicalBilan
            {
                MedicalCheckId = medicalCheck.MedCheckID,
                RequiredDate = DateTime.Today.AddDays(7) // Default 1 week from today
            };

            ViewBag.BilanTypes = new List<string> { "Blood Test", "X-Ray", "ECG", "Eye Test", "Hearing Test", "Other" };
            return View(bilan);
        }

        // POST: MedicalCheck/AddBilan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBilan([Bind("MedicalCheckId,BilanType,Details,DurationMonths,DurationDays,RequiredDate,IsCompleted,CompletedDate,Result,Remarks")] MedicalBilan medicalBilan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicalBilan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bilan médical ajouté avec succès.";
                return RedirectToAction(nameof(Details), new { id = medicalBilan.MedicalCheckId });
            }

            ViewBag.BilanTypes = new List<string> { "Blood Test", "X-Ray", "ECG", "Eye Test", "Hearing Test", "Other" };
            return View(medicalBilan);
        }

        // GET: MedicalCheck/EditBilan/5
        public async Task<IActionResult> EditBilan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalBilan = await _context.MedicalBilans.FindAsync(id);
            if (medicalBilan == null)
            {
                return NotFound();
            }

            ViewBag.BilanTypes = new List<string> { "Blood Test", "X-Ray", "ECG", "Eye Test", "Hearing Test", "Other" };
            return View(medicalBilan);
        }

        // POST: MedicalCheck/EditBilan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBilan(int id, [Bind("BilanID,MedicalCheckId,BilanType,Details,DurationMonths,DurationDays,RequiredDate,IsCompleted,CompletedDate,Result,Remarks")] MedicalBilan medicalBilan)
        {
            if (id != medicalBilan.BilanID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicalBilan);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Bilan médical mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalBilanExists(medicalBilan.BilanID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = medicalBilan.MedicalCheckId });
            }

            ViewBag.BilanTypes = new List<string> { "Blood Test", "X-Ray", "ECG", "Eye Test", "Hearing Test", "Other" };
            return View(medicalBilan);
        }

        // POST: MedicalCheck/CompleteBilan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteBilan(int id)
        {
            var medicalBilan = await _context.MedicalBilans.FindAsync(id);
            if (medicalBilan == null)
            {
                return NotFound();
            }

            medicalBilan.IsCompleted = true;
            medicalBilan.CompletedDate = DateTime.Today;

            _context.Update(medicalBilan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bilan marqué comme complété.";
            return RedirectToAction(nameof(Details), new { id = medicalBilan.MedicalCheckId });
        }

        // GET: MedicalCheck/DeleteBilan/5
        public async Task<IActionResult> DeleteBilan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalBilan = await _context.MedicalBilans
                .Include(mb => mb.MedicalCheck)
                .FirstOrDefaultAsync(mb => mb.BilanID == id);

            if (medicalBilan == null)
            {
                return NotFound();
            }

            return View(medicalBilan);
        }

        // POST: MedicalCheck/DeleteBilan/5
        [HttpPost, ActionName("DeleteBilan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBilanConfirmed(int id)
        {
            var medicalBilan = await _context.MedicalBilans.FindAsync(id);
            var medicalCheckId = medicalBilan?.MedicalCheckId;

            if (medicalBilan != null)
            {
                _context.MedicalBilans.Remove(medicalBilan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bilan supprimé avec succès.";
            }

            return RedirectToAction(nameof(Details), new { id = medicalCheckId });
        }

        // GET: MedicalCheck/History/5 (CrewMember's medical history)
        public async Task<IActionResult> History(int? crewMemberId)
        {
            if (crewMemberId == null)
            {
                return NotFound();
            }

            var crewMember = await _context.CrewMembers
                .Include(cm => cm.MedicalChecks)
                    .ThenInclude(mc => mc.MedicalBilans)
                .FirstOrDefaultAsync(cm => cm.Id == crewMemberId);

            if (crewMember == null)
            {
                return NotFound();
            }

            // Filter to show last 2 years of medical checks
            var twoYearsAgo = DateTime.Today.AddYears(-2);
            crewMember.MedicalChecks = crewMember.MedicalChecks
                .Where(mc => mc.CheckDate >= twoYearsAgo)
                .OrderByDescending(mc => mc.CheckDate)
                .ToList();

            return View(crewMember);
        }

        // GET: MedicalCheck/ExpiringSoon
        public async Task<IActionResult> ExpiringSoon()
        {
            var thirtyDaysFromNow = DateTime.Today.AddDays(30);

            var expiringChecks = await _context.MedicalChecks
                .Include(mc => mc.CrewMember)
                .Include(mc => mc.MedicalBilans)
                .Where(mc => mc.NextDueDate.HasValue &&
                           mc.NextDueDate <= thirtyDaysFromNow &&
                           mc.NextDueDate >= DateTime.Today)
                .OrderBy(mc => mc.NextDueDate)
                .ToListAsync();

            return View(expiringChecks);
        }

        // GET: MedicalCheck/Overdue
        public async Task<IActionResult> Overdue()
        {
            var overdueChecks = await _context.MedicalChecks
                .Include(mc => mc.CrewMember)
                .Include(mc => mc.MedicalBilans)
                .Where(mc => mc.IsExpired)
                .OrderBy(mc => mc.NextDueDate)
                .ToListAsync();

            return View(overdueChecks);
        }

        // GET: MedicalCheck/OverdueBilans
        public async Task<IActionResult> OverdueBilans()
        {
            var overdueBilans = await _context.MedicalBilans
                .Include(mb => mb.MedicalCheck)
                    .ThenInclude(mc => mc.CrewMember)
                .Where(mb => mb.IsOverdue)
                .OrderBy(mb => mb.RequiredDate)
                .ToListAsync();

            return View(overdueBilans);
        }

        private bool MedicalCheckExists(int id)
        {
            return _context.MedicalChecks.Any(e => e.MedCheckID == id);
        }

        private bool MedicalBilanExists(int id)
        {
            return _context.MedicalBilans.Any(e => e.BilanID == id);
        }

        // GET: MedicalCheck/GetCrewMemberInfo/5
        public async Task<JsonResult> GetCrewMemberInfo(int crewMemberId)
        {
            var crewMember = await _context.CrewMembers
                .Include(cm => cm.Person)
                    .ThenInclude(p => p.Rank)
                .Include(cm => cm.Squadron)
                .FirstOrDefaultAsync(cm => cm.Id == crewMemberId);
            if (crewMember == null)
            {
                return Json(new { success = false, message = "Crew member not found." });
            }
            var data = new
            {
                success = true,
                crewMemberId = crewMember.Id,
                fullName = crewMember.Person?.FullName,
                matricule = crewMember.Person?.Matricule,
                grade = crewMember.Person?.Rank?.Name,
                dateOfBirth = crewMember.Person?.DateOfBirth?.ToString("dd/MM/yyyy"),
                captain = crewMember.Captain,
                nickName = crewMember.NickName,
                role = crewMember.Role,
                unit = crewMember.Squadron?.Name,
                crewMemberType = crewMember.CrewMemberType,
                speciality = crewMember.Person?.Speciality
            };
            return Json(data);
        }
    }
}
