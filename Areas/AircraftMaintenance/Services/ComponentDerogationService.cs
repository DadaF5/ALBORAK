using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    /// <summary>
    /// NEW — CRUD for ComponentDerogation. Create + Void only, deliberately
    /// no Update/Delete — append-only history (see ComponentDerogation.cs
    /// class doc comment). ComponentLifeStatusCalculator now DOES consult
    /// this table (see that class) — CreateAsync/VoidAsync below trigger the
    /// same "recompute every affected Component" pass
    /// ComponentLifeLimitProfileService uses after a profile change, so a
    /// new/voided derogation is reflected immediately rather than sitting
    /// stale until an unrelated ComponentEvent happens to that S/N.
    /// </summary>
    public interface IComponentDerogationService
    {
        Task<List<ComponentDerogationListDto>> GetByComponentTypeAsync(int componentTypeId);
        Task<(bool Success, string Message, int? Id)> CreateAsync(ComponentDerogationFormDto dto, string? createdByUserId);
        /// <summary>NEW — Void action. Flips IsActive=false and stamps VoidReason/VoidedAtUtc/VoidedByUserId ONLY — every other field on the row is left exactly as originally entered (append-only discipline preserved; this is a status flag, not an edit).</summary>
        Task<(bool Success, string Message)> VoidAsync(ComponentDerogationVoidDto dto, string? voidedByUserId);
    }

    public class ComponentDerogationService : IComponentDerogationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IComponentLifeStatusCalculator _calculator;

        public ComponentDerogationService(IUnitOfWork uow, IComponentLifeStatusCalculator calculator)
        {
            _uow = uow;
            _calculator = calculator;
        }

        public async Task<List<ComponentDerogationListDto>> GetByComponentTypeAsync(int componentTypeId)
        {
            var derogations = await _uow.ComponentDerogations.GetByComponentTypeAsync(componentTypeId);
            return derogations.Select(d => new ComponentDerogationListDto
            {
                Id = d.Id,
                Summary = BuildSummary(d),
                DimensionTypeCode = d.DimensionType?.Code,
                DimensionTypeName = d.DimensionType?.Name,
                TargetStageType = d.TargetStageType,
                ApplicabilityRuleType = d.ApplicabilityRuleType,
                SerialNumber = d.SerialNumber,
                SerialNumberPrefix = d.SerialNumberPrefix,
                SerialBoundary = d.SerialBoundary,
                LotReference = d.LotReference,
                Mode = d.Mode,
                Direction = d.Direction,
                Value = d.Value,
                Reference = d.Reference,
                Reason = d.Reason,
                IssuedDate = d.IssuedDate,
                EffectiveUntil = d.EffectiveUntil,
                Tier = d.Tier,
                ApprovalAuthority = d.ApprovalAuthority,
                SupportingEvidence = d.SupportingEvidence,
                IsConditional = d.IsConditional,
                ConditionDescription = d.ConditionDescription,
                IsActive = d.IsActive,
                SupersedesDerogationId = d.SupersedesDerogationId,
                VoidReason = d.VoidReason,
                VoidedAtUtc = d.VoidedAtUtc,
                VoidedByUserName = d.VoidedByUser?.FullLabel ?? d.VoidedByUserId // same "FullLabel or fall back to raw Id" convention as ComponentService's PerformedByUserName
            }).ToList();
        }

        /// <summary>NEW — same "compute one human-readable line server-side" convention as ComponentLifeLimitProfileService.BuildStageSummaries, e.g. "Extension : +24 mois (Absolue) — Réforme".</summary>
        private static string BuildSummary(ComponentDerogation d)
        {
            var unit = d.DimensionType?.Unit;
            var unitSuffix = unit == ComponentLifeLimitDimensionUnit.Hours ? "h"
                : unit == ComponentLifeLimitDimensionUnit.Days ? "j"
                : unit == ComponentLifeLimitDimensionUnit.Months ? " mois"
                : unit == ComponentLifeLimitDimensionUnit.Years ? " ans" : "";
            var sign = d.Direction == DerogationDirection.Extension ? "+" : "-";
            var valueText = d.Mode == ComponentToleranceType.PercentOfInterval
                ? $"{sign}{d.Value}%"
                : $"{sign}{d.Value}{unitSuffix}";
            var directionLabel = d.Direction == DerogationDirection.Extension ? "Extension" : "Réduction";
            var modeLabel = d.Mode == ComponentToleranceType.PercentOfInterval ? "% de la valeur d'origine" : "Absolue";
            var stageLabel = d.TargetStageType == ComponentLifeLimitStageType.Overhaul ? "Révision" : "Réforme";
            return $"{directionLabel} : {valueText} ({modeLabel}) — {stageLabel}";
        }

        public async Task<(bool Success, string Message, int? Id)> CreateAsync(ComponentDerogationFormDto dto, string? createdByUserId)
        {
            if (dto.ApplicabilityRuleType == ApplicabilityRuleType.Specific && string.IsNullOrWhiteSpace(dto.SerialNumber))
                return (false, "Le numéro de série est requis pour une applicabilité 'Spécifique'.", null);

            if ((dto.ApplicabilityRuleType == ApplicabilityRuleType.RangeFrom || dto.ApplicabilityRuleType == ApplicabilityRuleType.RangeTo)
                && string.IsNullOrWhiteSpace(dto.SerialBoundary))
                return (false, "La borne numérique est requise pour une applicabilité par plage.", null);

            if (dto.ApplicabilityRuleType == ApplicabilityRuleType.Lot && string.IsNullOrWhiteSpace(dto.LotReference))
                return (false, "La référence de lot est requise pour une applicabilité 'Lot'.", null);

            if (dto.IsConditional && string.IsNullOrWhiteSpace(dto.ConditionDescription))
                return (false, "La description de la condition est requise pour une dérogation conditionnelle.", null);

            // Same "trust only the Id, look up the authoritative row"
            // discipline as ComponentLifeLimitProfileService — DimensionTypeCode/
            // Name are display-only, ignored here.
            var dimensionType = await _uow.ComponentLifeLimitDimensionTypes.GetByIdAsync(dto.DimensionTypeId);
            if (dimensionType == null || !dimensionType.IsActive)
                return (false, "Dimension invalide ou inactive.", null);

            if (dto.SupersedesDerogationId.HasValue)
            {
                var superseded = await _uow.ComponentDerogations.GetByIdAsync(dto.SupersedesDerogationId.Value);
                if (superseded == null || superseded.ComponentTypeId != dto.ComponentTypeId)
                    return (false, "La dérogation à corriger/annuler est introuvable pour ce numéro de pièce.", null);
            }

            var entity = new ComponentDerogation
            {
                ComponentTypeId = dto.ComponentTypeId,
                DimensionTypeId = dto.DimensionTypeId,
                TargetStageType = dto.TargetStageType,
                ApplicabilityRuleType = dto.ApplicabilityRuleType,
                SerialNumber = dto.SerialNumber,
                SerialNumberPrefix = dto.SerialNumberPrefix,
                SerialBoundary = dto.SerialBoundary,
                LotReference = dto.LotReference,
                Mode = dto.Mode,
                Direction = dto.Direction,
                Value = dto.Value,
                Reference = dto.Reference,
                Reason = dto.Reason,
                IssuedDate = dto.IssuedDate,
                EffectiveUntil = dto.EffectiveUntil,
                Tier = dto.Tier,
                ApprovalAuthority = dto.ApprovalAuthority,
                SupportingEvidence = dto.SupportingEvidence,
                IsConditional = dto.IsConditional,
                ConditionDescription = dto.ConditionDescription,
                SupersedesDerogationId = dto.SupersedesDerogationId,
                IsActive = true,
                CreatedByUserId = createdByUserId
            };

            _uow.ComponentDerogations.Add(entity);
            await _uow.CompleteAsync();

            await RecomputeAffectedComponentsAsync(dto.ComponentTypeId);

            return (true, "Dérogation enregistrée avec succès.", entity.Id);
        }

        public async Task<(bool Success, string Message)> VoidAsync(ComponentDerogationVoidDto dto, string? voidedByUserId)
        {
            var entity = await _uow.ComponentDerogations.GetByIdAsync(dto.Id);
            if (entity == null || entity.ComponentTypeId != dto.ComponentTypeId)
                return (false, "Dérogation introuvable pour ce numéro de pièce.");

            if (!entity.IsActive)
                return (false, "Cette dérogation est déjà annulée.");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return (false, "Le motif d'annulation est requis.");

            // Only the void fields change — every other field (Value, Mode,
            // Reference, IssuedDate, etc.) is left exactly as originally
            // entered. This is a status flag, not an edit of the record.
            entity.IsActive = false;
            entity.VoidReason = dto.Reason;
            entity.VoidedAtUtc = DateTime.UtcNow;
            entity.VoidedByUserId = voidedByUserId;

            _uow.ComponentDerogations.Update(entity);
            await _uow.CompleteAsync();

            await RecomputeAffectedComponentsAsync(entity.ComponentTypeId);

            return (true, "Dérogation annulée.");
        }

        /// <summary>
        /// NEW — same pattern and same "fine at this data volume, revisit
        /// with a background job if the catalog grows very large" caveat as
        /// ComponentLifeLimitProfileService.RecomputeAffectedComponentsAsync.
        /// Recomputes every Component of this ComponentType, not just ones
        /// this specific derogation's applicability matches — determining
        /// that in advance would duplicate DerogationApplies' matching logic
        /// that already lives in the calculator; simplest to let
        /// RecomputeAsync itself decide per-component whether the derogation
        /// actually applies.
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
