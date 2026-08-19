// Areas/AircraftMaintenance/Services/SnagService.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public class SnagService : ISnagService
    {
        private readonly IUnitOfWork _uow;
        public SnagService(IUnitOfWork uow) => _uow = uow;

        public async Task<(bool, string, int?)> ReportAsync(SnagCreateDto dto, string reportedByUserId)
        {
            var snagNumber = await _uow.Snags.GetNextSnagNumberAsync(dto.DiscoveryDate.Year);

            var snag = new Snag
            {
                SnagNumber = snagNumber,
                AircraftId = dto.AircraftId,
                AtaId = dto.AtaId,
                Severity = dto.Severity,
                Impact = dto.Impact,
                ReportedBy = dto.ReportedBy,
                DiscoveryPhase = dto.DiscoveryPhase,
                DiscoveredDuringWorkOrderId = dto.DiscoveredDuringWorkOrderId,
                DiscoveryFH = dto.DiscoveryFH,
                DiscoveryCycles = dto.DiscoveryCycles,
                DiscoveryDate = dto.DiscoveryDate,
                DiscoveryBaseId = dto.DiscoveryBaseId,
                Description = dto.Description,
                Status = SnagStatus.OPEN
            };

            await _uow.Snags.AddAsync(snag);
            await _uow.CompleteAsync();
            return (true, "Snag signalé.", snag.Id);
        }

        public async Task<(bool, string)> LinkToWorkOrderAsync(int snagId, int workOrderId)
        {
            var snag = await _uow.Snags.GetByIdAsync(snagId);
            if (snag == null) return (false, "Snag introuvable.");
            if (snag.Status == SnagStatus.CLOSED) return (false, "Snag déjà clôturé.");

            await _uow.WorkOrderSnags.AddAsync(new WorkOrderSnag { WorkOrderId = workOrderId, SnagId = snagId });
            snag.LinkedWorkOrderId = workOrderId;
            snag.Status = SnagStatus.LINKED;
            _uow.Snags.Update(snag);
            await _uow.CompleteAsync();
            return (true, "Snag lié à l'OT.");
        }

        public async Task<(bool, string)> DeferAsync(int snagId, SnagDeferralDto dto, string authorizedByUserId)
        {
            var snag = await _uow.Snags.GetByIdAsync(snagId);
            if (snag == null) return (false, "Snag introuvable.");
            if (snag.Status == SnagStatus.CLOSED) return (false, "Snag déjà clôturé.");

            snag.IsDeferred = true;
            snag.DeferralReference = dto.DeferralReference;
            snag.DeferralLimitFH = dto.DeferralLimitFH;
            snag.DeferralLimitCycles = dto.DeferralLimitCycles;
            snag.DeferralLimitDate = dto.DeferralLimitDate;
            snag.DeferralAuthorizedByUserId = authorizedByUserId;
            snag.DeferralAuthorizedAt = DateTime.UtcNow;
            snag.Status = SnagStatus.DEFERRED;
            snag.Impact = AirworthinessImpact.RESTRICTED; // Red Dash

            _uow.Snags.Update(snag);
            await _uow.CompleteAsync();
            return (true, "Report autorisé (Red Dash).");
        }

        public async Task<(bool, string)> CloseAsync(int snagId, string closedByUserId)
        {
            var snag = await _uow.Snags.GetByIdAsync(snagId);
            if (snag == null) return (false, "Snag introuvable.");

            // ⚠ Previously missing — LinkToWorkOrderAsync and DeferAsync both
            // guard against acting on an already-closed snag, but CloseAsync
            // didn't. Re-closing an already-closed snag silently "succeeded"
            // and overwrote ClosedAt/ClosedByUserId with the new call's
            // values, destroying the original closure record (who actually
            // closed it, and when) with no trace it had happened.
            if (snag.Status == SnagStatus.CLOSED) return (false, "Snag déjà clôturé.");

            snag.Status = SnagStatus.CLOSED;
            snag.Impact = AirworthinessImpact.NONE;
            snag.ClosedAt = DateTime.UtcNow;
            snag.ClosedByUserId = closedByUserId;

            _uow.Snags.Update(snag);
            await _uow.CompleteAsync();
            return (true, "Snag clôturé.");
        }

        public async Task CloseLinkedSnagsAsync(int workOrderId, string closedByUserId)
        {
            var links = await _uow.WorkOrderSnags.GetByWorkOrderAsync(workOrderId);
            foreach (var link in links)
            {
                var snag = await _uow.Snags.GetByIdAsync(link.SnagId);
                if (snag == null || snag.Status == SnagStatus.CLOSED) continue;

                snag.Status = SnagStatus.CLOSED;
                snag.Impact = AirworthinessImpact.NONE;
                snag.ClosedAt = DateTime.UtcNow;
                snag.ClosedByUserId = closedByUserId;
                link.ResolvedOnClose = true;
                _uow.Snags.Update(snag);
            }
            await _uow.CompleteAsync(); // single round-trip, called from WorkOrder.Close()
        }

        public async Task<(bool, string)> UpdateAsync(int id, SnagUpdateDto dto)
        {
            var snag = await _uow.Snags.GetByIdAsync(id);
            if (snag == null) return (false, "Snag introuvable.");

            if (dto.Severity.HasValue) snag.Severity = dto.Severity.Value;
            if (dto.Impact.HasValue) snag.Impact = dto.Impact.Value;
            if (dto.Description != null) snag.Description = dto.Description;

            _uow.Snags.Update(snag);
            await _uow.CompleteAsync();
            return (true, "Snag mis à jour.");
        }
    }
}
