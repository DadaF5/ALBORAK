using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    /// <summary>
    /// Single source of truth for component life/due math — same role as
    /// InspectionStatusCalculator plays for InspectionType, but a separate
    /// class: component life is a staged, per-checkpoint schedule that can be
    /// specific to one S/N, not an interval that just recurs the same way
    /// every time like an inspection.
    ///
    /// Algorithm: for whichever profile resolves for this Component (see
    /// ResolveProfile), each dimension's stages are walked into a FIXED
    /// checkpoint grid from zero (e.g. 3000, 4000, 5000, 6000, 6500...10000)
    /// — "next due" is just the smallest checkpoint >= the tracked value.
    /// SinceNew profiles use the since-new cumulative counter against this
    /// fixed grid; SinceOverhaul profiles use the since-overhaul counter
    /// (which already resets to 0 at each Overhaul event), so the same grid
    /// gets walked again from the top automatically after every overhaul —
    /// no special "restart" logic needed. Flagged design choice: SinceNew
    /// checkpoints are absolute/fixed, not floating from whenever the
    /// previous overhaul actually happened — confirm this matches real parts.
    ///
    /// Revision 13: rewritten from 5 hardcoded named counters (FH/Cycles/
    /// CalendarDays/TgoLandings/FullStopLandings) to a generic loop over
    /// every active ComponentLifeLimitDimensionType. Adding a future
    /// aircraft-specific counter (C130 APU starts, Canadair "number of
    /// Drops") is now a seeded lookup row plus, when a live/cumulative
    /// source for it becomes available, a small addition to
    /// IAircraftReadingProvider — this class itself needs NO changes.
    /// CALENDAR_DAYS remains the one dimension special-cased by Code: it is
    /// computed from dates (ManufactureDate / LastOverhaulDate vs today),
    /// never from ComponentEvent readings or IAircraftReadingProvider.
    /// </summary>
    public interface IComponentLifeStatusCalculator
    {
        /// <summary>Recomputes and persists ComponentLifeStatus for one component. Call after every ComponentEvent is recorded.</summary>
        Task<ComponentLifeStatus> RecomputeAsync(int componentId);
    }

    public class ComponentLifeStatusCalculator : IComponentLifeStatusCalculator
    {
        private readonly IUnitOfWork _uow;
        private readonly IAircraftReadingProvider _readings;

        public ComponentLifeStatusCalculator(
            IUnitOfWork uow,
            IAircraftReadingProvider readings)
        {
            _uow = uow;
            _readings = readings;
        }

        private record Checkpoint(int Value, int StageSequence, ComponentLifeLimitStageType StageType, int? Tolerance);

        /// <summary>
        /// OverhaulCheckpointsCrossed / RetirementCrossed exist to catch a real
        /// failure mode: "next checkpoint >= trackedValue" alone would silently
        /// hide a component that blew past TWO checkpoints without ever being
        /// overhauled in between (it would just report status against whichever
        /// checkpoint is next, which can look perfectly fine even though a
        /// mandatory overhaul was skipped). See RecomputeAsync's missedOverhaulCount.
        /// </summary>
        private record DimensionResult(bool IsConstrained, int? Remaining, int? Tolerance, int StageSequence, ComponentLifeLimitStageType StageType, int OverhaulCheckpointsCrossed, bool RetirementCrossed);

        public async Task<ComponentLifeStatus> RecomputeAsync(int componentId)
        {
            var component = await _uow.Components.GetWithDetailsAsync(componentId)
                ?? throw new InvalidOperationException($"Component {componentId} not found.");

            var history = await _uow.ComponentEvents.GetHistoryAsync(componentId); // ordered by EventDate, Id

            var allDimensionTypes = (await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync())
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder)
                .ToList();
            var calendarDim = allDimensionTypes.FirstOrDefault(d => d.IsCalendarBased);
            var nonCalendarDims = allDimensionTypes.Where(d => !d.IsCalendarBased).ToList();

            // NEW (Revision 12) — seed from ComponentInitialReading instead of
            // always starting at zero, so a component received with
            // pre-existing usage (used/serviceable transfer-in) doesn't
            // silently report as brand-new until its next install. No row for
            // a dimension (the common case, a genuinely new part / a
            // dimension this Component's PN never tracked) behaves exactly
            // as before: that dimension starts at 0.
            var initial = component.InitialReading;
            var initialByDim = initial?.Values.ToDictionary(v => v.DimensionTypeId, v => v)
                ?? new Dictionary<int, ComponentInitialReadingValue>();

            var cum = new Dictionary<int, int>();
            var so = new Dictionary<int, int>();

            DateOnly? lastOverhaulDate = initial?.PriorLastOverhaulDate;

            foreach (var d in nonCalendarDims)
            {
                var openingVal = initialByDim.TryGetValue(d.Id, out var iv) ? iv.InitialValue : 0;
                cum[d.Id] = openingVal;

                // Since-overhaul seed: if a prior overhaul is on record, start
                // from ITS baseline (may be less than the since-new opening
                // value); otherwise mirror the existing "since-overhaul equals
                // since-new until the first Overhaul event" rule, just applied
                // to the opening balance instead of zero.
                if (lastOverhaulDate != null)
                {
                    so[d.Id] = initialByDim.TryGetValue(d.Id, out var sov) ? (sov.PriorSinceOverhaulValue ?? 0) : 0;
                }
                else
                {
                    so[d.Id] = openingVal;
                }
            }

            var open = nonCalendarDims.ToDictionary(d => d.Id, d => (int?)null);
            DateOnly? earliestDate = component.ManufactureDate;
            bool readingUnavailableForOpenInstall = false;
            // Seeded with overhauls the part genuinely had BEFORE entering this
            // system (told to us at Receipt), so MissedOverhaulCount below
            // doesn't flag those as skipped — only overhauls missed since.
            var actualOverhaulEventCount = initial?.PriorOverhaulCount ?? 0;

            foreach (var e in history)
            {
                earliestDate ??= e.EventDate;
                if (e.EventDate < earliestDate) earliestDate = e.EventDate;

                var readingsAtEvent = e.Readings.ToDictionary(r => r.DimensionTypeId, r => r.ValueAtEvent);

                switch (e.EventType)
                {
                    // AttachToParent is treated exactly like Install for accrual-window
                    // purposes: the sub-assembly starts accruing against whatever
                    // aircraft was resolved (root's CurrentAircraftId) at attach time —
                    // ComponentService stamps the same per-dimension Readings snapshot
                    // on both event types for this reason (see AttachToParentAsync).
                    // Same convention for DetachFromParent/Remove below.
                    case ComponentEventType.Install:
                    case ComponentEventType.AttachToParent:
                        foreach (var d in nonCalendarDims)
                            open[d.Id] = readingsAtEvent.TryGetValue(d.Id, out var val) ? val : 0;
                        break;

                    case ComponentEventType.Remove:
                    case ComponentEventType.DetachFromParent:
                        if (open.Values.Any(v => v.HasValue))
                        {
                            foreach (var d in nonCalendarDims)
                            {
                                var openVal = open[d.Id] ?? 0;
                                var closeVal = readingsAtEvent.TryGetValue(d.Id, out var val) ? val : openVal;
                                var delta = Math.Max(0, closeVal - openVal);
                                cum[d.Id] += delta;
                                so[d.Id] += delta;
                            }
                        }
                        foreach (var d in nonCalendarDims) open[d.Id] = null;
                        break;

                    case ComponentEventType.Overhaul:
                        foreach (var d in nonCalendarDims) so[d.Id] = 0;
                        lastOverhaulDate = e.EventDate;
                        actualOverhaulEventCount++;
                        foreach (var d in nonCalendarDims) open[d.Id] = null; // defensive, see original comment
                        break;

                    case ComponentEventType.Receipt:
                    case ComponentEventType.TransferToStock:
                    case ComponentEventType.Scrap:
                        break;
                }
            }

            // Status = Installed is reinterpreted for sub-assemblies (design doc
            // §2): a component with ParentComponentId set has its own
            // CurrentAircraftId left null, so the live/open accrual window must
            // resolve the EFFECTIVE aircraft by walking the parent chain to its
            // root, instead of reading CurrentAircraftId directly. Root-level
            // components (no ParentComponentId) behave exactly as before.
            if (component.Status == ComponentStatus.Installed)
            {
                var effectiveAircraftId = await ResolveEffectiveAircraftIdAsync(component);
                var wasOpen = open.Values.Any(v => v.HasValue);

                if (wasOpen && effectiveAircraftId.HasValue)
                {
                    var liveReadings = await _readings.GetCurrentReadingsAsync(effectiveAircraftId.Value);

                    // FH and Cycles are the two dimensions every installed
                    // component's live accrual has always depended on (same
                    // gate the pre-Revision-13 code used: reading.FHMinutes
                    // and reading.Cycles both had to resolve). Any other
                    // dimension degrades gracefully below — it simply stays
                    // at its last known value until a real source exists.
                    var fhOk = nonCalendarDims.Any(d => d.Code == "FH") && liveReadings.ContainsKey("FH");
                    var cyclesOk = nonCalendarDims.Any(d => d.Code == "CYCLES") && liveReadings.ContainsKey("CYCLES");

                    if (fhOk && cyclesOk)
                    {
                        foreach (var d in nonCalendarDims)
                        {
                            if (!liveReadings.TryGetValue(d.Code, out var liveVal)) continue; // not (yet) available for this dimension — stays at last known value
                            var openVal = open[d.Id] ?? 0;
                            var delta = Math.Max(0, liveVal - openVal);
                            cum[d.Id] += delta;
                            so[d.Id] += delta;
                        }
                    }
                    else
                    {
                        readingUnavailableForOpenInstall = true;
                    }
                }
                else
                {
                    // Either no open Install/AttachToParent event found (data
                    // inconsistency), or a sub-assembly whose root couldn't be
                    // resolved to an installed aircraft (e.g. root itself is
                    // in stock/under repair) — either way, usage since the last
                    // event can't be computed right now.
                    readingUnavailableForOpenInstall = true;
                }
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var sinceNewBase = earliestDate ?? today;
            var cumDays = Math.Max(0, today.DayNumber - sinceNewBase.DayNumber);
            var soBase = lastOverhaulDate ?? sinceNewBase;
            var soDays = Math.Max(0, today.DayNumber - soBase.DayNumber);

            if (calendarDim != null)
            {
                cum[calendarDim.Id] = cumDays;
                so[calendarDim.Id] = soDays;
            }

            var status = new ComponentLifeStatus
            {
                ComponentId = componentId,
                LastOverhaulDate = lastOverhaulDate,
            };

            foreach (var d in allDimensionTypes)
            {
                status.Dimensions.Add(new ComponentLifeStatusDimension
                {
                    DimensionTypeId = d.Id,
                    Cumulative = cum.TryGetValue(d.Id, out var cv) ? cv : 0,
                    SinceOverhaul = so.TryGetValue(d.Id, out var sv) ? sv : 0,
                    Remaining = null,
                });
            }

            if (component.ComponentType?.TrackingMethod != ComponentTrackingMethod.HardTime)
            {
                status.Status = ComponentLifeStatusValue.NotLifeLimited;
                await _uow.ComponentLifeStatuses.UpsertAsync(status);
                await _uow.CompleteAsync();
                return status;
            }

            var profile = ResolveProfile(component, component.ComponentType!.LifeLimitProfiles);
            if (profile == null)
            {
                status.Status = ComponentLifeStatusValue.Unknown; // HardTime type, no profile matches this S/N
                await _uow.ComponentLifeStatuses.UpsertAsync(status);
                await _uow.CompleteAsync();
                return status;
            }

            status.MatchedLifeLimitProfileId = profile.Id;

            if (readingUnavailableForOpenInstall)
            {
                status.Status = ComponentLifeStatusValue.Unknown;
                await _uow.ComponentLifeStatuses.UpsertAsync(status);
                await _uow.CompleteAsync();
                return status;
            }

            var useOverhaulBasis = profile.LifeBasis == ComponentLifeBasis.SinceOverhaul;
            var orderedStages = profile.Stages.OrderBy(s => s.SequenceOrder).ToList();

            var results = new Dictionary<int, DimensionResult>();
            foreach (var d in allDimensionTypes)
            {
                var trackedValue = useOverhaulBasis
                    ? (so.TryGetValue(d.Id, out var sv) ? sv : 0)
                    : (cum.TryGetValue(d.Id, out var cv) ? cv : 0);
                results[d.Id] = EvaluateDimension(orderedStages, d.Id, trackedValue);
            }

            foreach (var entry in status.Dimensions)
            {
                var r = results[entry.DimensionTypeId];
                entry.Remaining = r.IsConstrained ? r.Remaining : null;
            }

            var constrained = results.Where(kv => kv.Value.IsConstrained).ToList();

            if (constrained.Count == 0)
            {
                status.Status = ComponentLifeStatusValue.Unknown; // profile matched but has no usable stage/dimension data
            }
            else
            {
                var worst = ComponentLifeStatusValue.Ok;
                DimensionResult? driving = null;
                int? drivingDimensionTypeId = null;

                foreach (var kv in constrained)
                {
                    var r = kv.Value;
                    var dimStatus = r.Remaining!.Value <= 0
                        ? ComponentLifeStatusValue.Overdue
                        : (r.Tolerance.HasValue && r.Remaining.Value <= r.Tolerance.Value ? ComponentLifeStatusValue.Alert : ComponentLifeStatusValue.Ok);

                    if (dimStatus > worst || driving == null)
                    {
                        worst = dimStatus;
                        driving = r;
                        drivingDimensionTypeId = kv.Key;
                    }
                }

                // Catches a component that sailed past more than one mandatory
                // overhaul checkpoint without an Overhaul event recorded in
                // between — the "next checkpoint" proximity check above can look
                // perfectly fine (plenty of remaining margin to whatever
                // checkpoint is next) while actually having already skipped one
                // or more earlier ones entirely. SinceNew: compare checkpoints
                // crossed over the component's whole life against overhauls
                // actually recorded. SinceOverhaul: the since-overhaul counter is
                // 0 by definition right after a reset, so being 1 checkpoint deep
                // into it is the normal "currently overdue on the next one" case
                // — only 2+ crossings in the same un-reset stretch means a whole
                // cycle was skipped.
                var maxCheckpointsCrossed = constrained.Max(kv => kv.Value.OverhaulCheckpointsCrossed);
                var missedOverhaulCount = useOverhaulBasis
                    ? Math.Max(0, maxCheckpointsCrossed - 1)
                    : Math.Max(0, maxCheckpointsCrossed - actualOverhaulEventCount);

                status.MissedOverhaulCount = missedOverhaulCount;
                status.LifeLimitExceeded = constrained.Any(kv => kv.Value.RetirementCrossed);

                if (missedOverhaulCount > 0 && worst < ComponentLifeStatusValue.Overdue)
                    worst = ComponentLifeStatusValue.Overdue;

                status.Status = worst;
                status.CurrentStageSequence = driving?.StageSequence;
                status.DrivingDimensionTypeId = drivingDimensionTypeId;
                status.DrivingDimensionRemaining = driving?.Remaining;
                status.DrivingDimensionTolerance = driving?.Tolerance;
            }

            await _uow.ComponentLifeStatuses.UpsertAsync(status);
            await _uow.CompleteAsync();
            return status;
        }

        /// <summary>
        /// NEW — resolves the aircraft a Component's usage should accrue
        /// against right now. Root component (no parent): its own
        /// CurrentAircraftId, unchanged from before the hierarchy feature.
        /// Sub-assembly (ParentComponentId set): walk the ancestor chain to
        /// the ultimate root and use ITS CurrentAircraftId — but only if that
        /// root is itself currently Installed; a root sitting in stock/under
        /// repair means every attached child is also, in effect, not on an
        /// aircraft right now (even though the child's own Status still reads
        /// Installed under the "attached to a parent" reinterpretation).
        /// </summary>
        private async Task<int?> ResolveEffectiveAircraftIdAsync(Component component)
        {
            if (component.CurrentAircraftId.HasValue) return component.CurrentAircraftId;
            if (!component.ParentComponentId.HasValue) return null;

            var ancestors = await _uow.Components.GetAncestorChainAsync(component.Id);
            var root = ancestors.Count > 0 ? ancestors[^1] : null;
            return root?.Status == ComponentStatus.Installed && root.CurrentAircraftId.HasValue
                ? root.CurrentAircraftId
                : null;
        }

        /// <summary>
        /// SPECIFIC > RANGE > PN_BASED, mirroring JobCardApplicability's
        /// resolution priority. Numeric range comparison is approximate — this
        /// project's real JobCardApplicability matching logic wasn't available
        /// to copy exactly; verify against that file if serial numbers here
        /// don't parse the same way (e.g. mixed alpha/numeric suffixes).
        /// </summary>
        private static ComponentLifeLimitProfile? ResolveProfile(Component component, IEnumerable<ComponentLifeLimitProfile> profiles)
        {
            var active = profiles.Where(p => p.IsActive).ToList();

            var specific = active.FirstOrDefault(p =>
                p.ApplicabilityRuleType == ApplicabilityRuleType.Specific &&
                string.Equals(p.SerialNumber, component.SerialNumber, StringComparison.OrdinalIgnoreCase));
            if (specific != null) return specific;

            var (prefix, numeric) = SplitSerial(component.SerialNumber);
            if (numeric.HasValue)
            {
                var rangeMatch = active.FirstOrDefault(p =>
                {
                    if (p.ApplicabilityRuleType != ApplicabilityRuleType.RangeFrom && p.ApplicabilityRuleType != ApplicabilityRuleType.RangeTo)
                        return false;
                    if (!string.IsNullOrEmpty(p.SerialNumberPrefix) &&
                        !string.Equals(p.SerialNumberPrefix, prefix, StringComparison.OrdinalIgnoreCase))
                        return false;
                    if (!int.TryParse(p.SerialBoundary, out var boundary)) return false;

                    return p.ApplicabilityRuleType == ApplicabilityRuleType.RangeFrom
                        ? numeric.Value >= boundary
                        : numeric.Value <= boundary;
                });
                if (rangeMatch != null) return rangeMatch;
            }

            return active.FirstOrDefault(p => p.ApplicabilityRuleType == ApplicabilityRuleType.PnBased);
        }

        private static (string Prefix, int? Numeric) SplitSerial(string serialNumber)
        {
            var digitsStart = serialNumber.Length;
            for (var i = 0; i < serialNumber.Length; i++)
            {
                if (char.IsDigit(serialNumber[i])) { digitsStart = i; break; }
            }
            var prefix = serialNumber[..digitsStart].Trim();
            var digits = serialNumber[digitsStart..].Trim();
            return int.TryParse(digits, out var n) ? (prefix, n) : (prefix, null);
        }

        private static DimensionResult EvaluateDimension(
            List<ComponentLifeLimitStage> orderedStages,
            int dimensionTypeId,
            int trackedValue)
        {
            var checkpoints = new List<Checkpoint>();
            var running = 0;

            foreach (var stage in orderedStages)
            {
                var dim = stage.Dimensions.FirstOrDefault(sd => sd.DimensionTypeId == dimensionTypeId);
                if (dim == null) continue;

                var iv = dim.Interval;
                var end = dim.BandEnd;
                if (iv is null || iv <= 0 || end is null) continue;

                while (running + iv.Value <= end.Value)
                {
                    running += iv.Value;
                    checkpoints.Add(new Checkpoint(running, stage.SequenceOrder, stage.StageType, ResolveTolerance(stage, iv.Value, dim.Tolerance)));
                }
                if (running < end.Value)
                {
                    running = end.Value;
                    checkpoints.Add(new Checkpoint(running, stage.SequenceOrder, stage.StageType, ResolveTolerance(stage, iv.Value, dim.Tolerance)));
                }
            }

            if (checkpoints.Count == 0)
                return new DimensionResult(false, null, null, 0, ComponentLifeLimitStageType.Overhaul, 0, false);

            var next = checkpoints.FirstOrDefault(cp => cp.Value >= trackedValue) ?? checkpoints[^1];
            var remaining = next.Value - trackedValue;

            var overhaulCheckpointsCrossed = checkpoints.Count(cp => cp.StageType == ComponentLifeLimitStageType.Overhaul && cp.Value <= trackedValue);
            var retirementCrossed = checkpoints.Any(cp => cp.StageType == ComponentLifeLimitStageType.Retirement && cp.Value <= trackedValue);

            return new DimensionResult(true, remaining, next.Tolerance, next.StageSequence, next.StageType, overhaulCheckpointsCrossed, retirementCrossed);
        }

        private static int? ResolveTolerance(ComponentLifeLimitStage stage, int interval, int? tolerance)
        {
            if (!tolerance.HasValue) return null;
            return stage.ToleranceType == ComponentToleranceType.PercentOfInterval
                ? (int)Math.Round(interval * tolerance.Value / 100.0)
                : tolerance.Value;
        }
    }
}
