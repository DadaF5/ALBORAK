using System;

namespace FRAProject.Models
{
    // Normalized qualification / certification table.
    // Use AcTypeId to tie a pilot's qualification to an aircraft type, or QualificationType for other certs.
    public class UserQualification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = ""; // FK to AspNetUsers.Id (ApplicationUser.Id)
        public int? AcTypeId { get; set; }       // optional: which aircraft type this qualification applies to
        public string QualificationType { get; set; } = ""; // e.g. "BasicPilot", "NightRating", "TypeRating"
        public DateTime IssuedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public int? IssuedByUserId { get; set; } // optional admin who issued
        public int? DocumentId { get; set; }     // link to stored proof (UserDocument)
        public string? Notes { get; set; }
    }
}