namespace Catalog.Contracts.Dto.Exchange
{
    public sealed class SyncConfirmationDto
    {
        public string SyncSessionCode { get; set; } = null!;
        public IEnumerable<string> RejectedCodes { get; set; } = [];
        public IEnumerable<ExternalEntityMappingDto> SuccessfullySynced { get; set; } = [];
    }

    public sealed record ExternalEntityMappingDto(string SourceCode, string ExternalCode);
}
