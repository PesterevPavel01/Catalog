namespace Catalog.Contracts.Response
{
    public sealed class ModuleChangePermissionResponse
    {
        public ModuleChangePermissionResponse() { }

        public ModuleChangePermissionResponse(string? moduleCode, bool result, string? error = null)
        {
            ModuleCode = moduleCode;
            Result = result;
            Error = error;
        }

        public string? ModuleCode { get; set; }
        public bool Result { get; set; }
        public string? Error { get; set; }
    }
}
