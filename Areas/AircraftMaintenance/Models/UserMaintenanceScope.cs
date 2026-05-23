namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Runtime (non-persisted) computed scope for a maintenance user.
    ///
    /// Built from the active UserMaintenanceAssignment + any active UserMaintenanceAssignmentGroups.
    ///
    /// Usage:
    ///   Resolved once per request (e.g. via a scoped service or controller helper).
    ///   Used to filter read/write access in controllers and service methods.
    ///
    /// Security behavior by role:
    ///
    ///   Technician (Code = "TECH"):
    ///     - READ  : own work-order sign-offs only.
    ///     - WRITE : sign off on task cards assigned to self.
    ///     - Scope : BaseId + PrimaryAcMainGroupId only (no extra groups).
    ///
    ///   Base Supervisor (Code = "BASE_SUP"):
    ///     - READ  : all work orders within their Base and any ActiveAcMainGroupIds.
    ///     - WRITE : create/update/close work orders for their base/group scope.
    ///     - Scope : BaseId + PrimaryAcMainGroupId + AdditionalAcMainGroupIds.
    ///
    ///   Master Supervisor (Code = "MASTER_SUP"):
    ///     - READ  : all work orders across all bases and groups.
    ///     - WRITE : full access (create, reassign, close, override).
    ///     - Scope : no filter — sees everything.
    ///
    /// Active assignment resolution logic:
    ///   1. Load the single UserMaintenanceAssignment where:
    ///        UserId = current user  AND  IsActive = true
    ///        AND (EffectiveTo IS NULL OR EffectiveTo >= today)
    ///   2. Load all UserMaintenanceAssignmentGroups for that assignment where:
    ///        (EffectiveTo IS NULL OR EffectiveTo >= today)
    ///   3. ActiveAcMainGroupIds = { PrimaryAcMainGroupId } ∪ { all additional group IDs }.
    ///
    /// Historical tracking:
    ///   All past assignments remain in UserMaintenanceAssignment (IsActive = false or EffectiveTo &lt; today).
    ///   Query history with: WHERE UserId = @userId ORDER BY EffectiveFrom DESC.
    /// </summary>
    public class UserMaintenanceScope
    {
        public string UserId { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;

        // =========================================
        // Active assignment snapshot
        // =========================================
        public int AssignmentId { get; init; }
        public DateTime AssignmentEffectiveFrom { get; init; }

        // =========================================
        // Organizational scope
        // =========================================
        public int BaseId { get; init; }
        public string BaseName { get; init; } = string.Empty;

        public int PrimaryAcMainGroupId { get; init; }
        public string PrimaryAcMainGroupName { get; init; } = string.Empty;

        /// <summary>
        /// Union of PrimaryAcMainGroupId and all currently-active additional group IDs.
        /// Use this list for data access filtering.
        /// </summary>
        public IReadOnlyList<int> ActiveAcMainGroupIds { get; init; } = Array.Empty<int>();
        public IReadOnlyList<string> ActiveAcMainGroupNames { get; init; } = Array.Empty<string>();

        // =========================================
        // Role
        // =========================================
        public int MaintenanceRoleId { get; init; }
        public string RoleCode { get; init; } = string.Empty;
        public string RoleName { get; init; } = string.Empty;

        // =========================================
        // Role helpers
        // =========================================
        public bool IsTechnician => RoleCode == "TECH";
        public bool IsBaseSupervisor => RoleCode == "BASE_SUP";
        public bool IsMasterSupervisor => RoleCode == "MASTER_SUP";

        /// <summary>
        /// True when the user has any active maintenance assignment.
        /// False indicates the user is not currently assigned to maintenance.
        /// </summary>
        public bool HasActiveAssignment => AssignmentId > 0;
    }
}
