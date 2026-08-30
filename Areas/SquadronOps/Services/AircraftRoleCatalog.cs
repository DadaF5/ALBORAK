using FRAProject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FRAProject.Areas.SquadronOps.Services
{
    // NEW (Batch 16, 2026-08-30), RE-KEYED (Batch 17, 2026-08-30) — per
    // Dadda's real-world concern: as more squadrons come onto SquadronOps
    // (F16, C130, and eventually others), a single global AircraftRole
    // list offered identically on every sortie stops making sense. An F16
    // crew shouldn't see "Passenger" or "Loadmaster"; a C130 crew
    // shouldn't see "WeaponOfficer".
    //
    // FIX (Batch 17) — Batch 16 keyed this on AcMainGroup.Code ("F16-2B"),
    // which was wrong: Dadda's own query showed the F16-2B group contains
    // BOTH the single-seat F16C and the two-seat F16D. Keying on the
    // group meant a single-seat F16C sortie was wrongly offered Copilot/
    // WeaponOfficer/FlightEngineer too — those only apply to the two-seat
    // D model. Re-keyed on AcType.Code instead (the specific airframe
    // variant a Sortie actually carries, via the real Sortie.AcType
    // navigation property — confirmed directly from Sortie.cs), which is
    // the correct granularity: role sets can differ between AcTypes in
    // the SAME AcMainGroup, but never differ within one AcType.
    //
    // Per Dadda's confirmed choice ("Code-level mapping", not a new DB
    // table/Réglages screen), this is a small in-code lookup — cheapest to
    // ship, at the cost of needing a code change (a small one) whenever a
    // new aircraft type's role set needs to be added or changed.
    //
    // Keyed on AcType.Code, NOT Id or Name — same reasoning as the
    // AcMainGroup keying decision this replaces: Id is environment-
    // specific, Name was never confirmed, Code is confirmed real/short/
    // unique (verified directly against a real query against
    // [ALBORAK].[dbo].[AcTypes]: "F16C", "F16D", "C130H", "F5E", "F5F",
    // "AJET").
    //
    // IMPORTANT — what happens for an AcType.Code NOT listed here (F5E,
    // F5F, AJET at the time of writing, none of which has a confirmed
    // role set yet): GetAllowedRoles falls back to every AircraftRole
    // value, unfiltered. Deliberate — an unconfigured type shows
    // everything rather than silently and incorrectly restricting a
    // squadron nobody has described the crew structure for yet. Add its
    // Code and role list here once confirmed, the same way F16C/F16D/
    // C130H were.
    public static class AircraftRoleCatalog
    {
        private static readonly Dictionary<string, AircraftRole[]> ByAcTypeCode =
            new(StringComparer.OrdinalIgnoreCase)
        {
            // F16C — single-seat, per Dadda's confirmation ("F16 either
            // One seat or two"): only Captain, nothing else. No second
            // crew position exists on this airframe.
            ["F16C"] = new[]
            {
                AircraftRole.Captain
            },

            // F16D — two-seat variant of the same AcMainGroup. Captain
            // plus exactly ONE of Copilot/WeaponOfficer/FlightEngineer
            // (confirmed 2026-08-30). This catalog only controls which
            // roles are OFFERED, not "exactly one of the other three" as
            // a hard rule — that combination logic isn't enforced
            // anywhere yet (see the Batch 16/17 README's open item).
            ["F16D"] = new[]
            {
                AircraftRole.Captain,
                AircraftRole.Copilot,
                AircraftRole.WeaponOfficer,
                AircraftRole.FlightEngineer
            },

            // C130H — confirmed 2026-08-30: Captain, Co-Pilot, Navigator
            // (or Combat Systems Officer), Flight Engineer, Loadmaster.
            // Explicitly NOT Passenger or Mechanic — Dadda's answer added
            // only Captain on top of the four originally listed.
            ["C130H"] = new[]
            {
                AircraftRole.Captain,
                AircraftRole.Copilot,
                AircraftRole.Navigator,
                AircraftRole.FlightEngineer,
                AircraftRole.Loadmaster
            }
        };

        /// <summary>
        /// Returns the AircraftRole values valid for the given
        /// AcType.Code, in enum declaration order. Falls back to every
        /// AircraftRole value for a Code not (yet) configured above.
        /// </summary>
        public static IReadOnlyList<AircraftRole> GetAllowedRoles(string? acTypeCode)
        {
            if (!string.IsNullOrWhiteSpace(acTypeCode) &&
                ByAcTypeCode.TryGetValue(acTypeCode, out var configured))
            {
                return configured;
            }

            return Enum.GetValues(typeof(AircraftRole)).Cast<AircraftRole>().ToList();
        }
    }
}
