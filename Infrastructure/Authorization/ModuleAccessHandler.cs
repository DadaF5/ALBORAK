// Authorization/ModuleAccessHandler.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Authorization
{
    public class ModuleAccessHandler : AuthorizationHandler<ModuleAccessRequirement>
    {
        private readonly IUnitOfWork _uow;
        public ModuleAccessHandler(IUnitOfWork uow) => _uow = uow;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, ModuleAccessRequirement requirement)
        {
            // Admin always bypasses — single hardcoded escape hatch so a bug
            // anywhere else in this system can never lock out every user.
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return; // not authenticated — fail closed

            var assignments = await _uow.UserAssignments.GetActiveByUserIdAsync(userId);

            var hasAccess = assignments.Any(a =>
                a.IsBaseAdmin || // Base Admin sees every module for their base
                (a.ModuleRole != null && a.ModuleRole.ModuleCode == requirement.ModuleCode
                    && (!requirement.RequireWrite || a.ModuleRole.CanWrite || a.IsBaseAdmin)));

            if (hasAccess)
                context.Succeed(requirement);

            // No explicit context.Fail() — leaving it unsucceeded is enough,
            // and Fail() would short-circuit any OTHER handler for the same
            // requirement that might later be added (e.g. a future
            // service-account bypass). Fail-closed by default either way.
        }
    }
}