namespace Catalog.Contracts.Dto.Approval
{
    public record ApproveDto
    {
        public required string WorkflowCode { get; set; }
        public required string UserName { get; set; }
    }
}
