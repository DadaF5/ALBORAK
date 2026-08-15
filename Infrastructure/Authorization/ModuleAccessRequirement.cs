// Authorization/ModuleAccessRequirement.cs
using Microsoft.AspNetCore.Authorization;

namespace FRAProject.Authorization
{
    public class ModuleAccessRequirement : IAuthorizationRequirement
    {
        public string ModuleCode { get; }
        public bool RequireWrite { get; }

        public ModuleAccessRequirement(string moduleCode, bool requireWrite = false)
        {
            ModuleCode = moduleCode;
            RequireWrite = requireWrite;
        }
    }
}