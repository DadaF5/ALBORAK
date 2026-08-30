// Areas/SquadronOps/Repositories/ISquadronRepository.cs
//
// NEW (2026-08-29, "redesign from zero" pass). Specialist repository for
// Squadron — same convention confirmed from the real IWorkOrderRepository /
// WorkOrderRepository pair: extends IGenericRepository<T> so callers get
// full generic CRUD PLUS these hand-written custom methods.
//
// Why Squadron gets a specialist repo even though this redesign was scoped
// to "just Odv + Sortie": the Squadron -> Wing -> Department -> Base scope
// join (needed to resolve a squadron's REAL authorization-scope base — see
// ALBORAK_SquadronOps_FreshStart_Handoff.md, "three distinct base
// concepts") was duplicated three times across the two controllers being
// rebuilt (OdvPlanningController.IsSquadronInScopeAsync/
// GetInScopeSquadronIdsAsync, SortiesController.IsOdvInScopeAsync) — and
// that exact duplication is what caused the real Wing.BaseId-vs-
// Department.BaseId bug fixed earlier this session. Re-duplicating the
// same query into two new repository classes during this redesign would
// just recreate the same risk in a new place, so it gets ONE home here
// instead. If you'd rather keep this strictly out of scope, say so and
// these two methods can move back into private controller helpers
// un-converted (still going through the DbContext), same as before.
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.SquadronOps.Repositories
{
    public interface ISquadronRepository : IGenericRepository<Squadron>
    {
        /// <summary>
        /// Resolves a squadron's authorization-scope Wing and Base via
        /// Squadron -> Wing -> Department -> Base (Department.BaseId is
        /// [Required], never null — this is the ONLY correct base to use
        /// for scoping checks). Returns null if the squadron doesn't exist.
        /// Wing.BaseId is a separate, still-unconfirmed-purpose field and
        /// must NOT be used here.
        /// </summary>
        Task<(int WingId, int BaseId)?> GetScopeInfoAsync(int squadronId);

        /// <summary>
        /// All squadron Ids whose scope-Base is in allowedBaseIds AND
        /// (allowedWingIds is empty OR whose scope-Wing is in
        /// allowedWingIds) — same semantics as the UserScope object
        /// (empty Wing list = no Wing-level restriction).
        /// </summary>
        Task<HashSet<int>> GetInScopeIdsAsync(IReadOnlyCollection<int> allowedBaseIds, IReadOnlyCollection<int> allowedWingIds);
    }
}
