using FRAProject.Areas.Settings.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.Models
{
    /// <summary>
    /// A CallSign (radio/callsign) that can be scoped to Base or Squadron (optional).
    /// </summary>
    public class CallSign
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }

        // Optional scoping so a call sign can be specific to a Base or Squadron
        public int? BaseId { get; set; }
        public Base Base { get; set; }
        public int? SquadronId { get; set; }
        public Squadron Squadron { get; set; }

        // Active flag so admins can retire call signs without deleting
        public bool IsActive { get; set; } = true;

        // Audit
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public string? UpdatedBy { get; set; }
    }
}