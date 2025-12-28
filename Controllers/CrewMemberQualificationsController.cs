using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FRAProject.Controllers
{
    [Authorize]
    public class CrewMemberQualificationsController : Controller
    {
        private readonly FRAContext _context;

        public CrewMemberQualificationsController(FRAContext context)
        {
            _context = context;
        }

        // GET: CrewMemberQualifications/Index/5?crewMemberId=5
        public async Task<IActionResult> Index(int? crewMemberId)
        {
            if (crewMemberId == null)
            {
                return NotFound();
            }

            var crewMember = await _context.CrewMembers
                .Include(c => c.Squadron)
                .Include(c => c.Person)
                .Include(c => c.CrewMemberQualifications)
                    .ThenInclude(cmq => cmq.Qualification)
                .FirstOrDefaultAsync(c => c.Id == crewMemberId);

            if (crewMember == null)
            {
                return NotFound();
            }

            ViewData["CrewMemberId"] = crewMemberId;
            ViewData["CrewMemberName"] = $"{crewMember.Captain} ({crewMember.NickName})";
            ViewData["Squadron"] = crewMember.Squadron?.Name;

            return View(crewMember.CrewMemberQualifications.ToList());
        }

        // GET: CrewMemberQualifications/Create/5?crewMemberId=5
        public async Task<IActionResult> Create(int? crewMemberId)
        {
            if (crewMemberId == null)
            {
                return NotFound();
            }

            var crewMember = await _context.CrewMembers
                .Include(c => c.Person)
                .FirstOrDefaultAsync(c => c.Id == crewMemberId);

            if (crewMember == null)
            {
                return NotFound();
            }

            ViewData["CrewMemberId"] = crewMemberId;
            ViewData["CrewMemberName"] = $"{crewMember.Captain} ({crewMember.NickName})";

            // Get active qualifications
            var qualifications = await _context.Qualifications
                .Where(q => q.Active)
                .OrderBy(q => q.Name)
                .ToListAsync();

            // Exclude qualifications the crew member already has
            var existingQualificationIds = await _context.CrewMemberQualifications
                .Where(cmq => cmq.CrewMemberId == crewMemberId && cmq.Status == "Active")
                .Select(cmq => cmq.QualificationId)
                .ToListAsync();

            var availableQualifications = qualifications
                .Where(q => !existingQualificationIds.Contains(q.Id))
                .Select(q => new SelectListItem
                {
                    Value = q.Id.ToString(),
                    Text = $"{q.Name} ({q.QualificationType})"
                })
                .ToList();

            ViewData["QualificationId"] = new SelectList(availableQualifications, "Value", "Text");

            // Set default dates
            var model = new CrewMemberQualification
            {
                CrewMemberId = crewMemberId.Value,
                ValidFrom = DateTime.Today,
                Status = "Active"
            };

            return View(model);
        }

        // POST: CrewMemberQualifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CrewMemberId,QualificationId,ValidFrom,ValidUntil,IssuedBy,Remarks,Status")] CrewMemberQualification crewMemberQualification)
        {
            if (ModelState.IsValid)
            {
                // Check if qualification already exists for this crew member
                var existing = await _context.CrewMemberQualifications
                    .FirstOrDefaultAsync(cmq =>
                        cmq.CrewMemberId == crewMemberQualification.CrewMemberId &&
                        cmq.QualificationId == crewMemberQualification.QualificationId &&
                        cmq.Status == "Active");

                if (existing != null)
                {
                    ModelState.AddModelError("", "This crew member already has this active qualification.");
                }
                else
                {
                    _context.Add(crewMemberQualification);
                    await _context.SaveChangesAsync();

                    // Update crew member's primary qualification if needed
                    var crewMember = await _context.CrewMembers
                        .Include(c => c.CrewMemberQualifications)
                        .FirstOrDefaultAsync(c => c.Id == crewMemberQualification.CrewMemberId);

                    if (crewMember != null && crewMember.PrimaryQualificationId == null)
                    {
                        crewMember.PrimaryQualificationId = crewMemberQualification.QualificationId;
                        crewMember.UpdatedAtUtc = DateTime.UtcNow;
                        _context.Update(crewMember);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index), new { crewMemberId = crewMemberQualification.CrewMemberId });
                }
            }

            // Reload ViewData if validation fails
            var crewMemberInfo = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.Id == crewMemberQualification.CrewMemberId);

            ViewData["CrewMemberName"] = $"{crewMemberInfo?.Captain} ({crewMemberInfo?.NickName})";

            var qualifications = await _context.Qualifications
                .Where(q => q.Active)
                .Select(q => new SelectListItem
                {
                    Value = q.Id.ToString(),
                    Text = $"{q.Name} ({q.QualificationType})"
                })
                .ToListAsync();

            ViewData["QualificationId"] = new SelectList(qualifications, "Value", "Text", crewMemberQualification.QualificationId);

            return View(crewMemberQualification);
        }

        // GET: CrewMemberQualifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crewMemberQualification = await _context.CrewMemberQualifications
                .Include(cmq => cmq.CrewMember)
                .Include(cmq => cmq.Qualification)
                .FirstOrDefaultAsync(cmq => cmq.Id == id);

            if (crewMemberQualification == null)
            {
                return NotFound();
            }

            ViewData["CrewMemberName"] = $"{crewMemberQualification.CrewMember?.Captain} ({crewMemberQualification.CrewMember?.NickName})";
            ViewData["QualificationName"] = crewMemberQualification.Qualification?.Name;

            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Expired", Text = "Expired" },
                new SelectListItem { Value = "Suspended", Text = "Suspended" },
                new SelectListItem { Value = "Revoked", Text = "Revoked" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" }
            };

            ViewData["Status"] = new SelectList(statuses, "Value", "Text", crewMemberQualification.Status);

            return View(crewMemberQualification);
        }

        // POST: CrewMemberQualifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CrewMemberId,QualificationId,ValidFrom,ValidUntil,IssuedBy,Remarks,Status")] CrewMemberQualification crewMemberQualification)
        {
            if (id != crewMemberQualification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(crewMemberQualification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CrewMemberQualificationExists(crewMemberQualification.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { crewMemberId = crewMemberQualification.CrewMemberId });
            }

            // Reload ViewData if validation fails
            var cmq = await _context.CrewMemberQualifications
                .Include(c => c.CrewMember)
                .Include(c => c.Qualification)
                .FirstOrDefaultAsync(c => c.Id == id);

            ViewData["CrewMemberName"] = $"{cmq?.CrewMember?.Captain} ({cmq?.CrewMember?.NickName})";
            ViewData["QualificationName"] = cmq?.Qualification?.Name;

            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Expired", Text = "Expired" },
                new SelectListItem { Value = "Suspended", Text = "Suspended" },
                new SelectListItem { Value = "Revoked", Text = "Revoked" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" }
            };

            ViewData["Status"] = new SelectList(statuses, "Value", "Text", crewMemberQualification.Status);

            return View(crewMemberQualification);
        }

        // GET: CrewMemberQualifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crewMemberQualification = await _context.CrewMemberQualifications
                .Include(cmq => cmq.CrewMember)
                .Include(cmq => cmq.Qualification)
                .FirstOrDefaultAsync(cmq => cmq.Id == id);

            if (crewMemberQualification == null)
            {
                return NotFound();
            }

            return View(crewMemberQualification);
        }

        // POST: CrewMemberQualifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var crewMemberQualification = await _context.CrewMemberQualifications
                .Include(cmq => cmq.CrewMember)
                .FirstOrDefaultAsync(cmq => cmq.Id == id);

            if (crewMemberQualification == null)
            {
                return NotFound();
            }

            var crewMemberId = crewMemberQualification.CrewMemberId;

            // Check if this is the primary qualification
            var crewMember = await _context.CrewMembers.FindAsync(crewMemberId);
            if (crewMember?.PrimaryQualificationId == crewMemberQualification.QualificationId)
            {
                crewMember.PrimaryQualificationId = null;
                crewMember.UpdatedAtUtc = DateTime.UtcNow;
                _context.Update(crewMember);
            }

            _context.CrewMemberQualifications.Remove(crewMemberQualification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { crewMemberId });
        }

        // Helper method to set as primary qualification
        public async Task<IActionResult> SetAsPrimary(int id)
        {
            var crewMemberQualification = await _context.CrewMemberQualifications
                .Include(cmq => cmq.CrewMember)
                .FirstOrDefaultAsync(cmq => cmq.Id == id);

            if (crewMemberQualification == null)
            {
                return NotFound();
            }

            var crewMember = await _context.CrewMembers.FindAsync(crewMemberQualification.CrewMemberId);
            if (crewMember != null)
            {
                crewMember.PrimaryQualificationId = crewMemberQualification.QualificationId;
                crewMember.UpdatedAtUtc = DateTime.UtcNow;
                _context.Update(crewMember);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { crewMemberId = crewMemberQualification.CrewMemberId });
        }

        private bool CrewMemberQualificationExists(int id)
        {
            return _context.CrewMemberQualifications.Any(e => e.Id == id);
        }
    }
}