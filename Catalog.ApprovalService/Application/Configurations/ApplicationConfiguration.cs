using Catalog.Contracts.Entities.Configurations;

namespace Catalog.ApprovalService.Application.Configurations
{
    public sealed class ApplicationConfiguration
    {
        public IEnumerable<ApprovalWorkflowMapItem> ApprovalWorkflowMap { get; set; }
    }
}
