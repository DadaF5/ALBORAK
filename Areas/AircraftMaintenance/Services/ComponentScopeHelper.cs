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
    }

    public class ComponentScopeHelper : IComponentScopeHelper
    {
        private readonly IUserScopeService _scope;
        public ComponentScopeHelper(IUserScopeService scope) => _scope = scope;

        public async Task<bool> IsComponentInScopeAsync(ClaimsPrincipal user, Component component)
        {
            var scope = await _scope.GetScopeAsync(user, "MAINTENANCE");
            if (scope.IsUnrestricted) return true;

            switch (component.Status)
            {
                case ComponentStatus.Installed when component.CurrentAircraftId.HasValue:
                    // Requires CurrentAircraft to be eager-loaded by the caller's
                    // query (GetAllWithCurrentLocationAsync/GetWithDetailsAsync
                    // both already Include it) — falls back to false (hidden)
                    // rather than throwing if it somehow isn't loaded.
                    var aircraftBaseId = component.CurrentAircraft?.BaseId;
                    return aircraftBaseId.HasValue && scope.AllowedBaseIds.Contains(aircraftBaseId.Value);

                case ComponentStatus.InStock:
                case ComponentStatus.UnderRepair:
                    return component.StockBaseId.HasValue && scope.AllowedBaseIds.Contains(component.StockBaseId.Value);

                default:
                    // Removed (transitional) / Scrapped — no live aircraft or stock
                    // location to scope against; fall back to Base via StockBaseId
                    // if still set, otherwise hidden (safest default).
                    return component.StockBaseId.HasValue && scope.AllowedBaseIds.Contains(component.StockBaseId.Value);
            }
        }
    }
}
