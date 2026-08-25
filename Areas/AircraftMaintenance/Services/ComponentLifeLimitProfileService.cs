using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public interface IComponentLifeLimitProfileService
    {
        Task<List<ComponentLifeLimitProfileListDto>> GetByComponentTypeAsync(int componentTypeId);
        Task<ComponentLifeLimitProfileFormDto?> GetForEditAsync(int id);
        Task<(bool Success, string Message, int? Id)> SaveAsync(ComponentLifeLimitProfileFormDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public class ComponentLifeLimitProfileService : IComponentLifeLimitProfileService
    {
        private readonly IUnitOfWork _uow;
        private readonly IComponentLifeStatusCalculator _calculator;

        public ComponentLifeLimitProfileService(
            IUnitOfWork uow,
            IComponentLifeStatusCalculator calculator)
        {
            _uow = uow;
            _calculator = calculator;
        }

        public async Task<List<ComponentLifeLimitProfileListDto>> GetByComponentTypeAsync(int componentTypeId)
        {
            var profiles = await _uow.ComponentLifeLimitProfiles.GetByComponentTypeAsync(componentTypeId);
            return profiles.Select(p => new ComponentLifeLimitProfileListDto
            {
                Id = p.Id,
                ApplicabilityRuleType = p.ApplicabilityRuleType,
                SerialNumber = p.SerialNumber,
                SerialNumberPrefix = p.SerialNumberPrefix,
                SerialBoundary = p.SerialBoundary,
                Reason = p.Reason,
                LifeBasis = p.LifeBasis,
                IsActive = p.IsActive,
                StageCount = p.Stages.Count,
                StageSummaries = BuildStageSummaries(p.Stages)
            }).ToList();
        }

        /// <summary>
        /// NEW — one human-readable line per stage (e.g. "Révision : FH 900 h",
        /// "Réforme : FH 3000 h (limite 3000h)") for the ManageLifeLimits list,
        /// which previously showed only a bare stage count with no way to see
        /// the actual configured values without opening Modifier. Uses the
        /// same DimensionUnitConverter.ToDisplayValue the edit form uses, so
        /// the numbers match exactly (stored minutes -> displayed hours, etc).
        /// </summary>
        private static List<string> BuildStageSummaries(IEnumerable<ComponentLifeLimitStage> stages)
        {
            var result = new List<string>();
            foreach (var s in stages.OrderBy(s => s.SequenceOrder))
            {
                var stageLabel = s.StageType == ComponentLifeLimitStageType.Overhaul ? "Révision" : "Réforme";
                var dimParts = s.Dimensions
                    .Where(d => d.DimensionType != null)
                    .OrderBy(d => d.DimensionType!.SortOrder)
                    .Select(d =>
                    {
                        var unit = d.DimensionType!.Unit;
                        var unitSuffix = unit == ComponentLifeLimitDimensionUnit.Hours ? "h"
                            : unit == ComponentLifeLimitDimensionUnit.Days ? "j"
                            : unit == ComponentLifeLimitDimensionUnit.Months ? "mois" // NEW
                            : unit == ComponentLifeLimitDimensionUnit.Years ? "ans" : ""; // NEW
                        var interval = DimensionUnitConverter.ToDisplayValue(unit, d.Interval);
                        var bandEnd = DimensionUnitConverter.ToDisplayValue(unit, d.BandEnd);
                        var text = $"{d.DimensionType.Code} {interval}{unitSuffix}".TrimEnd();
                        if (bandEnd.HasValue) text += $" (limite {bandEnd}{unitSuffix})";
                        return text;
                    });
                result.Add($"{stageLabel} : {string.Join(", ", dimParts)}");
            }
            return result;
        }

        public async Task<ComponentLifeLimitProfileFormDto?> GetForEditAsync(int id)
        {
            var p = await _uow.ComponentLifeLimitProfiles.GetWithStagesAsync(id);
            if (p == null) return null;

            return new ComponentLifeLimitProfileFormDto
            {
                Id = p.Id,
                ComponentTypeId = p.ComponentTypeId,
                ApplicabilityRuleType = p.ApplicabilityRuleType,
                SerialNumber = p.SerialNumber,
                SerialNumberPrefix = p.SerialNumberPrefix,
                SerialBoundary = p.SerialBoundary,
                Reason = p.Reason,
                LifeBasis = p.LifeBasis,
                IsActive = p.IsActive,
                Stages = p.Stages.OrderBy(s => s.SequenceOrder).Select(s => new ComponentLifeLimitStageFormDto
                {
                    SequenceOrder = s.SequenceOrder,
                    StageType = s.StageType,
                    ToleranceType = s.ToleranceType,
                    Dimensions = s.Dimensions
                        .Where(d => d.DimensionType != null)
                        .OrderBy(d => d.DimensionType!.SortOrder)
                        .Select(d => new ComponentLifeLimitStageDimensionFormDto
                        {
                            DimensionTypeId = d.DimensionTypeId,
                            DimensionTypeCode = d.DimensionType!.Code,
                            DimensionTypeName = d.DimensionType.Name,
                            Unit = d.DimensionType.Unit,
                            ReferenceBasisId = d.ReferenceBasisId,
                            ReferenceBasisCode = d.ReferenceBasis?.Code,
                            ReferenceBasisName = d.ReferenceBasis?.Name,
                            Interval = DimensionUnitConverter.ToDisplayValue(d.DimensionType.Unit, d.Interval),
                            BandEnd = DimensionUnitConverter.ToDisplayValue(d.DimensionType.Unit, d.BandEnd),
                            Tolerance = DimensionUnitConverter.ToDisplayValue(d.DimensionType.Unit, d.Tolerance),
                        }).ToList()
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message, int? Id)> SaveAsync(ComponentLifeLimitProfileFormDto dto)
        {
            if (dto.ApplicabilityRuleType == ApplicabilityRuleType.Specific && string.IsNullOrWhiteSpace(dto.SerialNumber))
                return (false, "Le numéro de série est requis pour une applicabilité 'Spécifique'.", null);

            if ((dto.ApplicabilityRuleType == ApplicabilityRuleType.RangeFrom || dto.ApplicabilityRuleType == ApplicabilityRuleType.RangeTo)
                && string.IsNullOrWhiteSpace(dto.SerialBoundary))
                return (false, "La borne numérique est requise pour une applicabilité par plage.", null);

            if (dto.ApplicabilityRuleType == ApplicabilityRuleType.PnBased
                && await _uow.ComponentLifeLimitProfiles.HasActivePnBasedProfileAsync(dto.ComponentTypeId, dto.Id))
                return (false, "Un profil par défaut (PN_BASED) actif existe déjà pour ce numéro de pièce.", null);

            if (!dto.Stages.Any())
                return (false, "Au moins une étape est requise.", null);

            // NEW — ReferenceBasisId lives on the per-stage row (see
            // ComponentLifeLimitStageDimension doc comment for why), but a
            // dimension's basis should read as ONE choice per profile, not
            // a different one per stage — enforce that agreement here since
            // the DB schema itself doesn't (application-level rule).
            var inconsistentDimension = dto.Stages
                .SelectMany(s => s.Dimensions)
                .GroupBy(d => d.DimensionTypeId)
                .FirstOrDefault(g => g.Select(d => d.ReferenceBasisId).Distinct().Count() > 1);
            if (inconsistentDimension != null)
            {
                var dimName = inconsistentDimension.First().DimensionTypeName ?? $"Dimension #{inconsistentDimension.Key}";
                return (false, $"« {dimName} » doit utiliser la même référence de calcul sur toutes les étapes où elle apparaît.", null);
            }

            int profileId;

            if (dto.Id is null)
            {
                var entity = new ComponentLifeLimitProfile
                {
                    ComponentTypeId = dto.ComponentTypeId,
                    ApplicabilityRuleType = dto.ApplicabilityRuleType,
                    SerialNumber = dto.SerialNumber,
                    SerialNumberPrefix = dto.SerialNumberPrefix,
                    SerialBoundary = dto.SerialBoundary,
                    Reason = dto.Reason,
                    LifeBasis = dto.LifeBasis,
                    IsActive = dto.IsActive
                };
                _uow.ComponentLifeLimitProfiles.Add(entity);
                await _uow.CompleteAsync(); // entity.Id populated after this
                profileId = entity.Id;
            }
            else
            {
                var existing = await _uow.ComponentLifeLimitProfiles.GetByIdAsync(dto.Id.Value);
                if (existing == null) return (false, "Profil introuvable.", null);

                existing.ApplicabilityRuleType = dto.ApplicabilityRuleType;
                existing.SerialNumber = dto.SerialNumber;
                existing.SerialNumberPrefix = dto.SerialNumberPrefix;
                existing.SerialBoundary = dto.SerialBoundary;
                existing.Reason = dto.Reason;
                existing.LifeBasis = dto.LifeBasis;
                existing.IsActive = dto.IsActive;
                _uow.ComponentLifeLimitProfiles.Update(existing);
                profileId = existing.Id;
            }

            // Look up the authoritative Unit for each posted DimensionTypeId
            // from the DB rather than trusting whatever Unit value round-tripped
            // through the form — DimensionTypeId is the only field the service
            // trusts from the posted row (see ComponentLifeLimitStageDimensionFormDto).
            var dimensionTypesById = (await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync())
                .ToDictionary(d => d.Id);

            // NEW — same "look up the authoritative row, trust only the Id"
            // discipline as dimensionTypesById above: ReferenceBasisId is the
            // only reference-basis field the service trusts from the posted
            // row (ReferenceBasisCode/Name are display-only, ignored here).
            var referenceBasisIds = (await _uow.ComponentReferenceBases.GetAllAsync())
                .Select(b => b.Id)
                .ToHashSet();

            var stages = dto.Stages.Select(s => new ComponentLifeLimitStage
            {
                StageType = s.StageType,
                ToleranceType = s.ToleranceType,
                Dimensions = s.Dimensions
                    .Where(d => dimensionTypesById.ContainsKey(d.DimensionTypeId))
                    .Select(d =>
                    {
                        var unit = dimensionTypesById[d.DimensionTypeId].Unit;
                        return new ComponentLifeLimitStageDimension
                        {
                            DimensionTypeId = d.DimensionTypeId,
                            // A posted basis Id that doesn't resolve to a real
                            // row (stale/removed) is dropped back to null
                            // (profile-default) rather than saved as a dangling
                            // reference — same "trust the Id, verify it" spirit
                            // as the DimensionTypeId filter above.
                            ReferenceBasisId = d.ReferenceBasisId.HasValue && referenceBasisIds.Contains(d.ReferenceBasisId.Value)
                                ? d.ReferenceBasisId
                                : null,
                            Interval = DimensionUnitConverter.ToStoredValue(unit, d.Interval),
                            BandEnd = DimensionUnitConverter.ToStoredValue(unit, d.BandEnd),
                            Tolerance = DimensionUnitConverter.ToStoredValue(unit, d.Tolerance),
                        };
                    }).ToList()
            });

            await _uow.ComponentLifeLimitProfiles.ReplaceStagesAsync(profileId, stages);
            await _uow.CompleteAsync();

            await RecomputeAffectedComponentsAsync(dto.ComponentTypeId);

            return (true, "Profil de durée de vie enregistré avec succès.", profileId);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var profile = await _uow.ComponentLifeLimitProfiles.GetByIdAsync(id);
            if (profile == null) return (false, "Profil introuvable.");

            var componentTypeId = profile.ComponentTypeId;
            _uow.ComponentLifeLimitProfiles.Delete(profile);
            await _uow.CompleteAsync();

            await RecomputeAffectedComponentsAsync(componentTypeId);

            return (true, "Profil supprimé.");
        }

        /// <summary>
        /// Changing a profile can change which profile resolves for existing
        /// Components of this type — recompute all of them so ComponentLifeStatus
        /// doesn't go stale silently. Fine at this data volume; revisit with a
        /// background job if the catalog grows very large.
        /// </summary>
        private async Task RecomputeAffectedComponentsAsync(int componentTypeId)
        {
            var all = await _uow.Components.GetAllWithCurrentLocationAsync();
            foreach (var c in all.Where(c => c.ComponentTypeId == componentTypeId))
            {
                await _calculator.RecomputeAsync(c.Id);
            }
        }
    }
}
