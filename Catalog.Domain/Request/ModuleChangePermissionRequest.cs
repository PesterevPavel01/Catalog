using Catalog.Contracts.Dto.Module;

namespace Catalog.Contracts.Request
{
    public sealed class ModuleChangePermissionRequest
    {
        public ModuleChangePermissionRequest(){}

        public ModuleChangePermissionRequest(ModuleDto moduleDto)
        {
            ModuleDto = moduleDto;
        }

        public ModuleDto ModuleDto { get; set; }
    }
}
