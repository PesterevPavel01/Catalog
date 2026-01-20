namespace Catalog.Contracts.Dto.Exchange
{
    public record class ExportedEntitiesDto(String Type, IEnumerable<string> Codes){}
}
