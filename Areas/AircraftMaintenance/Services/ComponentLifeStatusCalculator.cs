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
    ///
    /// NEW — Derogations now wired in (see ComponentDerogation.cs for the
    /// entity and the confirmed calculation model). Once a profile resolves
    /// and its checkpoint grid is built per dimension, every active
    /// (IsActive, not expired, applicability-matched) ComponentDerogation
    /// targeting that dimension's TargetStageType shifts the LAST checkpoint
    /// of that StageType by the derogation's signed amount — see
    /// EvaluateDimension's derogation block for the exact anchor rule and
    /// DerogationApplies for the S/N/lot matching. Unlike ResolveProfile
    /// (single winner), every applicable derogation stacks. A conditional
    /// derogation (IsConditional) is applied unconditionally by this pass —
    /// there is no automated tracking yet of whether its follow-up condition
    /// (e.g. a repeat inspection) is still being met; ConditionDescription
    /// remains informational only. Flagged as a known limitation, not a
    /// blocker: matches this revision's scope (record + apply the fact of
    /// the derogation, not police its conditions).
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
            // CHANGED — was a single FirstOrDefault(d => d.IsCalendarBased); a
            // second calendar-based dimension (Service Life, alongside the
            // original Shelf-Life-shaped CALENDAR_DAYS) needs its own
            // independently computed clock — see the calendarDims loop below.
            var calendarDims = allDimensionTypes.Where(d => d.IsCalendarBased).ToList();
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
            // NEW — SINCE_INSTALL (current install window only, resets every
            // Remove) and SINCE_FIRST_INSTALL (permanent, starts once) — see
            // ComponentReferenceBasis.cs for the exact semantics of all 4
            // basis codes this calculator now understands.
            var sinceInstall = new Dictionary<int, int>();
            var sinceFirstInstall = new Dictionary<int, int>();

            DateOnly? lastOverhaulDate = initial?.PriorLastOverhaulDate;

            foreach (var d in nonCalendarDims)
            {
                var openingVal = initialByDim.TryGetValue(d.Id, out var iv) ? iv.InitialValue : 0;
                cum[d.Id] = openingVal;
                // NEW — SINCE_INSTALL/SINCE_FIRST_INSTALL both start at 0
                // regardless of any opening/prior-usage balance: SINCE_INSTALL
                // because it only ever measures the current install stint, and
                // SINCE_FIRST_INSTALL because its whole point is to EXCLUDE
                // usage that predates this system's tracking (that's exactly
                // what SINCE_NEW/cum's openingVal already covers).
                sinceInstall[d.Id] = 0;
                sinceFirstInstall[d.Id] = 0;

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

            // NEW — the two extra start-date rules SINCE_INSTALL/
            // SINCE_FIRST_INSTALL need for calendar-based dimensions.
            // mostRecentInstallDate: EventDate of the latest Install/
            // AttachToParent — SINCE_INSTALL's calendar clock while
            // currently installed (0 while removed). firstInstallOrRepairDate:
            // EventDate of whichever came FIRST, ever — an Install/
            // AttachToParent, or a Remove with Destination = UnderRepair —
            // i.e. Dadda's Service Life trigger ("removed from stock
            // condition, either installed or removed to workshop"). Set
            // once, never overwritten — matches SINCE_FIRST_INSTALL's
            // "permanent" semantics.
            DateOnly? mostRecentInstallDate = null;
            DateOnly? firstInstallOrRepairDate = null;

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
                        {
                            open[d.Id] = readingsAtEvent.TryGetValue(d.Id, out var val) ? val : 0;
                            sinceInstall[d.Id] = 0; // NEW — defensive reset, see doc comment above the dictionary declaration
                        }
                        mostRecentInstallDate = e.EventDate; // NEW
                        firstInstallOrRepairDate ??= e.EventDate; // NEW
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
                                sinceInstall[d.Id] += delta;       // NEW — folds in this stint's usage before the reset below
                                sinceFirstInstall[d.Id] += delta;  // NEW — deltas only ever occur inside an open window, which can't exist before the first Install, so this naturally accrues nothing until then
                            }
                        }
                        foreach (var d in nonCalendarDims)
                        {
                            open[d.Id] = null;
                            sinceInstall[d.Id] = 0; // NEW — SINCE_INSTALL resets every Remove/DetachFromParent
                        }
                        // NEW — Service Life's calendar trigger: first Remove
                        // whose Destination is UnderRepair (a part sent to the
                        // workshop, never installed, still starts its Service
                        // Life clock). DetachFromParent never sets Destination
                        // (always returns to InStock — see DetachFromParentAsync),
                        // so this only ever fires for a real Remove event.
                        if (e.EventType == ComponentEventType.Remove && e.Destination == ComponentStatus.UnderRepair)
                            firstInstallOrRepairDate ??= e.EventDate;
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
                            sinceInstall[d.Id] += delta;      // NEW — still inside the current install window, so this keeps accruing right alongside cum/so
                            sinceFirstInstall[d.Id] += delta; // NEW
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

            // CHANGED — was a single "if (calendarDim != null)" against
            // whichever ONE calendar dimension FirstOrDefault happened to
            // pick. Every active calendar-based dimension now gets its own
            // 4 basis values computed independently (e.g. CALENDAR_DAYS/Shelf
            // Life stays SINCE_NEW-shaped; a second calendar dimension for
            // Service Life uses SINCE_FIRST_INSTALL — see
            // ComponentReferenceBasis.cs). currentlyOpen mirrors the
            // hours/count SINCE_INSTALL rule: 0 while removed, not just
            // "last known value".
            var currentlyOpen = open.Values.Any(v => v.HasValue);
            var sinceInstallDays = currentlyOpen && mostRecentInstallDate.HasValue
                ? Math.Max(0, today.DayNumber - mostRecentInstallDate.Value.DayNumber)
                : 0;
            var sinceFirstInstallDays = firstInstallOrRepairDate.HasValue
                ? Math.Max(0, today.DayNumber - firstInstallOrRepairDate.Value.DayNumber)
                : 0;

            foreach (var cd in calendarDims)
            {
                cum[cd.Id] = cumDays;
                so[cd.Id] = soDays;
                sinceInstall[cd.Id] = sinceInstallDays;
                sinceFirstInstall[cd.Id] = sinceFirstInstallDays;
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

            // NEW — per-dimension reference basis. A dimension with no
            // explicit ReferenceBasisId set on any of its stage rows falls
            // back to the profile-wide LifeBasis (useOverhaulBasis), i.e. the
            // exact behavior every existing profile already has — this
            // feature is additive, nothing already configured changes.
            var basisCodeByDim = ResolveBasisCodes(orderedStages);

            // NEW — Derogations affecting this component's checkpoints.
            // Fetched once per RecomputeAsync call (small per-ComponentType
            // dataset, same "read the small set, filter in C#" pattern as
            // ComponentEvents history), filtered to: not voided (IsActive
            // already applied at the query), not expired as of today, and
            // applicable to this specific S/N/lot (DerogationApplies — same
            // rule family as ResolveProfile, but every match stacks, there's
            // no single winner). Grouped by DimensionTypeId at the
            // EvaluateDimension call site below.
            var allDerogations = await _uow.ComponentDerogations.GetActiveByComponentTypeAsync(component.ComponentTypeId);
            var (derogPrefix, derogNumeric) = SplitSerial(component.SerialNumber);
            var applicableDerogations = allDerogations
                .Where(dg => !dg.EffectiveUntil.HasValue || dg.EffectiveUntil.Value >= today)
                .Where(dg => DerogationApplies(dg, component, derogPrefix, derogNumeric))
                .ToList();
            status.HasActiveDerogation = applicableDerogations.Count > 0;

            var results = new Dictionary<int, DimensionResult>();
            foreach (var d in allDimensionTypes)
            {
                int trackedValue;
                if (basisCodeByDim.TryGetValue(d.Id, out var basisCode))
                {
                    trackedValue = basisCode switch
                    {
                        "SINCE_OVERHAUL" => so.TryGetValue(d.Id, out var sv) ? sv : 0,
                        "SINCE_INSTALL" => sinceInstall.TryGetValue(d.Id, out var siv) ? siv : 0,
                        "SINCE_FIRST_INSTALL" => sinceFirstInstall.TryGetValue(d.Id, out var sfv) ? sfv : 0,
                        // "SINCE_NEW" and anything unrecognized (e.g. a future
                        // basis code this version doesn't know how to compute
                        // yet) both fall back to cum, same as no basis set.
                        _ => cum.TryGetValue(d.Id, out var cv) ? cv : 0,
                    };
                }
                else
                {
                    trackedValue = useOverhaulBasis
                        ? (so.TryGetValue(d.Id, out var sv2) ? sv2 : 0)
                        : (cum.TryGetValue(d.Id, out var cv2) ? cv2 : 0);
                }
                var derogationsForDim = applicableDerogations.Where(dg => dg.DimensionTypeId == d.Id).ToList();
                results[d.Id] = EvaluateDimension(orderedStages, d.Id, trackedValue, derogationsForDim);
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
                //
                // KNOWN LIMITATION (unchanged by the per-dimension reference
                // basis feature): this still reads the profile-wide
                // useOverhaulBasis, not each constrained dimension's own
                // resolved basis. For a profile that mixes bases (e.g. FH on
                // SINCE_OVERHAUL, a Service Life calendar dimension on
                // SINCE_FIRST_INSTALL) the missed-overhaul heuristic can
                // misfire for the non-SinceOverhaul dimensions. Left as-is
                // deliberately — it's a supplementary diagnostic counter, not
                // the primary Ok/Alert/Overdue determination above, and widening
                // it correctly needs its own pass once real mixed-basis profiles
                // exist to verify against.
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

        /// <summary>
        /// NEW — resolves ONE effective ComponentReferenceBasis.Code per
        /// DimensionTypeId across a profile's stages. ReferenceBasisId lives
        /// on the per-STAGE ComponentLifeLimitStageDimension row (not a
        /// single per-profile-per-dimension row — see that entity's doc
        /// comment for why), so the same dimension can in principle have a
        /// different basis set on different stages; this takes the FIRST
        /// non-null one found in SequenceOrder and ignores the rest. Keeping
        /// every stage's basis for the same dimension in agreement is an
        /// application-level rule (ComponentLifeLimitProfileService), not
        /// something this method enforces.
        /// </summary>
        private static Dictionary<int, string> ResolveBasisCodes(List<ComponentLifeLimitStage> orderedStages)
        {
            var result = new Dictionary<int, string>();
            foreach (var stage in orderedStages)
            {
                foreach (var sd in stage.Dimensions)
                {
                    if (sd.ReferenceBasis == null) continue;
                    if (!result.ContainsKey(sd.DimensionTypeId))
                        result[sd.DimensionTypeId] = sd.ReferenceBasis.Code;
                }
            }
            return result;
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
            int trackedValue,
            List<ComponentDerogation> derogations)
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

            // NEW — apply derogations. Each targets exactly one StageType
            // (Overhaul or Retirement — see ComponentDerogation.TargetStageType);
            // applied to the LAST (highest-value) checkpoint of that StageType
            // only, not every intermediate step inside it — e.g. an Overhaul
            // stage stepping 2000/4000/6000/8000 only has its final 8000
            // boundary moved by a derogation targeting Overhaul; the
            // intermediate checkpoints are untouched. This mirrors the
            // real-world case this feature was built for: a derogation grants
            // relief against ONE specific hard limit (the next overhaul
            // threshold, or the retirement limit), not a wholesale rescheduling
            // of every future checkpoint. PercentOfInterval is always a
            // percentage of THIS checkpoint's own un-derogated original value
            // (never of an already-derogated running value — Dadda-confirmed:
            // two stacked absolute extensions both add on top of the original).
            // Multiple derogations targeting the same StageType simply sum.
            if (derogations.Count > 0)
            {
                foreach (var stageType in checkpoints.Select(c => c.StageType).Distinct().ToList())
                {
                    var relevant = derogations.Where(d => d.TargetStageType == stageType).ToList();
                    if (relevant.Count == 0) continue;

                    var boundary = checkpoints
                        .Select((cp, idx) => (cp, idx))
                        .Where(x => x.cp.StageType == stageType)
                        .OrderByDescending(x => x.cp.Value)
                        .First();
                    var original = boundary.cp.Value;

                    var delta = 0m;
                    foreach (var d in relevant)
                    {
                        var sign = d.Direction == DerogationDirection.Extension ? 1 : -1;
                        var amount = d.Mode == ComponentToleranceType.PercentOfInterval
                            ? original * d.Value / 100m
                            : d.Value;
                        delta += sign * amount;
                    }

                    // Clamped at 0 — a Reduction large enough to zero out (or
                    // "invert") a limit is a real data/authority conflict that
                    // should be caught at the derogation-entry stage, not
                    // silently produce a negative checkpoint here.
                    var adjustedValue = Math.Max(0, original + (int)Math.Round(delta, MidpointRounding.AwayFromZero));
                    checkpoints[boundary.idx] = checkpoints[boundary.idx] with { Value = adjustedValue };
                }
            }

            // Robust to the checkpoint list no longer being strictly ascending
            // after a derogation adjustment (e.g. a large Reduction on the
            // Retirement boundary pulling it below an earlier Overhaul
            // checkpoint) — always resolved by value, never by list position.
            var next = checkpoints.Where(cp => cp.Value >= trackedValue).OrderBy(cp => cp.Value).FirstOrDefault()
                ?? checkpoints.OrderByDescending(cp => cp.Value).First();
            var remaining = next.Value - trackedValue;

            var overhaulCheckpointsCrossed = checkpoints.Count(cp => cp.StageType == ComponentLifeLimitStageType.Overhaul && cp.Value <= trackedValue);
            var retirementCrossed = checkpoints.Any(cp => cp.StageType == ComponentLifeLimitStageType.Retirement && cp.Value <= trackedValue);

            return new DimensionResult(true, remaining, next.Tolerance, next.StageSequence, next.StageType, overhaulCheckpointsCrossed, retirementCrossed);
        }

        /// <summary>
        /// NEW — mirrors ResolveProfile's SPECIFIC / RANGE / PN_BASED matching
        /// (plus the new LOT rule), but answers whether ONE derogation matches
        /// — unlike ResolveProfile there is no single winner, every match is
        /// summed by the caller.
        /// </summary>
        private static bool DerogationApplies(ComponentDerogation d, Component component, string prefix, int? numeric)
        {
            switch (d.ApplicabilityRuleType)
            {
                case ApplicabilityRuleType.Specific:
                    return string.Equals(d.SerialNumber, component.SerialNumber, StringComparison.OrdinalIgnoreCase);

                case ApplicabilityRuleType.RangeFrom:
                case ApplicabilityRuleType.RangeTo:
                    if (!numeric.HasValue) return false;
                    if (!string.IsNullOrEmpty(d.SerialNumberPrefix) &&
                        !string.Equals(d.SerialNumberPrefix, prefix, StringComparison.OrdinalIgnoreCase))
                        return false;
                    if (!int.TryParse(d.SerialBoundary, out var boundary)) return false;
                    return d.ApplicabilityRuleType == ApplicabilityRuleType.RangeFrom
                        ? numeric.Value >= boundary
                        : numeric.Value <= boundary;

                case ApplicabilityRuleType.Lot:
                    return !string.IsNullOrWhiteSpace(component.LotReference) &&
                           string.Equals(d.LotReference, component.LotReference, StringComparison.OrdinalIgnoreCase);

                default: // PnBased
                    return true;
            }
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
