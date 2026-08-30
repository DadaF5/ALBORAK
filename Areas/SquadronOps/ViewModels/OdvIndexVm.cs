using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    public class OdvIndexVm
    {
        // Filters
        public int? SelectedSquadronId { get; set; }
        public DateTime SelectedDate { get; set; }
        public int? SelectedAcMainGroupId { get; set; }

        // NEW (Batch 14, 2026-08-30) — lets the view show the Escadron
        // filter dropdown only for an unrestricted (Admin) user, matching
        // the legacy CIPL_FlyingProgram.aspx page's Escadron dropdown. A
        // scoped user's own UserScope already limits them to their
        // squadron(s), so showing them a dropdown that can't actually
        // widen their view would be misleading.
        public bool IsUnrestrictedScope { get; set; }

        // Create-model (bind the create form to this so posted values are preserved)
        public OdvCreateVm? CreateModel { get; set; }

        // Select lists for the page
        public List<SelectListItem>? Squadrons { get; set; }
        public List<SelectListItem>? AcMainGroups { get; set; }
        public List<SelectListItem>? Missions { get; set; }
        public List<SelectListItem>? CallSigns { get; set; } // Value = Id.ToString()
        public List<SelectListItem>? Aircrafts { get; set; }
        public List<SelectListItem>? AcTypes { get; set; }
        public List<SelectListItem>? CrewMembers { get; set; }

        // in OdvIndexVm
        public List<SelectListItem>? ZoneList { get; set; }
        public List<SelectListItem>? MissionTypeList { get; set; }

        // NEW (Batch 15, 2026-08-30) — full entities (not just
        // SelectListItem Value/Text pairs) for the new "Ajouter une sortie
        // à un ODV" combined card. That card's ODV dropdown carries each
        // ODV's own SquadronId/AcMainGroupId (already rendered from Odvs
        // below), and client-side JS filters these two lists against
        // whichever ODV is selected — AcTypesFull by AcMainGroupId, and
        // CrewMembersFull by SquadronId — so the Type avion / crew
        // dropdowns only ever offer choices valid for that ODV. This is a
        // client-side convenience only: both SortiesController.Create and
        // SortieCrewsController.Create/.Edit already re-validate the same
        // constraints server-side (confirmed in their real code), so a
        // stale/tampered client selection is still safely rejected either
        // way.
        //
        // FIX (Batch 16, 2026-08-30) — AcType's real namespace is
        // FRAProject.Areas.Settings.Models, confirmed directly from the
        // real AcType.cs. Batch 15 had this file importing
        // FRAProject.Areas.AircraftMaintenance.Models instead (copied from
        // a using line elsewhere without checking it was the right one for
        // THIS type) — would not have compiled. Fixed here; see this
        // file's top using list.
        //
        // RE-USE (Batch 17, 2026-08-30) — this same list now also drives
        // the redesigned combined card's per-row Role dropdown, since
        // AircraftRoleCatalog was re-keyed from AcMainGroup.Code to
        // AcType.Code (Batch 16's F16-2B keying wrongly gave the
        // single-seat F16C the two-seat F16D's extra roles — see
        // AircraftRoleCatalog.cs's header for the full story). Each AcType
        // here already carries its own Code, so the Batch 16
        // AcMainGroupsFull field below is no longer needed and has been
        // removed.
        public List<AcType>? AcTypesFull { get; set; }
        public List<CrewMember>? CrewMembersFull { get; set; }

        // Data to display
        public List<Odv>? Odvs { get; set; }
    }
}
