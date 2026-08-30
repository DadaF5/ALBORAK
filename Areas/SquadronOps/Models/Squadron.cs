using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.SquadronOps.Models
{
    public class Squadron
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        [Display(Name = "Call-Sign (BORAK)")]
        public string? CallSign { get; set; }

        [StringLength(100)]
        [Display(Name = "Logo Path")]
        public string? LogoPath { get; set; }

        [NotMapped]
        [Display(Name = "Squadron Logo")]
        public IFormFile? LogoFile { get; set; }

        [StringLength(40)]
        [Display(Name = "Nom de l'Escadron")]
        public string? FrenchName { get; set; }

        [StringLength(10)]
        [Display(Name = "Short Call-Sign (BRK)")]
        public string? CallSignShort { get; set; }

        // FK to Wing
        [Required]
        public int WingId { get; set; }
        public Wing Wing { get; set; }

        public bool Active { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // NEW (2026-08-29) — squadron's CURRENT operating base. Additive,
        // nullable. This is deliberately separate from:
        //   - Department.BaseId (via Wing.DepartmentId) — the squadron's
        //     ADMINISTRATIVE/authorization-scope base, used by
        //     IUserScopeService/OdvPlanningController.IsSquadronInScopeAsync
        //     to decide WHO can see this squadron's data. Unrelated to
        //     where the squadron is actually flying from. Do not conflate.
        //   - Wing.BaseId — real field, nullable, not read by any real
        //     scoping/business code seen so far; purpose unconfirmed.
        //
        // CurrentBaseId answers a different, operational question: e.g.
        // F16's home base is 6th AFB, but Squadron 312 currently operates
        // from 2nd AFB while Squadron 512 operates from its home base.
        // A new Odv's BaseId should default from here (see
        // OdvPlanningController.Create) and can still be overridden per
        // ODV via Odv.BaseId for a one-off detachment.
        // ════════════════════════════════════════════════════════════════
        [Display(Name = "Current Operating Base")]
        public int? CurrentBaseId { get; set; }
        public Base? CurrentBase { get; set; }

        // Computed for display
        [NotMapped]
        public string FullName => $"{Name} ({Wing?.Name})";

        // Navigation: crew members belonging to this squadron
        // Initialize collection to avoid null checks in code.
        public ICollection<CrewMember> CrewMembers { get; set; } = new List<CrewMember>();
        public ICollection<Odv> Odvs { get; set; } = new List<Odv>();
        // ✅ NEW
        public ICollection<Mission> Missions { get; set; } = new List<Mission>();


    }
}
