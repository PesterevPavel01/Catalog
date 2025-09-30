namespace Catalog.Contracts.Dto.Message
{
    public sealed record MessageDto
    {
            public required string OrderItemCode { get; set; }
            public required string Text { get; set; }
            public required DateTime CreatedAt { get; set; }
            public required IEnumerable<string> SenderRoles { get; set; }
    }
}
