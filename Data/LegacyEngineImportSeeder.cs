using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Data.Seeders
{
    /// <summary>
    /// ONE-TIME IMPORT — NOT a startup seeder. Do NOT add a permanent call to
    /// this from Program.cs's regular seeding block; wire it up temporarily
    /// (see the bottom of this file for the exact snippet), run the app once,
    /// then remove the call. Unlike the other seeders in this folder this one
    /// is importing real physical inventory, not catalog/lookup rows.
    ///
    /// Source: Dadda's real query against the legacy DB —
    ///   SELECT EngineId, EngineSN, Eng_Type, AcMainGroupID, Serviceable, AcMainGroup
    ///   FROM tblMeca_Engines eng INNER JOIN tblAcMainGroup gp ON gp.AcMainGroupID = eng.AcMainGroupID
    ///   WHERE eng.AcMainGroupID IN (1,2) AND eng.Active='True'
    /// — legacy AcMainGroupID 1 = F5, 2 = A-Jet. 66 rows, transcribed verbatim
    /// below (Rows array) — nothing invented, nothing dropped.
    ///
    /// Confirmed design decisions (asked and answered before writing this):
    ///   - StockBaseId: ALL 66 engines go to the real Base with Id=1 (Dadda
    ///     confirmed the real Id directly, no Code lookup needed — see
    ///     TargetBaseId below).
    ///   - AircraftManufacturerId (per Dadda: "case when gp.AcMainGroupID = 1
    ///     then 4 when gp.AcMainGroupID = 2 then 1002"): F5 engines (Type B
    ///     and Type C) get AircraftManufacturerId=4, the A-Jet engine gets
    ///     AircraftManufacturerId=1002 — stamped on the ComponentType, not
    ///     per-Component (AircraftManufacturerId lives on ComponentType).
    ///   - Serviceable=1 -&gt; Status=InStock, Serviceable=0 -&gt; Status=UnderRepair.
    ///   - Stock-only import — no Install step. This query has no aircraft/tail
    ///     column at all, so none of these 66 rows are linked to a real
    ///     aircraft/position here. If some are actually mounted today, that's a
    ///     separate follow-up once you have the real engine-to-aircraft
    ///     assignment data (a different legacy table/join).
    ///   - Eligible positions: each engine ComponentType is linked to every
    ///     ENG*-coded ComponentPosition on every AcType in its family (broadest
    ///     safe default — this query only has AcMainGroupID, not AcTypeID, so
    ///     there's no finer-grained data to work from).
    ///
    /// NOT covered by this import, flagged loudly because it matters for a
    /// life-limited part: **no FH/Cycles/hours data exists in this dataset at
    /// all.** Every engine is receipted with an EMPTY InitialValues list, i.e.
    /// it starts accruing from ZERO. These are real in-service legacy engines
    /// with real accumulated hours — until a second import backfills each
    /// engine's real opening FH/Cycles (same shape as Receipt's "Pièce usagée"
    /// table), any due/overdue calculation for these 66 engines will be wrong.
    /// Do not treat this import as "engines are now correctly tracked" — only
    /// "engines now exist as real Component rows, ready to receive their real
    /// opening readings." The legacy EngineId is stashed in each Receipt
    /// event's Remarks specifically so that follow-up backfill can match rows
    /// precisely.
    /// </summary>
    public static class LegacyEngineImportSeeder
    {
        // ================= FILL IN BEFORE RUNNING =================

        /// <summary>CONFIRMED by Dadda — the real Base.Id these 66 engines are stocked at.</summary>
        private const int TargetBaseId = 1;

        /// <summary>TODO — Dadda: set to YOUR real AspNetUsers.Id (ComponentEvent.PerformedByUserId is a required FK — cannot be a made-up string). Still the only unresolved value.</summary>
        private const string PerformedByUserId = "TODO_FILL_IN";

        // =============================================================

        /// <summary>
        /// PartNumber/Nomenclature are SYNTHESIZED, not given verbatim by the
        /// legacy data (tblMeca_Engines only has "Eng_Type" = "TYPE B"/"TYPE C"/
        /// NULL, no real manufacturer PN string). Edit these two constants
        /// first if you have the real PN — PartNumber is meant to stay stable
        /// once components reference it, same caution as everywhere else in
        /// this module.
        /// </summary>
        private const string F5TypeBPartNumber = "F5-ENG-TYPEB";
        private const string F5TypeBNomenclature = "Moteur F5 — Type B";
        private const string F5TypeCPartNumber = "F5-ENG-TYPEC";
        private const string F5TypeCNomenclature = "Moteur F5 — Type C";
        private const string AJetEnginePartNumber = "AJET-ENG";
        private const string AJetEngineNomenclature = "Moteur A-Jet";

        /// <summary>
        /// CONFIRMED by Dadda: "case when gp.AcMainGroupID = 1 then 4 when
        /// gp.AcMainGroupID = 2 then 1002 end as [AircraftManufacturers.Id]"
        /// — F5 (both engine types) -&gt; 4, A-Jet -&gt; 1002. Stamped on the
        /// ComponentType at creation only (see GetOrCreateComponentTypeAsync)
        /// — an existing row from a prior run is reused untouched, same
        /// don't-clobber-a-manual-edit rule as position-linking below.
        /// </summary>
        private const int F5AircraftManufacturerId = 4;
        private const int AJetAircraftManufacturerId = 1002;

        /// <summary>Transcribed verbatim from Dadda's legacy query result — 66 rows, EngineId 2/24/28/29/38/43/48/49/56/58/60 genuinely absent from the source (not a transcription gap).</summary>
        private static readonly (int EngineId, string SerialNumber, string? EngType, int LegacyAcMainGroupId, bool Serviceable)[] Rows = new[]
        {
            (1, "225022", "TYPE B", 1, false), (3, "225010", "TYPE B", 1, false),
            (4, "227747", "TYPE C", 1, false), (5, "226121", "TYPE B", 1, true),
            (6, "227776", "TYPE B", 1, true),  (7, "227820", "TYPE B", 1, false),
            (8, "227744", "TYPE B", 1, false), (9, "226237", "TYPE C", 1, true),
            (10, "227821", "TYPE C", 1, true), (11, "227548", "TYPE C", 1, true),
            (12, "227900", "TYPE C", 1, false),(13, "227545", "TYPE C", 1, true),
            (14, "227731", "TYPE C", 1, true), (15, "227638", "TYPE C", 1, true),
            (16, "227723", "TYPE C", 1, false),(17, "227782", "TYPE B", 1, true),
            (18, "227896", "TYPE C", 1, false),(19, "227911", "TYPE C", 1, false),
            (20, "227784", "TYPE C", 1, false),(21, "227732", "TYPE B", 1, true),
            (22, "227842", "TYPE C", 1, true),
            (23, "41476", null, 2, true), (25, "41299", null, 2, true),
            (26, "41411", null, 2, true), (27, "41614", null, 2, true),
            (30, "41344", null, 2, true), (31, "41678", null, 2, true),
            (32, "41341", null, 2, true), (33, "41156", null, 2, true),
            (34, "41418", null, 2, true), (35, "41280", null, 2, true),
            (36, "41405", null, 2, true), (37, "41651", null, 2, true),
            (39, "41512", null, 2, true), (40, "41604", null, 2, true),
            (41, "41720", null, 2, true), (42, "41641", null, 2, true),
            (44, "41993", null, 2, true), (45, "41472", null, 2, true),
            (46, "41506", null, 2, true),
            (47, "226378", "TYPE B", 1, false), (50, "227488", "TYPE B", 1, false),
            (51, "227761", "TYPE B", 1, false), (52, "225174", "TYPE B", 1, false),
            (53, "225073", "TYPE B", 1, false), (54, "225416", "TYPE B", 1, false),
            (55, "226256", "TYPE B", 1, false),
            (57, "41475", null, 2, true), (59, "41603", null, 2, true),
            (61, "227569", "TYPE B", 1, false), (62, "227552", "TYPE B", 1, false),
            (63, "227626", "TYPE B", 1, false), (64, "227748", "TYPE B", 1, false),
            (65, "227551", "TYPE B", 1, false), (66, "227603", "TYPE B", 1, false),
            (67, "227214", "TYPE B", 1, false), (68, "227088", "TYPE B", 1, false),
            (69, "227001", "TYPE B", 1, false), (70, "227026", "TYPE B", 1, false),
            (71, "227904", "TYPE B", 1, false), (72, "227104", "TYPE B", 1, false),
            (73, "227929", "TYPE B", 1, false), (74, "227745", "TYPE B", 1, false),
            (75, "227738", "TYPE B", 1, false), (76, "227209", "TYPE B", 1, false),
            (77, "227130", "TYPE B", 1, false),
        };

        public static async Task<List<string>> SeedAsync(IUnitOfWork uow, IComponentService componentService)
        {
            var log = new List<string>();

            if (PerformedByUserId == "TODO_FILL_IN")
            {
                log.Add("ABORTED — PerformedByUserId still says TODO_FILL_IN at the top of LegacyEngineImportSeeder.cs. Fill it in with your real AspNetUsers.Id first.");
                return log;
            }

            var targetBase = await uow.Bases.GetByIdAsync(TargetBaseId);
            if (targetBase == null)
            {
                log.Add($"ABORTED — no Base found with Id={TargetBaseId}. Check Réglages and fix TargetBaseId.");
                return log;
            }

            // --- Phase 1: resolve/create the 3 engine ComponentTypes ---
            var f5TypeB = await GetOrCreateComponentTypeAsync(uow, F5TypeBPartNumber, F5TypeBNomenclature, F5AircraftManufacturerId, log);
            var f5TypeC = await GetOrCreateComponentTypeAsync(uow, F5TypeCPartNumber, F5TypeCNomenclature, F5AircraftManufacturerId, log);
            var ajetEng = await GetOrCreateComponentTypeAsync(uow, AJetEnginePartNumber, AJetEngineNomenclature, AJetAircraftManufacturerId, log);
            await uow.CompleteAsync(); // commit so the 3 new rows have real Ids before anything below uses them

            // --- Phase 2: link eligible positions (first-time only per type — never clobbers a manual edit) ---
            await LinkEligiblePositionsAsync(uow, f5TypeB, "F5", log);
            await LinkEligiblePositionsAsync(uow, f5TypeC, "F5", log);
            await LinkEligiblePositionsAsync(uow, ajetEng, "A-Jet", log);
            await uow.CompleteAsync();

            // --- Phase 3: receipt each physical engine ---
            var today = DateOnly.FromDateTime(DateTime.Today);
            int created = 0, skippedDuplicate = 0, skippedUnrecognized = 0;

            foreach (var row in Rows)
            {
                var componentType = (row.LegacyAcMainGroupId, row.EngType) switch
                {
                    (1, "TYPE B") => f5TypeB,
                    (1, "TYPE C") => f5TypeC,
                    (2, _) => ajetEng,
                    _ => null
                };
                if (componentType == null)
                {
                    log.Add($"SKIPPED EngineId={row.EngineId} SN={row.SerialNumber} — unrecognized combination (AcMainGroupID={row.LegacyAcMainGroupId}, Eng_Type={row.EngType ?? "NULL"}).");
                    skippedUnrecognized++;
                    continue;
                }

                var dto = new ComponentReceiptDto
                {
                    ComponentTypeId = componentType.Id,
                    SerialNumber = row.SerialNumber,
                    StockBaseId = targetBase.Id,
                    EventDate = today,
                    Remarks = $"Import legacy (tblMeca_Engines) — EngineId={row.EngineId}. Aucune donnée d'heures/cycles dans la source — valeurs initiales à zéro, à corriger via une passe de rattrapage.",
                    InitialValues = new List<ComponentInitialReadingValueFormDto>() // no legacy hours data available — see class doc comment
                };

                var result = await componentService.ReceiptAsync(dto, PerformedByUserId);
                if (!result.Success)
                {
                    // ReceiptAsync's own guard (ExistsSerialAsync) makes this safe
                    // to re-run — a duplicate just no-ops here, it never throws.
                    log.Add($"SKIPPED EngineId={row.EngineId} SN={row.SerialNumber} ({componentType.PartNumber}) — {result.Message}");
                    skippedDuplicate++;
                    continue;
                }

                // ReceiptAsync always creates Status=InStock (it has no
                // UnderRepair path of its own) — for Serviceable=0 rows, flip
                // the entity directly right after. Deliberate simplification:
                // this represents an EXISTING real-world state at import time,
                // not a live Remove-to-repair transition, so no extra
                // ComponentEvent is logged for it (there is nothing to log —
                // it was never actually Installed-then-Removed in reality as
                // far as this import knows).
                if (!row.Serviceable && result.Id.HasValue)
                {
                    var entity = await uow.Components.GetByIdAsync(result.Id.Value);
                    if (entity != null)
                    {
                        entity.Status = ComponentStatus.UnderRepair;
                        uow.Components.Update(entity);
                    }
                }

                created++;
            }

            await uow.CompleteAsync();

            log.Add($"DONE — {created} engine(s) receipted, {skippedDuplicate} skipped as already-imported, {skippedUnrecognized} skipped as unrecognized. Base='{targetBase.BaseCode}'.");
            log.Add("REMINDER — these engines have ZERO opening FH/Cycles (no source data for it). Do not trust due/overdue status for them until a real hours backfill is done.");
            return log;
        }

        private static async Task<ComponentType> GetOrCreateComponentTypeAsync(IUnitOfWork uow, string partNumber, string nomenclature, int aircraftManufacturerId, List<string> log)
        {
            var existing = (await uow.ComponentTypes.GetAllAsync()).FirstOrDefault(t => t.PartNumber == partNumber);
            if (existing != null)
            {
                log.Add($"ComponentType '{partNumber}' already exists (Id={existing.Id}, AircraftManufacturerId={existing.AircraftManufacturerId?.ToString() ?? "null"}) — reused as-is, not recreated/updated.");
                return existing;
            }

            var created = new ComponentType
            {
                PartNumber = partNumber,
                Nomenclature = nomenclature,
                TrackingMethod = ComponentTrackingMethod.HardTime,
                AircraftManufacturerId = aircraftManufacturerId,
                IsActive = true
            };
            uow.ComponentTypes.Add(created);
            log.Add($"Created ComponentType '{partNumber}' — '{nomenclature}' (TrackingMethod=HardTime, AircraftManufacturerId={aircraftManufacturerId}). AtaId left null — not in the legacy dataset, set via Edit if you have it.");
            return created;
        }

        /// <summary>
        /// Links componentType to every ComponentPosition whose Code starts
        /// with "ENG" on every AcType belonging to an AcMainGroup matching
        /// familyMatch (Code or Name, case-insensitive contains). Skips
        /// (does not clobber) if componentType already has ANY eligible
        /// position — so re-running this seeder after you've manually edited
        /// "Gérer les positions éligibles" never undoes that edit.
        /// </summary>
        private static async Task LinkEligiblePositionsAsync(IUnitOfWork uow, ComponentType componentType, string familyMatch, List<string> log)
        {
            var existingPositionIds = await uow.ComponentTypes.GetPositionIdsAsync(componentType.Id);
            if (existingPositionIds.Any())
            {
                log.Add($"SKIPPED position-linking for '{componentType.PartNumber}' — already has {existingPositionIds.Count} eligible position(s) (prior run or manual edit); left untouched.");
                return;
            }

            var mainGroups = await uow.AcMainGroups.GetAllAsync();
            var matchedGroups = mainGroups
                .Where(g => g.Code.Contains(familyMatch, StringComparison.OrdinalIgnoreCase)
                         || g.Name.Contains(familyMatch, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (!matchedGroups.Any())
            {
                log.Add($"SKIPPED position-linking for '{componentType.PartNumber}' — no AcMainGroup found matching '{familyMatch}'. Check the real Code/Name in Réglages.");
                return;
            }

            var matchedGroupIds = matchedGroups.Select(g => g.Id).ToHashSet();
            var acTypes = await uow.AcTypes.GetAllAsync();
            var relevantAcTypeIds = acTypes.Where(a => matchedGroupIds.Contains(a.AcMainGroupId)).Select(a => a.Id).ToHashSet();

            var allPositions = await uow.ComponentPositions.GetAllAsync();
            var engPositions = allPositions
                .Where(p => relevantAcTypeIds.Contains(p.AcTypeId) && p.Code.StartsWith("ENG", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!engPositions.Any())
            {
                log.Add($"SKIPPED position-linking for '{componentType.PartNumber}' — matched AcMainGroup(s) [{string.Join(", ", matchedGroups.Select(g => g.Code))}] but found NO ENG*-coded ComponentPosition on any of their AcTypes. Create the position(s) first (ComponentPositions/Tree), then re-run this seeder.");
                return;
            }

            await uow.ComponentTypes.SetPositionsAsync(componentType.Id, engPositions.Select(p => p.Id));
            log.Add($"Linked '{componentType.PartNumber}' to {engPositions.Count} position(s): {string.Join(", ", engPositions.Select(p => $"{p.Code}@AcType#{p.AcTypeId}"))}.");
        }
    }
}

// ============================================================================
// HOW TO RUN — temporary, one-time. Do NOT leave this wired into every
// startup. In Program.cs, right after your existing seeding block (near the
// other *.SeedAsync(...) calls), add:
//
//     using (var importScope = app.Services.CreateScope())
//     {
//         var uow = importScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//         var componentService = importScope.ServiceProvider.GetRequiredService<IComponentService>();
//         var importLog = await LegacyEngineImportSeeder.SeedAsync(uow, componentService);
//         foreach (var line in importLog) Console.WriteLine("[LegacyEngineImport] " + line);
//     }
//
// Run the app once, read the console output (it tells you exactly what was
// created/skipped/aborted), then DELETE that block from Program.cs so it
// never runs again on a normal startup.
// ============================================================================
