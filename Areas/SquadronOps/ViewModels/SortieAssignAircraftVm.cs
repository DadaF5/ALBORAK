using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    // NEW (2026-08-29, Batch 9) — dedicated "Assign Aircraft" step, per
    // Dadda's confirmed decision: its own action, not folded into
    // Create/Edit, matching SortieStatus.AircraftAssigned as a real
    // workflow stage rather than a field on the planning form.
    public class SortieAssignAircraftVm
    {
        public int SortieId { get; set; }

        [Required(ErrorMessage = "Select an aircraft.")]
        [Display(Name = "Aircraft")]
        public int AircraftId { get; set; }

        // Display-only context for the view — not posted back, just
        // repopulated server-side on GET and on a failed POST.
        public string? SortieCode { get; set; }
        public string? AcTypeName { get; set; }

        public List<SelectListItem>? Aircraft { get; set; }
    }
}
