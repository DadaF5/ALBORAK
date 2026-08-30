// Areas/SquadronOps/Repositories/IOdvRepository.cs
//
// NEW (2026-08-29, "redesign from zero" pass). Extends
// IGenericRepository<Odv> — same confirmed convention as
// IWorkOrderRepository — for full generic CRUD plus the two Include-aware
// queries the OdvPlanning board and Cancel action actually need. This is
// what supersedes Batch 1's plain "IGenericRepository<Odv> Odvs" entry,
// since the plain generic repo has no .Include()/.ThenInclude() support at
// all (confirmed by reading the real GenericRepository.cs).
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.SquadronOps.Repositories
{
    public interface IOdvRepository : IGenericRepository<Odv>
    {
        /// <summary>
        /// The OdvPlanning Index board for one date: Mission, AcMainGroup,
        /// CallSign, and every Sortie with its AcType and
        /// SortieCrews.CrewMember eager-loaded. allowedSquadronIds == null
        /// means unrestricted (no squadron filter); allowedAcMainGroupIds
        /// == null or empty means no AcMainGroup filter — same "empty list
        /// = no restriction" semantics as UserScope.AllowedAcMainGroupIds.
        /// </summary>
        Task<List<Odv>> GetBoardForDateAsync(DateTime date, HashSet<int>? allowedSquadronIds, HashSet<int>? allowedAcMainGroupIds);

        /// <summary>
        /// A single (tracked) Odv with its Sorties included — used by
        /// Cancel, which mutates both the Odv and its child Sorties in one
        /// SaveChanges.
        /// </summary>
        Task<Odv?> GetByIdWithSortiesAsync(int id);
    }
}
