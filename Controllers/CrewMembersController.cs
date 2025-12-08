using FRAProject.Data;
using FRAProject.Enums;
using FRAProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    public class CrewMembersController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<CrewMembersController> _logger;
        private readonly IWebHostEnvironment _env;

        // Max upload size in bytes (2 MB)
        private const long MaxUploadBytes = 2 * 1024 * 1024;

        // Allowed file extensions
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public CrewMembersController(FRAContext context, ILogger<CrewMembersController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // GET: CrewMembers
        // searchString searches NickName, Role, Mobile, Status, PrimaryQualification.Name and Person FullName
        // sortOrder supports: "name_asc/desc", "squadron_asc/desc", "type_asc/desc", "status_asc/desc"
        public async Task<IActionResult> Index(string sortOrder, string? searchString, int pageNumber = 1, int pageSize = 25)
        {
            ViewData["CurrentSort"] = sortOrder ?? "name_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageSize"] = pageSize;

            IQueryable<CrewMember> query = _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Include(cm => cm.PrimaryQualification)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(cm =>
                    EF.Functions.Like(cm.NickName ?? string.Empty, $"%{s}%") ||
                    EF.Functions.Like(cm.Role ?? string.Empty, $"%{s}%") ||
                    EF.Functions.Like(cm.Mobile ?? string.Empty, $"%{s}%") ||
                    EF.Functions.Like(cm.Status.ToString(), $"%{s}%") ||
                    (cm.PrimaryQualification != null && EF.Functions.Like(cm.PrimaryQualification.Name ?? string.Empty, $"%{s}%")) ||
                    (cm.Person != null && EF.Functions.Like(
                        ((cm.Person.FirstName ?? "") + " " + (cm.Person.LastName ?? "")), $"%{s}%"))
                );
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(cm => cm.NickName).ThenBy(cm => cm.Id),
                "squadron_asc" => query.OrderBy(cm => cm.SquadronId).ThenBy(cm => cm.NickName),
                "squadron_desc" => query.OrderByDescending(cm => cm.SquadronId).ThenByDescending(cm => cm.NickName),
                "type_asc" => query.OrderBy(cm => cm.CrewMemberType).ThenBy(cm => cm.NickName),
                "type_desc" => query.OrderByDescending(cm => cm.CrewMemberType).ThenByDescending(cm => cm.NickName),
                "status_asc" => query.OrderBy(cm => cm.Status).ThenBy(cm => cm.NickName),
                "status_desc" => query.OrderByDescending(cm => cm.Status).ThenByDescending(cm => cm.NickName),
                "name_asc" or _ => query.OrderBy(cm => cm.NickName).ThenBy(cm => cm.Id)
            };

            pageNumber = Math.Max(1, pageNumber);
            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["TotalItems"] = totalItems;
            ViewData["PageNumber"] = pageNumber;

            return View(items);
        }

        // GET: CrewMembers/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectListsAsync();
            return View();
        }

        // POST: CrewMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("SequenceNo,Captain,NickName,Role,Active,Mobile,Status,AllowedToSign,CrewMemberType,SquadronId,PersonId,PrimaryQualificationId")] CrewMember crewMember,
            IFormFile PhotoFile,
            string Photo)
        {
            // Prevent early model validation failure for Photo (we handle it below)
            ModelState.Remove(nameof(crewMember.Photo));

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return View(crewMember);
            }

            // Require either an uploaded file or a URL for Create
            if ((PhotoFile == null || PhotoFile.Length == 0) && string.IsNullOrWhiteSpace(Photo))
            {
                ModelState.AddModelError("Photo", "Please provide a photo (either upload an image or enter an image URL).");
                await PopulateSelectListsAsync();
                return View(crewMember);
            }

            // Prevent duplicate: ensure PersonId is not already assigned to another crew member (1:1)
            if (crewMember.PersonId != 0)
            {
                var exists = await _context.CrewMembers.AnyAsync(cm => cm.PersonId == crewMember.PersonId);
                if (exists)
                {
                    ModelState.AddModelError(nameof(CrewMember.PersonId), "This person is already assigned to a crew member.");
                    await PopulateSelectListsAsync();
                    return View(crewMember);
                }
            }

            // Handle file upload or URL value (Photo parameter)
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                if (!TryValidateUpload(PhotoFile, out var validationError))
                {
                    ModelState.AddModelError("PhotoFile", validationError);
                    await PopulateSelectListsAsync();
                    return View(crewMember);
                }

                try
                {
                    crewMember.Photo = await SaveUploadAsync(PhotoFile, "crewmembers");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save uploaded photo");
                    ModelState.AddModelError("PhotoFile", "Failed to save uploaded file.");
                    await PopulateSelectListsAsync();
                    return View(crewMember);
                }
            }
            else // use Photo URL (we already validated presence above)
            {
                crewMember.Photo = Photo.Trim();
            }

            _context.Add(crewMember);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: CrewMembers/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var crewMember = await _context.CrewMembers.FindAsync(id);
            if (crewMember == null) return NotFound();

            await PopulateSelectListsAsync(crewMember);
            return View(crewMember);
        }

        // POST: CrewMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,SequenceNo,Captain,NickName,Role,Active,Mobile,Status,AllowedToSign,CrewMemberType,SquadronId,PersonId,PrimaryQualificationId")] CrewMember model,
            IFormFile PhotoFile,
            string Photo)
        {
            if (id != model.Id) return BadRequest();

            // Prevent early model validation failure for Photo (we handle uploads/URLs below)
            ModelState.Remove(nameof(model.Photo));

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            // Prevent duplicate PersonId (exclude current record)
            if (model.PersonId != 0)
            {
                var exists = await _context.CrewMembers.AnyAsync(cm => cm.Id != model.Id && cm.PersonId == model.PersonId);
                if (exists)
                {
                    ModelState.AddModelError(nameof(CrewMember.PersonId), "This person is already assigned to another crew member.");
                    await PopulateSelectListsAsync(model);
                    return View(model);
                }
            }

            var existing = await _context.CrewMembers.FindAsync(id);
            if (existing == null) return NotFound();

            // If a new file was uploaded, validate and save it
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                if (!TryValidateUpload(PhotoFile, out var validationError))
                {
                    ModelState.AddModelError("PhotoFile", validationError);
                    await PopulateSelectListsAsync(model);
                    return View(model);
                }

                try
                {
                    var newPath = await SaveUploadAsync(PhotoFile, "crewmembers");

                    // Delete old uploaded file if it was stored under /uploads/
                    if (!string.IsNullOrWhiteSpace(existing.Photo) && existing.Photo.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                    {
                        TryDeletePhysicalFile(existing.Photo);
                    }

                    existing.Photo = newPath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save uploaded photo");
                    ModelState.AddModelError("PhotoFile", "Failed to save uploaded file.");
                    await PopulateSelectListsAsync(model);
                    return View(model);
                }
            }
            else if (!string.IsNullOrWhiteSpace(Photo))
            {
                // If user provided a URL, use it
                existing.Photo = Photo.Trim();
            }
            // else: keep existing.Photo (no change)

            // Map other updatable fields
            existing.SequenceNo = model.SequenceNo;
            existing.Captain = model.Captain;
            existing.NickName = model.NickName;
            existing.Role = model.Role;
            existing.Active = model.Active;
            existing.Mobile = model.Mobile;
            existing.Status = model.Status;
            existing.AllowedToSign = model.AllowedToSign;
            existing.CrewMemberType = model.CrewMemberType;
            existing.SquadronId = model.SquadronId;
            existing.PersonId = model.PersonId;
            existing.PrimaryQualificationId = model.PrimaryQualificationId;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            try
            {
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CrewMemberExists(model.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: CrewMembers/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var crewMember = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Include(cm => cm.PrimaryQualification)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (crewMember == null) return NotFound();

            return View(crewMember);
        }

        // POST: CrewMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var crewMember = await _context.CrewMembers.FindAsync(id);
            if (crewMember != null)
            {
                // delete uploaded photo file if applicable
                if (!string.IsNullOrWhiteSpace(crewMember.Photo) && crewMember.Photo.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    TryDeletePhysicalFile(crewMember.Photo);
                }

                _context.CrewMembers.Remove(crewMember);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Validate upload before saving. Returns true if valid; otherwise false and sets error message.
        private bool TryValidateUpload(IFormFile file, out string? error)
        {
            error = null;

            if (file == null)
            {
                error = "No file was uploaded.";
                return false;
            }

            if (file.Length == 0)
            {
                error = "The uploaded file is empty.";
                return false;
            }

            if (file.Length > MaxUploadBytes)
            {
                error = $"File is too large. Maximum allowed size is {MaxUploadBytes / (1024 * 1024)} MB.";
                return false;
            }

            var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();
            if (!contentType.StartsWith("image/"))
            {
                error = "Only image file types are allowed.";
                return false;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            {
                error = $"Unsupported image format. Allowed formats: {string.Join(", ", AllowedExtensions)}.";
                return false;
            }

            // Check file signature (magic bytes)
            if (!HasValidImageSignature(file, ext))
            {
                error = "The uploaded file does not match the expected image format.";
                return false;
            }

            return true;
        }

        // Helper: check file signature (magic bytes) for common image formats
        private bool HasValidImageSignature(IFormFile file, string ext)
        {
            try
            {
                // We'll read up to 12 bytes (enough for PNG, JPEG, GIF, WEBP)
                var header = new byte[12];
                using (var stream = file.OpenReadStream())
                {
                    // Read synchronously from the opened stream
                    int bytesRead = stream.Read(header, 0, header.Length);
                }

                // JPEG: FF D8 FF
                if ((ext == ".jpg" || ext == ".jpeg") && header.Length >= 3)
                {
                    if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                        return true;
                    return false;
                }

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (ext == ".png" && header.Length >= 8)
                {
                    var pngSig = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                    if (header.Take(8).SequenceEqual(pngSig)) return true;
                    return false;
                }

                // GIF: "GIF87a" or "GIF89a"
                if (ext == ".gif" && header.Length >= 6)
                {
                    var gif87 = new byte[] { (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'7', (byte)'a' };
                    var gif89 = new byte[] { (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'9', (byte)'a' };
                    if (header.Take(6).SequenceEqual(gif87) || header.Take(6).SequenceEqual(gif89)) return true;
                    return false;
                }

                // WEBP: "RIFF" .... "WEBP" -> bytes 0-3 == RIFF and 8-11 == WEBP
                if (ext == ".webp" && header.Length >= 12)
                {
                    var riff = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
                    var webp = new byte[] { (byte)'W', (byte)'E', (byte)'B', (byte)'P' };
                    if (header.Take(4).SequenceEqual(riff) && header.Skip(8).Take(4).SequenceEqual(webp)) return true;
                    return false;
                }

                // If extension known but signature not matched above, reject.
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate file signature");
                return false;
            }
        }

        // Helper: save uploaded file, return public relative URL (e.g. /uploads/crewmembers/abcd.jpg)
        private async Task<string> SaveUploadAsync(IFormFile file, string folder)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            // double-check size/extension (defensive)
            if (!TryValidateUpload(file, out var err))
            {
                throw new InvalidOperationException(err);
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", folder);
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            // ensure no path traversal
            var normalized = Path.GetFullPath(filePath);
            if (!normalized.StartsWith(Path.GetFullPath(uploadsRoot)))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            using (var stream = new FileStream(normalized, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // return application-relative url
            return $"/uploads/{folder}/{fileName}";
        }

        // Helper: try delete physical file referenced by a stored relative url (safety: only delete under /uploads/)
        private void TryDeletePhysicalFile(string relativeUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativeUrl)) return;
                // Expected format: /uploads/<folder>/<file>
                if (!relativeUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return;

                var rel = relativeUrl.TrimStart('/');
                var physical = Path.Combine(_env.WebRootPath ?? "wwwroot", rel.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(physical))
                {
                    System.IO.File.Delete(physical);
                }
            }
            catch (Exception ex)
            {
                // log but don't throw - file deletion should not break the user flow
                _logger.LogWarning(ex, "Failed to delete old upload file {Url}", relativeUrl);
            }
        }

        // Helper: populate dropdown/select lists using related models and enums
        private async Task PopulateSelectListsAsync(CrewMember? current = null)
        {
            var persons = await _context.Persons
                .AsNoTracking()
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Select(p => new { p.Id, FullName = (p.FirstName ?? "") + " " + (p.LastName ?? "") })
                .ToListAsync();

            var squadrons = await _context.Squadrons
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var qualifications = await _context.Qualifications
                .AsNoTracking()
                .OrderBy(q => q.Name)
                .Select(q => new { q.Id, q.Name })
                .ToListAsync();

            ViewData["Persons"] = new SelectList(persons, "Id", "FullName", current?.PersonId);
            ViewData["Squadrons"] = new SelectList(squadrons, "Id", "Name", current?.SquadronId);
            ViewData["Qualifications"] = new SelectList(qualifications, "Id", "Name", current?.PrimaryQualificationId);

            ViewData["CrewMemberTypes"] = Enum.GetValues(typeof(CrewMemberType))
                .Cast<CrewMemberType>()
                .Select(e => new SelectListItem { Text = e.ToString(), Value = e.ToString(), Selected = current != null && current.CrewMemberType.ToString() == e.ToString() })
                .ToList();

            ViewData["CrewMemberStatuses"] = Enum.GetValues(typeof(CrewMemberStatus))
                .Cast<CrewMemberStatus>()
                .Select(e => new SelectListItem { Text = e.ToString(), Value = e.ToString(), Selected = current != null && current.Status.ToString() == e.ToString() })
                .ToList();
        }

        private bool CrewMemberExists(int id)
        {
            return _context.CrewMembers.Any(e => e.Id == id);
        }
    }
}