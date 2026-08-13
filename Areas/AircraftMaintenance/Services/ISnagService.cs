// Areas/AircraftMaintenance/Services/ISnagService.cs
namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public interface ISnagService
    {
        // --- Report ---
        Task<(bool Success, string Message, int? SnagId)> ReportAsync(SnagCreateDto dto, string reportedByUserId);

        // --- Follow-up ---
        Task<(bool Success, string Message)> UpdateAsync(int id, SnagUpdateDto dto);
        Task<(bool Success, string Message)> LinkToWorkOrderAsync(int snagId, int workOrderId);
        Task<(bool Success, string Message)> DeferAsync(int snagId, SnagDeferralDto dto, string authorizedByUserId);

        // --- Close ---
        Task<(bool Success, string Message)> CloseAsync(int snagId, string closedByUserId);

        // Called from WorkOrder.Close() when WOKind == CORRECTIVE —
        // closes every linked Snag not already closed, sets ResolvedOnClose = true
        Task CloseLinkedSnagsAsync(int workOrderId, string closedByUserId);
    }
}