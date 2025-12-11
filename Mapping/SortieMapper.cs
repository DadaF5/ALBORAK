using FRAProject.Models;
using FRAProject.ViewModels;

namespace FRAProject.Mapping
{
    /// <summary>
    /// Small mapper for the "planned/initial" creation of a Sortie from SortieVm.
    /// Only maps the fields you said are provided when a sortie is first added.
    /// </summary>
    public static class SortieMapper
    {
        public static Sortie MapForCreate(SortieVm vm, int odvId, int? baseId = null, string? createdBy = null)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            var sortie = new Sortie
            {
                OdvId = odvId,
                BaseId = baseId,
                AircraftId = vm.AircraftId,
                Configuration = vm.Configuration,
                FuelQuantity = vm.FuelQuantity,
                StartTime = vm.StartTime,
                LandingTime = vm.LandingTime,
                TOFF = vm.TOFF,
                RealTOFF = vm.RealTOFF,
                RealLandingTime = vm.RealLandingTime,
                Notes = vm.Notes,
                IsCompleted = vm.IsCompleted,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            // Map crew entries, if any (now using CrewMemberId)
            if (vm.Crew != null && vm.Crew.Any())
            {
                foreach (var c in vm.Crew)
                {
                    var sc = new SortieCrew
                    {
                        CrewMemberId = c.CrewMemberId,
                        Role = c.Role,
                        IsPrimary = c.IsPrimary,
                        Remarks = c.Remarks
                       
                    };
                    sortie.SortieCrews.Add(sc);
                }
            }
            return sortie;
        }
    }
}