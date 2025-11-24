namespace Catalog.Contracts.Dto.Message
{
    public sealed record MessageDto
    {
        public required string OrderCode { get; set; }
        public required string ModuleCode { get; set; }
        public required string Text { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required IEnumerable<string> SenderRoles { get; set; }
    }

    public sealed record CreateMessageDto
    {
        public required string OrderCode { get; set; }
        public required string ModuleCode { get; set; }
        public required string Text { get; set; }
        public required string SenderName { get; set; }
    }
}
