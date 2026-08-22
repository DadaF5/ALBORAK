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
                SortOrder = t.SortOrder,
                EligiblePositionIds = await _uow.ComponentTypes.GetPositionIdsAsync(id)
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

            if (dto.EligiblePositionIds.Any())
            {
                await _uow.ComponentTypes.SetPositionsAsync(entity.Id, dto.EligiblePositionIds);
                await _uow.CompleteAsync();
            }

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
            await _uow.ComponentTypes.SetPositionsAsync(existing.Id, dto.EligiblePositionIds);
            await _uow.CompleteAsync();

            return (true, "Composant mis à jour avec succès.");
        }
    }
}
