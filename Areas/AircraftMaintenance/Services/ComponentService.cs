using System.Security.Claims;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services; // IUserScopeService — ASSUMPTION namespace, see ComponentScopeHelper.cs

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public interface IComponentService
    {
        Task<List<ComponentListDto>> GetScopedListAsync(ClaimsPrincipal user, bool includeInactive = false);
        Task<Component?> GetWithDetailsAsync(int id);
        Task<List<ComponentHistoryItemViewModel>> GetHistoryAsync(int componentId);

        Task<(bool Success, string Message, int? Id)> ReceiptAsync(ComponentReceiptDto dto, string performedByUserId);
        Task<(bool Success, string Message)> InstallAsync(ComponentInstallDto dto, string performedByUserId);
        Task<(bool Success, string Message)> RemoveAsync(ComponentRemoveDto dto, string performedByUserId);
        Task<(bool Success, string Message)> OverhaulAsync(ComponentOverhaulDto dto, string performedByUserId);
        Task<(bool Success, string Message)> ScrapAsync(ComponentScrapDto dto, string performedByUserId);

        /// <summary>NEW — attach a sub-assembly onto a parent Component's named slot (design doc §2).</summary>
        Task<(bool Success, string Message)> AttachToParentAsync(ComponentAttachToParentDto dto, string performedByUserId);
        /// <summary>NEW — detach a sub-assembly from its current parent Component.</summary>
        Task<(bool Success, string Message)> DetachFromParentAsync(ComponentDetachFromParentDto dto, string performedByUserId);
        /// <summary>NEW — per-slot readiness breakdown for a parent Component instance: for each slot defined on its ComponentType, which PNs are supported, how many are installed vs missing, and the actual PN/SN of each installed one. Empty list if the ComponentType defines no slots.</summary>
        Task<List<ComponentSlotStatusViewModel>> GetSlotStatusAsync(int componentId);

        Task<List<ComponentDueListItemViewModel>> GetDueOrOverdueAsync();
    }

    public class ComponentService : IComponentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IComponentLifeStatusCalculator _calculator;
        private readonly IAircraftReadingProvider _readings;
        private readonly IComponentScopeHelper _componentScope;
        private readonly IUserScopeService _userScope;

        public ComponentService(
            IUnitOfWork uow,
            IComponentLifeStatusCalculator calculator,
            IAircraftReadingProvider readings,
            IComponentScopeHelper componentScope,
            IUserScopeService userScope)
        {
            _uow = uow;
            _calculator = calculator;
            _readings = readings;
            _componentScope = componentScope;
            _userScope = userScope;
        }

        public async Task<List<ComponentListDto>> GetScopedListAsync(ClaimsPrincipal user, bool includeInactive = false)
        {
            var all = await _uow.Components.GetAllWithCurrentLocationAsync();
            var result = new List<ComponentListDto>();

            foreach (var c in all)
            {
                if (!includeInactive && !c.IsActive) continue;
                if (!await _componentScope.IsComponentInScopeAsync(user, c)) continue;

                result.Add(new ComponentListDto
                {
                    Id = c.Id,
                    PartNumber = c.ComponentType?.PartNumber ?? "",
                    Nomenclature = c.ComponentType?.Nomenclature ?? "",
                    SerialNumber = c.SerialNumber,
                    Status = c.Status,
                    LocationLabel = c.Status == ComponentStatus.Installed
                        ? $"{c.CurrentAircraft?.Registration} — {c.CurrentPosition?.Name}"
                        : c.StockBase?.BaseName,
                    LifeStatus = c.ComponentLifeStatus?.Status ?? ComponentLifeStatusValue.Unknown,
                    DrivingDimensionCode = c.ComponentLifeStatus?.DrivingDimensionType?.Code,
                    DrivingDimensionName = c.ComponentLifeStatus?.DrivingDimensionType?.Name,
                    DrivingDimensionUnit = c.ComponentLifeStatus?.DrivingDimensionType?.Unit,
                    DrivingDimensionRemainingDisplay = c.ComponentLifeStatus?.DrivingDimensionType != null
                        ? DimensionUnitConverter.ToDisplayValue(c.ComponentLifeStatus.DrivingDimensionType.Unit, c.ComponentLifeStatus.DrivingDimensionRemaining)
                        : null,
                    MissedOverhaulCount = c.ComponentLifeStatus?.MissedOverhaulCount ?? 0,
                    LifeLimitExceeded = c.ComponentLifeStatus?.LifeLimitExceeded ?? false,
                    HasActiveDerogation = c.ComponentLifeStatus?.HasActiveDerogation ?? false,
                    IsSubAssembly = c.ParentComponentId.HasValue,
                    ParentLabel = c.ParentComponentId.HasValue
                        ? $"{c.ParentComponent?.ComponentType?.PartNumber} — {c.ParentComponent?.SerialNumber}"
                        : null,
                    ChildCount = c.ChildComponents.Count
                });
            }

            return result.OrderBy(r => r.PartNumber).ThenBy(r => r.SerialNumber).ToList();
        }

        public Task<Component?> GetWithDetailsAsync(int id) => _uow.Components.GetWithDetailsAsync(id);

        /// <summary>
        /// NEW (Revision 13) — turns an IAircraftReadingProvider snapshot
        /// (Code -> value) into the ComponentEventReading rows for one
        /// ComponentEvent. CALENDAR_DAYS is skipped: it is never snapshotted
        /// per-event, see ComponentEvent.Readings' doc comment. A dimension
        /// with no resolvable Code in liveReadings (no source wired yet,
        /// e.g. TGO_LANDINGS) simply gets no row — same "null" meaning the
        /// old fixed nullable columns had.
        /// </summary>
        private static List<ComponentEventReading> BuildEventReadings(
            IReadOnlyDictionary<string, int> liveReadings,
            IEnumerable<ComponentLifeLimitDimensionType> dimensionTypes)
        {
            var list = new List<ComponentEventReading>();
            foreach (var d in dimensionTypes)
            {
                if (d.IsCalendarBased) continue;
                if (liveReadings.TryGetValue(d.Code, out var val))
                    list.Add(new ComponentEventReading { DimensionTypeId = d.Id, ValueAtEvent = val });
            }
            return list;
        }

        public async Task<List<ComponentHistoryItemViewModel>> GetHistoryAsync(int componentId)
        {
            var history = await _uow.ComponentEvents.GetHistoryAsync(componentId);
            return history.Select(e => new ComponentHistoryItemViewModel
            {
                EventType = e.EventType,
                EventDate = e.EventDate,
                AircraftLabel = e.Aircraft?.Registration,
                PositionLabel = e.Position?.Name,
                Readings = e.Readings
                    .Where(r => r.DimensionType != null)
                    .OrderBy(r => r.DimensionType!.SortOrder)
                    .Select(r => new ComponentEventReadingItemViewModel
                    {
                        DimensionCode = r.DimensionType!.Code,
                        DimensionName = r.DimensionType.Name,
                        Value = DimensionUnitConverter.ToDisplayValue(r.DimensionType.Unit, r.ValueAtEvent)
                    }).ToList(),
                RemovalReason = e.RemovalReason,
                LinkedWorkOrderNumber = e.LinkedWorkOrder?.WONumber,
                PerformedByUserName = e.PerformedByUser?.FullLabel ?? e.PerformedByUserId,
                Remarks = e.Remarks,
                RelatedParentComponentLabel = e.RelatedParentComponent != null
                    ? $"{e.RelatedParentComponent.ComponentType?.PartNumber} — {e.RelatedParentComponent.SerialNumber}"
                    : null
            }).ToList();
        }

        public async Task<(bool Success, string Message, int? Id)> ReceiptAsync(ComponentReceiptDto dto, string performedByUserId)
        {
            if (await _uow.Components.ExistsSerialAsync(dto.ComponentTypeId, dto.SerialNumber))
                return (false, "Ce numéro de série existe déjà pour ce numéro de pièce.", null);

            var component = new Component
            {
                ComponentTypeId = dto.ComponentTypeId,
                SerialNumber = dto.SerialNumber,
                ManufactureDate = dto.ManufactureDate,
                Status = ComponentStatus.InStock,
                StockBaseId = dto.StockBaseId,
                IsActive = true
            };
            _uow.Components.Add(component);
            await _uow.CompleteAsync(); // component.Id populated after this

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = component.Id,
                EventType = ComponentEventType.Receipt,
                EventDate = dto.EventDate,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Remarks
            });

            // NEW (Revision 12), generic per-dimension since Revision 13 —
            // only create an opening-reading row when the part actually
            // arrives with pre-existing usage; a genuinely new part (no
            // InitialValues rows with real content, no prior overhaul) gets
            // no row at all, so it costs nothing for the common case.
            var dimensionTypesById = (await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync()).ToDictionary(d => d.Id);

            var initialReading = new ComponentInitialReading
            {
                ComponentId = component.Id,
                // A prior overhaul date given without an explicit count is
                // treated as "at least the one we know about" — same
                // conservative-default reasoning as MissedOverhaulCount
                // elsewhere in this module (undercount, never overcount,
                // a real skipped overhaul).
                PriorOverhaulCount = dto.HasPriorOverhaul ? Math.Max(1, dto.PriorOverhaulCount ?? 1) : 0,
                PriorLastOverhaulDate = dto.HasPriorOverhaul ? dto.PriorLastOverhaulDate : null,
                Remarks = dto.Remarks,
                RecordedByUserId = performedByUserId
            };

            foreach (var v in dto.InitialValues)
            {
                // CALENDAR_DAYS deliberately never gets a row here — see
                // ComponentInitialReading's class doc comment. Guard against
                // a stray posted row for it (or an unknown Id) rather than
                // trusting the form exclusively.
                if (!dimensionTypesById.TryGetValue(v.DimensionTypeId, out var dimType) || dimType.IsCalendarBased) continue;

                var initialStored = DimensionUnitConverter.ToStoredValue(dimType.Unit, v.InitialValue) ?? 0;
                var priorSoStored = dto.HasPriorOverhaul ? DimensionUnitConverter.ToStoredValue(dimType.Unit, v.PriorSinceOverhaulValue) : null;

                if (initialStored == 0 && priorSoStored == null) continue; // nothing meaningful for this dimension

                initialReading.Values.Add(new ComponentInitialReadingValue
                {
                    DimensionTypeId = v.DimensionTypeId,
                    InitialValue = initialStored,
                    PriorSinceOverhaulValue = priorSoStored
                });
            }

            if (initialReading.Values.Any() || dto.HasPriorOverhaul)
            {
                _uow.ComponentInitialReadings.Add(initialReading);
            }

            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(component.Id);

            return (true, "Composant réceptionné avec succès.", component.Id);
        }

        public async Task<(bool Success, string Message)> InstallAsync(ComponentInstallDto dto, string performedByUserId)
        {
            var component = await _uow.Components.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return (false, "Composant introuvable.");

            if (component.Status != ComponentStatus.InStock && component.Status != ComponentStatus.UnderRepair)
                return (false, $"Impossible d'installer un composant au statut '{component.Status}'.");

            // NEW — a sub-assembly moves with its parent (design doc §2); it
            // cannot be independently installed onto an airframe position
            // while still attached. DetachFromParentAsync first.
            if (component.ParentComponentId.HasValue)
                return (false, "Ce composant est un sous-ensemble attaché à un composant parent — détachez-le d'abord.");

            var eligiblePositionIds = await _uow.ComponentTypes.GetPositionIdsAsync(component.ComponentTypeId);
            if (!eligiblePositionIds.Contains(dto.PositionId))
                return (false, "Cette position n'est pas éligible pour ce numéro de pièce.");

            var reading = await _readings.GetCurrentReadingsAsync(dto.AircraftId);
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();

            component.Status = ComponentStatus.Installed;
            component.CurrentAircraftId = dto.AircraftId;
            component.CurrentPositionId = dto.PositionId;
            component.StockBaseId = null;
            _uow.Components.Update(component);

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = component.Id,
                EventType = ComponentEventType.Install,
                EventDate = dto.EventDate,
                AircraftId = dto.AircraftId,
                PositionId = dto.PositionId,
                Readings = BuildEventReadings(reading, dimensionTypes),
                LinkedWorkOrderId = dto.LinkedWorkOrderId,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Remarks
            });
            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(component.Id);

            return (true, "Composant installé avec succès.");
        }

        public async Task<(bool Success, string Message)> RemoveAsync(ComponentRemoveDto dto, string performedByUserId)
        {
            if (dto.Destination != ComponentStatus.InStock && dto.Destination != ComponentStatus.UnderRepair)
                return (false, "La destination doit être 'En stock' ou 'En réparation'.");

            var component = await _uow.Components.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return (false, "Composant introuvable.");
            if (component.Status != ComponentStatus.Installed)
                return (false, "Ce composant n'est pas installé.");

            // NEW — a sub-assembly has no CurrentAircraftId of its own (see
            // Component.ParentComponentId doc comment) — Remove doesn't apply
            // to it, only DetachFromParentAsync does.
            if (component.ParentComponentId.HasValue)
                return (false, "Ce composant est un sous-ensemble attaché à un composant parent — utilisez « Détacher » plutôt que « Déposer ».");

            var aircraftId = component.CurrentAircraftId!.Value;
            var reading = await _readings.GetCurrentReadingsAsync(aircraftId);
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = component.Id,
                EventType = ComponentEventType.Remove,
                EventDate = dto.EventDate,
                AircraftId = aircraftId,
                PositionId = component.CurrentPositionId,
                Readings = BuildEventReadings(reading, dimensionTypes),
                RemovalReason = dto.RemovalReason,
                Destination = dto.Destination, // NEW — see ComponentEvent.Destination doc comment
                LinkedWorkOrderId = dto.LinkedWorkOrderId,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Remarks
            });

            component.Status = dto.Destination;
            component.CurrentAircraftId = null;
            component.CurrentPositionId = null;
            component.StockBaseId = dto.StockBaseId;
            _uow.Components.Update(component);
            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(component.Id);

            return (true, "Composant déposé avec succès.");
        }

        /// <summary>
        /// NEW — resolves the ultimate root of a Component's parent chain
        /// (itself, if it has no parent). Used to find "which aircraft is
        /// this whole assembly currently on" for snapshotting Attach/Detach
        /// events, same effective-aircraft concept ComponentLifeStatusCalculator
        /// uses for accrual.
        /// </summary>
        private async Task<Component> ResolveRootAsync(Component component)
        {
            if (!component.ParentComponentId.HasValue) return component;
            var ancestors = await _uow.Components.GetAncestorChainAsync(component.Id);
            return ancestors.Count > 0 ? ancestors[^1] : component;
        }

        public async Task<(bool Success, string Message)> AttachToParentAsync(ComponentAttachToParentDto dto, string performedByUserId)
        {
            if (dto.ComponentId == dto.ParentComponentId)
                return (false, "Un composant ne peut pas être son propre parent.");

            var child = await _uow.Components.GetWithDetailsAsync(dto.ComponentId);
            if (child == null) return (false, "Composant (sous-ensemble) introuvable.");

            var parent = await _uow.Components.GetWithDetailsAsync(dto.ParentComponentId);
            if (parent == null) return (false, "Composant parent introuvable.");

            if (child.Status != ComponentStatus.InStock && child.Status != ComponentStatus.UnderRepair)
                return (false, $"Impossible d'attacher un composant au statut '{child.Status}' — il doit être en stock ou en réparation.");

            if (parent.Status == ComponentStatus.Scrapped)
                return (false, "Le composant parent est réformé.");

            // Cycle guard: attaching would create a loop if the parent is
            // actually a descendant of the child somewhere up its own chain
            // (i.e. child would become its own ancestor). Should be
            // structurally impossible given the state checks above, but this
            // is cheap insurance — GetAncestorChainAsync also self-guards
            // against a corrupt cycle so this can never hang either way.
            var parentAncestors = await _uow.Components.GetAncestorChainAsync(parent.Id);
            if (parentAncestors.Any(a => a.Id == child.Id))
                return (false, "Cette attache créerait une boucle dans l'arborescence des composants.");

            var slotCode = dto.SlotCode.Trim().ToUpperInvariant();

            // NEW (normalized this revision) — capacity lives on the slot
            // DEFINITION now, not on each eligible-PN row (see ComponentTypeSlot
            // doc comment for why: two interchangeable PNs for the same
            // physical slot must never be able to disagree on how many of
            // that slot exist). GetBySlotCodeAsync resolves both the slot and
            // its eligible-PN list in one call.
            var slot = await _uow.ComponentTypeSlots.GetBySlotCodeAsync(parent.ComponentTypeId, slotCode);
            if (slot == null)
                return (false, $"L'emplacement '{slotCode}' n'existe pas sur ce type de composant parent.");

            var eligible = slot.EligibleChildren.Any(e => e.IsActive && e.ChildComponentTypeId == child.ComponentTypeId);
            if (!eligible)
                return (false, "Ce numéro de pièce n'est pas éligible pour cet emplacement sur ce composant parent.");

            var siblings = await _uow.Components.GetChildrenAsync(parent.Id);
            var occupied = siblings.Count(c => c.Id != child.Id && string.Equals(c.CurrentSlotCode, slotCode, StringComparison.OrdinalIgnoreCase));
            if (occupied >= slot.MaxCount)
                return (false, $"Emplacement '{slotCode}' complet ({occupied}/{slot.MaxCount}).");

            // Snapshot against whichever aircraft the whole assembly is
            // currently on (root of the PARENT's own chain — the parent may
            // itself be a sub-assembly several levels down).
            var root = await ResolveRootAsync(parent);
            var reading = root.Status == ComponentStatus.Installed && root.CurrentAircraftId.HasValue
                ? await _readings.GetCurrentReadingsAsync(root.CurrentAircraftId.Value)
                : new Dictionary<string, int>();
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();

            child.ParentComponentId = parent.Id;
            child.CurrentSlotCode = slotCode;
            child.Status = ComponentStatus.Installed; // reinterpreted for sub-assemblies: "attached to a parent"
            child.CurrentAircraftId = null;
            child.CurrentPositionId = null;
            child.StockBaseId = null;
            _uow.Components.Update(child);

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = child.Id,
                EventType = ComponentEventType.AttachToParent,
                EventDate = dto.EventDate,
                RelatedParentComponentId = parent.Id,
                SlotCode = slotCode,
                Readings = BuildEventReadings(reading, dimensionTypes),
                LinkedWorkOrderId = dto.LinkedWorkOrderId,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Remarks
            });
            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(child.Id);

            return (true, "Sous-ensemble attaché avec succès.");
        }

        public async Task<(bool Success, string Message)> DetachFromParentAsync(ComponentDetachFromParentDto dto, string performedByUserId)
        {
            var child = await _uow.Components.GetWithDetailsAsync(dto.ComponentId);
            if (child == null) return (false, "Composant introuvable.");
            if (!child.ParentComponentId.HasValue)
                return (false, "Ce composant n'est pas un sous-ensemble attaché.");

            // NEW — if this component itself hosts sub-assemblies (e.g. a DEEC
            // that somehow has its own children), those grandchildren move
            // with it automatically per design doc §2 ("If the engine is
            // pulled, all child components move with it automatically") — no
            // special handling needed here, they simply keep their own
            // ParentComponentId pointing at this component, unaffected by
            // this component's own detach.

            var parentId = child.ParentComponentId.Value;
            var root = await ResolveRootAsync(child); // resolved BEFORE detaching, while the chain is still intact
            var reading = root.Status == ComponentStatus.Installed && root.CurrentAircraftId.HasValue
                ? await _readings.GetCurrentReadingsAsync(root.CurrentAircraftId.Value)
                : new Dictionary<string, int>();
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = child.Id,
                EventType = ComponentEventType.DetachFromParent,
                EventDate = dto.EventDate,
                RelatedParentComponentId = parentId,
                SlotCode = child.CurrentSlotCode,
                Readings = BuildEventReadings(reading, dimensionTypes),
                RemovalReason = dto.RemovalReason,
                LinkedWorkOrderId = dto.LinkedWorkOrderId,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Remarks
            });

            // Detaching returns the sub-assembly to stock at the root
            // aircraft's Base, if resolvable — same "where does it land"
            // convention as Remove. Falls back to the parent's own StockBaseId
            // (covers the parent-in-stock case) or null if neither resolves;
            // a null StockBaseId leaves the part effectively unscoped until
            // manually corrected — flagged here rather than silently guessed.
            child.ParentComponentId = null;
            child.CurrentSlotCode = null;
            child.Status = ComponentStatus.InStock;
            child.StockBaseId = root.CurrentAircraft?.BaseId ?? root.StockBaseId;
            _uow.Components.Update(child);
            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(child.Id);

            return (true, "Sous-ensemble détaché avec succès.");
        }

        public async Task<List<ComponentSlotStatusViewModel>> GetSlotStatusAsync(int componentId)
        {
            var component = await _uow.Components.GetWithDetailsAsync(componentId);
            if (component == null) return new List<ComponentSlotStatusViewModel>();

            var slots = await _uow.ComponentTypeSlots.GetByParentComponentTypeAsync(component.ComponentTypeId);
            if (slots.Count == 0) return new List<ComponentSlotStatusViewModel>();

            var children = await _uow.Components.GetChildrenAsync(componentId);

            return slots.Select(slot =>
            {
                var installed = children
                    .Where(c => string.Equals(c.CurrentSlotCode, slot.SlotCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new ComponentSlotStatusViewModel
                {
                    SlotCode = slot.SlotCode,
                    SlotName = slot.SlotName,
                    MaxCount = slot.MaxCount,
                    SupportedPartNumbers = slot.EligibleChildren
                        .Where(e => e.IsActive)
                        .Select(e => $"{e.ChildComponentType?.PartNumber} — {e.ChildComponentType?.Nomenclature}")
                        .ToList(),
                    InstalledCount = installed.Count,
                    // Clamped at zero — an over-filled slot (more attached than
                    // MaxCount currently allows, e.g. after MaxCount was lowered
                    // post-attach) shows 0 missing rather than a negative number;
                    // the over-fill itself is visible from InstalledCount > MaxCount.
                    MissingCount = Math.Max(0, slot.MaxCount - installed.Count),
                    InstalledChildren = installed.Select(c => new ComponentChildViewModel
                    {
                        Id = c.Id,
                        PartNumber = c.ComponentType?.PartNumber ?? "",
                        Nomenclature = c.ComponentType?.Nomenclature ?? "",
                        SerialNumber = c.SerialNumber,
                        LifeStatus = c.ComponentLifeStatus?.Status ?? ComponentLifeStatusValue.Unknown,
                        LifeLimitExceeded = c.ComponentLifeStatus?.LifeLimitExceeded ?? false
                    }).ToList()
                };
            })
            .OrderBy(s => s.SlotCode)
            .ToList();
        }

        public async Task<(bool Success, string Message)> OverhaulAsync(ComponentOverhaulDto dto, string performedByUserId)
        {
            var component = await _uow.Components.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return (false, "Composant introuvable.");
            if (component.Status != ComponentStatus.UnderRepair)
                return (false, "Le composant doit être au statut 'En réparation' pour enregistrer une révision.");

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = component.Id,
                EventType = ComponentEventType.Overhaul,
                EventDate = dto.EventDate,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Remarks
            });

            // A completed overhaul returns the part to usable stock. Design
            // choice, not a hard MRO rule — adjust if your process needs an
            // explicit separate "return to stock" confirmation step.
            component.Status = ComponentStatus.InStock;
            _uow.Components.Update(component);
            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(component.Id);

            return (true, "Révision enregistrée — composant remis en stock.");
        }

        public async Task<(bool Success, string Message)> ScrapAsync(ComponentScrapDto dto, string performedByUserId)
        {
            var component = await _uow.Components.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return (false, "Composant introuvable.");
            if (component.Status == ComponentStatus.Installed)
                return (false, "Déposez le composant avant de le réformer.");
            if (component.Status == ComponentStatus.Scrapped)
                return (false, "Ce composant est déjà réformé.");

            _uow.ComponentEvents.Add(new ComponentEvent
            {
                ComponentId = component.Id,
                EventType = ComponentEventType.Scrap,
                EventDate = dto.EventDate,
                PerformedByUserId = performedByUserId,
                Remarks = dto.Reason
            });

            component.Status = ComponentStatus.Scrapped;
            component.IsActive = false;
            _uow.Components.Update(component);
            await _uow.CompleteAsync();

            await _calculator.RecomputeAsync(component.Id);

            return (true, "Composant réformé.");
        }

        public async Task<List<ComponentDueListItemViewModel>> GetDueOrOverdueAsync()
        {
            var due = await _uow.ComponentLifeStatuses.GetDueOrOverdueAsync();
            return due.Select(s => new ComponentDueListItemViewModel
            {
                ComponentId = s.ComponentId,
                PartNumber = s.Component?.ComponentType?.PartNumber ?? "",
                Nomenclature = s.Component?.ComponentType?.Nomenclature ?? "",
                SerialNumber = s.Component?.SerialNumber ?? "",
                AircraftRegistration = s.Component?.CurrentAircraft?.Registration,
                PositionLabel = s.Component?.CurrentPosition?.Name,
                Status = s.Status,
                DrivingDimensionCode = s.DrivingDimensionType?.Code,
                DrivingDimensionName = s.DrivingDimensionType?.Name,
                DrivingDimensionUnit = s.DrivingDimensionType?.Unit,
                DrivingDimensionRemainingDisplay = s.DrivingDimensionType != null
                    ? DimensionUnitConverter.ToDisplayValue(s.DrivingDimensionType.Unit, s.DrivingDimensionRemaining)
                    : null,
                MissedOverhaulCount = s.MissedOverhaulCount,
                LifeLimitExceeded = s.LifeLimitExceeded,
                HasActiveDerogation = s.HasActiveDerogation
            })
            // Missed overhauls / exceeded life limits are the most serious finding (real overstress risk,
            // not just "running late") — surface them above ordinary Overdue/Alert rows regardless of margin.
            .OrderBy(x => x.LifeLimitExceeded ? 0 : x.MissedOverhaulCount > 0 ? 1 : x.Status == ComponentLifeStatusValue.Overdue ? 2 : 3)
            .ThenByDescending(x => x.MissedOverhaulCount)
            .ToList();
        }
    }
}
