using Catalog.Contracts.Dto.Message;

namespace Catalog.Contracts.Dto.Order
{
    public sealed record CommonOrderDto
    {
        public required string Title { get; set; } 
        public required string Code { get; set; }
        public required string UserName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required bool IsCustom {  get; set; }
        public required bool IsApprovalCompleted {  get; set; }
        public required bool IsCompleted { get; set; }
        public IEnumerable<MessageDto> Messages { get; set; } = [];
        public required string Status { get; set; }
    }
}
