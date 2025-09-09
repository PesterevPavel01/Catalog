namespace Catalog.Contracts.Entities.Configurations
{
    public sealed class ApprovalWorkflowMapItem
    {
        public string ApprovalStageCode { get; set; }
        public int Position {  get; set; }
        public IEnumerable<string> AllowedApproverRoleCodes { get; set; }
    }
}
