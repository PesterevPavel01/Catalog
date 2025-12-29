namespace Catalog.Contracts.Dto.Events
{
    public sealed record OrderEventDto(string UserName, string OrderTitle, string orderCode, string Title, DateTime CreatedAt)
    {

    }
}
