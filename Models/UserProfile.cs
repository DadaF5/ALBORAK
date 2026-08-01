using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Domain profile for a platform user.
    ///
    /// Why a separate table from ApplicationUser?
    ///   ApplicationUser is owned by ASP.NET Identity.
    ///   UserProfile is owned by our domain.
    ///   Keeping them separate makes migrations cleaner and
    ///   ensures Identity upgrades don't touch domain data.
    ///
    /// Relationship:
    ///   One ApplicationUser → One UserProfile (PK = UserId)
    ///   UserId is both PK and FK → no separate Id column.
    ///
    /// Table: "UserProfiles"
    /// </summary>
    [Table("UserProfiles")]
    public class UserProfile
    {
        // ── PK = FK → AspNetUsers ─────────────────────────────────────────
        /// <summary>
        /// Same as ApplicationUser.Id (nvarchar 450).
        /// Configured as both PK and FK in Fluent API.
        /// </summary>
        [Key]
        public string UserId { get; set; } = string.Empty;

        // ── Extended profile fields ───────────────────────────────────────
        /// <summary>
        /// Full official name — may differ from FirstName + LastName
        /// e.g. "Mohammed Ibn Youssef ALAMI"
        /// </summary>
        [StringLength(200)]
        public string? FullOfficialName { get; set; }

        /// <summary>
        /// Military specialty code — e.g. "MECA", "AVION", "ELEC"
        /// Matches Specialty codes used on JobCard.
        /// Used to filter which job cards are shown to a technician.
        /// </summary>
        [StringLength(20)]
        public string? Specialty { get; set; }

        /// <summary>
        /// LMAM licence number — maintenance approval.
        /// Required for APRS and Navigability Officer roles.
        /// </summary>
        [StringLength(50)]
        public string? LMAMNumber { get; set; }

        /// <summary>
        /// LMAM licence expiry — checked before allowing sign-off.
        /// </summary>
        public DateOnly? LMAMExpiry { get; set; }

        /// <summary>
        /// Office / section within the base.
        /// </summary>
        [StringLength(100)]
        public string? Section { get; set; }

        /// <summary>
        /// Internal phone / radio number.
        /// </summary>
        [StringLength(30)]
        public string? InternalPhone { get; set; }

        // ── Audit ─────────────────────────────────────────────────────────
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public ApplicationUser? User { get; set; }
    }
}
