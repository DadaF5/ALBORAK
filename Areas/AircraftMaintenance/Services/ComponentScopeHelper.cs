using System.Security.Claims;
using FRAProject.Areas.AircraftMaintenance.Models;
// CONFIRMED this session against the real IUserScopeService.cs/UserScopeService.cs:
// the interface has exactly one method —
//   Task<UserScope> GetScopeAsync(ClaimsPrincipal user, string moduleCode);
// returning UserScope { IsUnrestricted, AllowedBaseIds, AllowedAcMainGroupIds, AllowedWingIds }.
// There is no IsAircraftInScopeAsync/IsAcTypeInScopeAsync on the real interface —
// those were invented in an earlier pass based on a since-superseded handoff doc.
// Every caller that needs an aircraft/base-level yes-or-no check must call
// GetScopeAsync itself and compare against the returned UserScope, which is
// what this helper now does.
using FRAProject.Services;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    /// <summary>
    /// Third scoping shape for this module (see design doc §3): Component's
    /// scope depends on its own Status —
    ///   Installed              -> aircraft-instance scoping, via the aircraft's BaseId
    ///   InStock / UnderRepair  -> Base-level scoping via StockBaseId
    /// Base-only (no AcMainGroup-level narrowing) for both cases — matches the
    /// original design, which never used AllowedAcMainGroupIds either; revisit
    /// if Component ever needs group-level scoping too.
    ///
    /// ASSUMPTION still open: the moduleCode string passed to GetScopeAsync
    /// ("MAINTENANCE") — not confirmed against how ModuleRole.ModuleCode is
    /// actually seeded/used elsewhere for the Aircraft Maintenance area. Fix
    /// if the real code differs (this is a runtime string match, not something
    /// that would show up as a compile error).
    /// </summary>
    public interface IComponentScopeHelper
    {
        Task<bool> IsComponentInScopeAsync(ClaimsPrincipal user, Component component);

        /// <summary>
        /// NEW — same rule as IsComponentInScopeAsync, against an ALREADY-RESOLVED
        /// UserScope. Added for ComponentService.GetScopedPagedListAsync, which
        /// loops over potentially thousands of Components for one request —
        /// calling IsComponentInScopeAsync per row meant one GetScopeAsync
        /// (itself a DB round trip) per Component, an N+1 that was invisible
        /// at the old handful-of-rows test scale but would not have stayed
        /// invisible once the real fleet (thousands of engines alone) loaded.
        /// Callers looping over many Components should call GetScopeAsync
        /// ONCE and use this overload; IsComponentInScopeAsync (single-item
        /// call sites — Details/Install/Remove/etc., unchanged) now just
        /// resolves the scope once and delegates here.
        /// </summary>
        bool IsComponentInScope(Component component, UserScope scope);

        /// <summary>NEW — effective current BaseId for a Component (CurrentAircraft.BaseId when Installed, StockBaseId otherwise) — the same split used for scoping, exposed so the Index filter's "Base" dropdown can filter on the same concept rather than reimplementing it.</summary>
        int? GetEffectiveBaseId(Component component);
    }

    public class ComponentScopeHelper : IComponentScopeHelper
    {
        private readonly IUserScopeService _scope;
        public ComponentScopeHelper(IUserScopeService scope) => _scope = scope;

        public async Task<bool> IsComponentInScopeAsync(ClaimsPrincipal user, Component component)
        {
            var scope = await _scope.GetScopeAsync(user, "MAINTENANCE");
            return IsComponentInScope(component, scope);
        }

        public bool IsComponentInScope(Component component, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;

            // Requires CurrentAircraft to be eager-loaded by the caller's query
            // (GetAllWithCurrentLocationAsync/GetWithDetailsAsync both already
            // Include it) — GetEffectiveBaseId falls back to null (hidden)
            // rather than throwing if it somehow isn't loaded.
            var baseId = GetEffectiveBaseId(component);
            return baseId.HasValue && scope.AllowedBaseIds.Contains(baseId.Value);
        }

        public int? GetEffectiveBaseId(Component component)
        {
            return component.Status switch
            {
                ComponentStatus.Installed when component.CurrentAircraftId.HasValue => component.CurrentAircraft?.BaseId,
                // InStock / UnderRepair / Removed (transitional) / Scrapped — no
                // live aircraft to scope against; fall back to Base via
                // StockBaseId if still set, otherwise null (hidden, safest
                // default) — same fallback the old switch used for its
                // "default" branch.
                _ => component.StockBaseId
            };
        }
    }
}
