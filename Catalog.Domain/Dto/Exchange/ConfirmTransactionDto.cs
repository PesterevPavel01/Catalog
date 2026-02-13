namespace Catalog.Contracts.Dto.Exchange
{
    public sealed record SyncConfirmationDto
    {
        public required string SyncSessionCode { get; set; } = null!;
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public IEnumerable<RejectedEntity> RejectedEntities { get; set; } = [];
        public IEnumerable<ExternalEntityMappingDto> SuccessfullySynced { get; set; } = [];
    }

    public sealed record ExternalEntityMappingDto(string SourceCode, string ExternalCode);

    public sealed record RejectedEntity (string Code, string? Error);
}
