// Areas/SquadronOps/Repositories/ISortiePlanningRepository.cs
//
// NEW (2026-08-29, "redesign from zero" pass). Full CRUD + Include-aware
// repository for Sortie, owned by SquadronOps.
//
// Deliberately NOT named ISortieRepository / not registered on IUnitOfWork
// as "Sorties" — that name is already taken by the real, existing
// FRAProject.Areas.AircraftMaintenance.Repositories.ISortieRepository,
// which is a narrow, Maintenance-owned, READ-ONLY specialist with exactly
// one method (GetAccumulatedFHByAcTypeAsync — FH aggregation for
// Maintenance's own purposes) and does NOT extend IGenericRepository<T> at
// all, so it has no Add/Update/GetById etc. Reusing that interface or
// property name for SquadronOps' own Sortie CRUD needs would either
// collide (two same-named types in scope) or silently overload a
// Maintenance-owned abstraction with SquadronOps concerns it was never
// designed for. This is a separate, SquadronOps-owned repository for the
// same underlying table — same pattern as WorkOrder, not touching
// Maintenance's SortieRepository.cs at all.
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.SquadronOps.Repositories
{
    public interface ISortiePlanningRepository : IGenericRepository<Sortie>
    {
        /// <summary>
        /// A single (tracked) Sortie with its parent Odv included — Edit
        /// and Cancel both need Odv.SquadronId/AcMainGroupId for the scope
        /// check.
        /// </summary>
        Task<Sortie?> GetByIdWithOdvAsync(int id);
    }
}
