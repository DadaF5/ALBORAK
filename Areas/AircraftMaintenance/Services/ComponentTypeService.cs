using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public interface IComponentTypeService
    {
        Task<List<ComponentTypeListDto>> GetAllAsync(bool includeInactive = false);
        Task<ComponentTypeFormDto?> GetForEditAsync(int id);
        Task<(bool Success, string Message, int? Id)> CreateAsync(ComponentTypeFormDto dto);
        Task<(bool Success, string Message)> UpdateAsync(ComponentTypeFormDto dto);
        /// <summary>NEW — backs the Details "hub" page. See ComponentTypeDetailsDto's doc comment.</summary>
        Task<ComponentTypeDetailsDto?> GetDetailsAsync(int id);
        /// <summary>NEW (follow-up) — backs the dedicated ManagePositions page. See ComponentTypePositionsFormDto's doc comment.</summary>
        Task<(bool Success, string Message)> UpdatePositionsAsync(int componentTypeId, List<int> positionIds);
    }

    public class ComponentTypeService : IComponentTypeService
    {
        private readonly IUnitOfWork _uow;

        public ComponentTypeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<ComponentTypeListDto>> GetAllAsync(bool includeInactive = false)
        {
            var all = await _uow.ComponentTypes.GetAllWithLifeLimitProfilesAsync();
            return all
                .Where(t => includeInactive || t.IsActive)
                .OrderBy(t => t.PartNumber)
                .Select(t => new ComponentTypeListDto
                {
                    Id = t.Id,
                    PartNumber = t.PartNumber,
                    Nomenclature = t.Nomenclature,
                    AtaLabel = t.Ata?.Name,
                    TrackingMethod = t.TrackingMethod,
                    LifeLimitProfileCount = t.LifeLimitProfiles?.Count(p => p.IsActive) ?? 0,
                    IsActive = t.IsActive
                }).ToList();
        }

        public async Task<ComponentTypeFormDto?> GetForEditAsync(int id)
        {
            var t = await _uow.ComponentTypes.GetWithLifeLimitAsync(id);
            if (t == null) return null;

            return new ComponentTypeFormDto
            {
                Id = t.Id,
                PartNumber = t.PartNumber,
                Nomenclature = t.Nomenclature,
                AtaId = t.AtaId,
                AircraftManufacturerId = t.AircraftManufacturerId,
                TrackingMethod = t.TrackingMethod,
                IsSerialized = t.IsSerialized,
                IsActive = t.IsActive,
                SortOrder = t.SortOrder
            };
        }

        public async Task<(bool Success, string Message, int? Id)> CreateAsync(ComponentTypeFormDto dto)
        {
            if (await _uow.ComponentTypes.ExistsByPartNumberAsync(dto.PartNumber))
                return (false, "Ce numéro de pièce existe déjà.", null);

            var entity = new ComponentType
            {
                PartNumber = dto.PartNumber,
                Nomenclature = dto.Nomenclature,
                AtaId = dto.AtaId,
                AircraftManufacturerId = dto.AircraftManufacturerId,
                TrackingMethod = dto.TrackingMethod,
                IsSerialized = dto.IsSerialized,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder
            };
            _uow.ComponentTypes.Add(entity);
            await _uow.CompleteAsync(); // entity.Id populated after this

            // CHANGED (Details-hub-page pass, follow-up) — Positions éligibles
            // no longer posts as part of this form; set separately via
            // UpdatePositionsAsync from the new ManagePositions page, same
            // "configure after creation, from the Details hub" flow already
            // used for Life Limits/Sub-assembly Slots/Derogations.
            var message = dto.TrackingMethod == ComponentTrackingMethod.HardTime
                ? "Composant créé avec succès. Ajoutez maintenant au moins un profil de durée de vie."
                : "Composant créé avec succès.";

            return (true, message, entity.Id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(ComponentTypeFormDto dto)
        {
            if (dto.Id is null) return (false, "Identifiant manquant.");

            if (await _uow.ComponentTypes.ExistsByPartNumberAsync(dto.PartNumber, dto.Id))
                return (false, "Ce numéro de pièce existe déjà.");

            var existing = await _uow.ComponentTypes.GetWithLifeLimitAsync(dto.Id.Value);
            if (existing == null) return (false, "Composant introuvable.");

            existing.PartNumber = dto.PartNumber;
            existing.Nomenclature = dto.Nomenclature;
            existing.AtaId = dto.AtaId;
            existing.AircraftManufacturerId = dto.AircraftManufacturerId;
            existing.TrackingMethod = dto.TrackingMethod;
            existing.IsSerialized = dto.IsSerialized;
            existing.IsActive = dto.IsActive;
            existing.SortOrder = dto.SortOrder;

            _uow.ComponentTypes.Update(existing);
            await _uow.CompleteAsync();

            return (true, "Composant mis à jour avec succès.");
        }

        /// <summary>
        /// NEW (Details-hub-page pass, follow-up) — Positions éligibles now
        /// saves independently of the rest of the catalog form, same
        /// "dedicated Manage page" split already used for Life Limits/
        /// Sub-assembly Slots/Derogations rather than bundling it into
        /// UpdateAsync above.
        /// </summary>
        public async Task<(bool Success, string Message)> UpdatePositionsAsync(int componentTypeId, List<int> positionIds)
        {
            var existing = await _uow.ComponentTypes.GetWithLifeLimitAsync(componentTypeId);
            if (existing == null) return (false, "Composant introuvable.");

            await _uow.ComponentTypes.SetPositionsAsync(componentTypeId, positionIds);
            await _uow.CompleteAsync();

            return (true, "Positions éligibles mises à jour avec succès.");
        }

        /// <summary>
        /// NEW — one read query per sub-area count, same "small dataset, read
        /// and count in C#" pattern already used throughout this module
        /// (RecomputeAffectedComponentsAsync etc.) rather than adding
        /// dedicated COUNT-only repository methods for a page that's read
        /// far less often than it's linked from.
        /// </summary>
        public async Task<ComponentTypeDetailsDto?> GetDetailsAsync(int id)
        {
            var t = await _uow.ComponentTypes.GetWithLifeLimitAsync(id);
            if (t == null) return null;

            var positionIds = (await _uow.ComponentTypes.GetPositionIdsAsync(id)).ToHashSet();
            var eligibleLabels = (await _uow.ComponentPositions.GetAllActiveWithAcTypeAsync())
                .Where(p => positionIds.Contains(p.Id))
                .OrderBy(p => p.AcType?.Code).ThenBy(p => p.SortOrder).ThenBy(p => p.Name)
                .Select(p => $"{p.AcType?.Code} — {p.Code} — {p.Name}")
                .ToList();

            var slots = await _uow.ComponentTypeSlots.GetByParentComponentTypeAsync(id, includeInactive: false);
            var derogations = await _uow.ComponentDerogations.GetByComponentTypeAsync(id);
            var componentCount = (await _uow.Components.GetAllWithCurrentLocationAsync())
                .Count(c => c.ComponentTypeId == id);

            return new ComponentTypeDetailsDto
            {
                Id = t.Id,
                PartNumber = t.PartNumber,
                Nomenclature = t.Nomenclature,
                AtaLabel = t.Ata != null ? $"{t.Ata.Code} — {t.Ata.Name}" : null,
                AircraftManufacturerLabel = t.AircraftManufacturer != null ? $"{t.AircraftManufacturer.Code} — {t.AircraftManufacturer.Name}" : null,
                TrackingMethod = t.TrackingMethod,
                IsSerialized = t.IsSerialized,
                IsActive = t.IsActive,
                EligiblePositionLabels = eligibleLabels,
                LifeLimitProfileCount = t.LifeLimitProfiles?.Count(p => p.IsActive) ?? 0,
                SubAssemblySlotCount = slots.Count,
                DerogationActiveCount = derogations.Count(d => d.IsActive),
                DerogationTotalCount = derogations.Count,
                ComponentCount = componentCount
            };
        }
    }
}
